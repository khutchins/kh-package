using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Tools {
    public class MeshMerge : MonoBehaviour {
        bool _alreadyMerged;

        void Awake() {
            Merge();
        }

        void Merge() {
            if (_alreadyMerged) {
                Debug.LogWarning($"Meshes already merged for {this.gameObject.name}!");
                return;
            }

            MergeAll(this.gameObject);
            _alreadyMerged = true;
        }

        private class CombinableObject {
            public readonly GameObject Obj;
            public readonly MeshFilter Filter;
            public readonly MeshRenderer Renderer;

            public CombinableObject(GameObject obj, MeshFilter filter, MeshRenderer renderer) {
                Obj = obj;
                Filter = filter;
                Renderer = renderer;
            }
        }

        private class Submesh {
            public readonly CombinableObject Obj;
            public readonly int SubMeshIdx;

            public Submesh(CombinableObject obj, int idx) {
                Obj = obj;
                SubMeshIdx = idx;
            }
        }

        private static List<CombinableObject> FindAllCombinableObjects(GameObject parent) {
            List<CombinableObject> objs = new List<CombinableObject>();
            foreach (MeshFilter filter in parent.GetComponentsInChildren<MeshFilter>()) {
                if (!filter.gameObject.activeInHierarchy || !filter.gameObject.activeSelf) continue;
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || !renderer.enabled) continue;
                if (filter.sharedMesh.subMeshCount > renderer.sharedMaterials.Length) {
                    Debug.LogWarning($"MeshFilter for {filter.gameObject} has more submeshes than materials. Skipping.");
                    continue;
                }

                objs.Add(new CombinableObject(filter.gameObject, filter, renderer));
            }
            return objs;
        }

        private static Dictionary<Material, List<Submesh>> ComputeMerges(IEnumerable<CombinableObject> objs) {
            var dict = new Dictionary<Material, List<Submesh>>();

            void EnsureAdd(Material mat, Submesh submesh) {
                dict.TryGetValue(mat, out List<Submesh> list);
                if (list == null) {
                    list = new List<Submesh>();
                    dict[mat] = list;
                }
                list.Add(submesh);
            }

            foreach (CombinableObject obj in objs) {
                obj.Renderer.enabled = false;
                int i = 0;
                for (i = 0; i < obj.Filter.sharedMesh.subMeshCount && i < obj.Renderer.sharedMaterials.Length; i++) {
                    EnsureAdd(obj.Renderer.sharedMaterials[i], new Submesh(obj, i));
                }
                for (; i < obj.Filter.sharedMesh.subMeshCount; i++) {
                    // Not sure when this can happen.
                    EnsureAdd(obj.Renderer.sharedMaterials[0], new Submesh(obj, i));
                }
                for (; i < obj.Renderer.sharedMaterials.Length; i++) {
                    // For some terrible reason, Unity's docs say that if materials > submeshes, the
                    // last submesh gets rendered with all the remaining materials. We'll preserve
                    // that behavior, I guess.
                    EnsureAdd(obj.Renderer.sharedMaterials[i], new Submesh(obj, obj.Filter.sharedMesh.subMeshCount - 1));
                }
            }
            return dict;
        }

        /// <summary>
        /// Combine mappings of materials to submeshes into meshes made up of single materials.
        /// </summary>
        /// <returns>A pair of (Material[], Mesh[]).</returns>
        private static (Material[] materials, Mesh[] meshes) SameMaterialCombines(Dictionary<Material, List<Submesh>> meshInfo) {
            int count = meshInfo.Count;
            var meshes = new Mesh[count];
            var materials = new Material[count];

            int i = 0;
            foreach (KeyValuePair<Material, List<Submesh>> pair in meshInfo) {
                List<Submesh> submeshes = pair.Value;

                var combine = new CombineInstance[submeshes.Count];
                int vertCount = 0;
                for (int j = 0; j < submeshes.Count; j++) {
                    combine[j].mesh = submeshes[j].Obj.Filter.sharedMesh;
                    combine[j].subMeshIndex = submeshes[j].SubMeshIdx;
                    combine[j].transform = submeshes[j].Obj.Filter.transform.localToWorldMatrix;
                    vertCount += submeshes[j].Obj.Filter.sharedMesh.vertexCount;
                }

                Mesh mesh = new Mesh();
                if (vertCount > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.CombineMeshes(combine);
                meshes[i] = mesh;
                materials[i] = pair.Key;
                ++i;
            }
            return (materials, meshes);
        }

        private static void FinalizeMesh(Mesh[] meshes, Material[] materials, MeshFilter filter, MeshRenderer renderer) {
            var combine = new CombineInstance[meshes.Length];

            for (int i = 0; i < meshes.Length; i++) {
                combine[i].mesh = meshes[i];
                combine[i].subMeshIndex = 0;
            }
            Mesh mesh = new Mesh();
            mesh.name = "Combined Mesh";
            mesh.CombineMeshes(combine, false, false);
            mesh.RecalculateBounds();

            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            renderer.staticShadowCaster = true;
            filter.sharedMesh = mesh;
        }

        public static void MergeAll(GameObject parent) {
            MergeAll(parent, parent);
        }

        public static void MergeAll(GameObject parent, GameObject objectToMergeInto) {
            if (parent.GetComponent<MeshFilter>() != null || parent.GetComponent<MeshRenderer>() != null) {
                Debug.LogWarning($"Can't merge into GO {objectToMergeInto.name}. It already has a filter or renderer.");
                return;
            }

            using (new ResetPosition(objectToMergeInto.transform)) {
                List<CombinableObject> objsToCombine = FindAllCombinableObjects(parent);
                if (objsToCombine.Count == 0) {
                    Debug.LogWarning("No valid objects to merge.");
                    return;
                }
                var materialMapping = ComputeMerges(objsToCombine);

                // Combine all meshes with the same material together.
                var (materials, meshes) = SameMaterialCombines(materialMapping);

                MeshFilter filter = parent.AddComponent<MeshFilter>();
                MeshRenderer renderer = parent.AddComponent<MeshRenderer>();

                // Combine all the combined meshes into one mesh with multiple and submeshes and materials.
                FinalizeMesh(meshes, materials, filter, renderer);
            }
        }

        private class ResetPosition : IDisposable {
            private readonly Transform _transform;
            private readonly Vector3 _cachedPos;
            private readonly Quaternion _cachedRot;
            private readonly Vector3 _cachedScale;

            public ResetPosition(Transform transform) {
                _transform = transform;
                _cachedPos = _transform.position;
                _cachedRot = _transform.rotation;
                _cachedScale = _transform.localScale;
                Apply(Vector3.zero, Quaternion.identity, Vector3.one);
            }

            public void Dispose() {
                Apply(_cachedPos, _cachedRot, _cachedScale);
            }

            private void Apply(Vector3 pos, Quaternion rot, Vector3 cachedScale) {
                _transform.position = pos;
                _transform.rotation = rot;
                _transform.localScale = cachedScale;
            }
        }
    }
}