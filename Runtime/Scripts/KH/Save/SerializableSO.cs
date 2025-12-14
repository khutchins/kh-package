using KH.KVBDSL;
using Ratferences;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KH.Save {
    enum NameType {
        ParameterName,
        ReferenceName
    }
    /// <summary>
    /// Base class for a scriptable object you want to serialize. Any References I can handle
    /// will automatically be serialized.
    /// </summary>
    public abstract class SerializableSO : ScriptableObject, ISavable {
        [Header("General Configuration")]
        [Tooltip("How to title names in the save file. ParameterName is the name of the parameter, ReferenceName is the name of the ScriptableObject.")]
        [SerializeField] NameType NameType;
        [Tooltip("Text asset in KVBDSL format to load defaults from.")]
        [SerializeField] TextAsset DefaultValues;

        public abstract string SavePath();
        public virtual void OnAfterLoad() { }

        private AttrInfo[] Attrs;

        void OnEnable() {
            PopulateFieldInfos();
        }

        private void PopulateFieldInfos() {
            FieldInfo[] infos = GetFieldInfos();
            List<AttrInfo> attrInfos = new List<AttrInfo>();
            foreach (FieldInfo field in infos) {
                var attr = field.GetCustomAttribute<SerializableSOAttribute>();
                var obj = field.GetValue(this);
                if (attr == null) {
                    if (obj is ValueReference) {
                        MaybeLogWarning($"Field {field.Name} with ValueReference isn't tagged with the [SerializableSO] attribute. Is this a mistake?");
                    }
                    continue;
                }

                if (obj == null || obj is not ValueReference so) {
                    MaybeLogWarning($"Invalid serializable {obj} for field {field.Name}. It must be a subclass of ValueReference.");
                    continue;
                }

                string name = NameForField(field);

                attrInfos.Add(new AttrInfo(name, obj as ValueReference, attr.SaveType));
            }
            Attrs = attrInfos.ToArray();
        }

        public void Save() {
            Dictionary<string, object> settings = ReadCurrentFieldValues();
            var text = new Serializer().Serialize(settings);
            IOHelper.EnsurePathAndWriteText(SavePath(), text);
        }

        public void Load() {
            LoadDefault();

            string path = SavePath();
            if (!File.Exists(path)) return;
            string text = File.ReadAllText(path);
            var dict = new Deserializer().Parse(text);
            SetCurrentFieldValues(dict);
        }

        public void LoadDefault() {
            if (DefaultValues == null) {
                MaybeLogWarning($"No default settings exist for {this.name}!");
                return;
            }
            var dict = new Deserializer().Parse(DefaultValues.text);
            SetCurrentFieldValues(dict);
        }

        private FieldInfo[] GetFieldInfos() {
            return this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        }

        private string NameForField(FieldInfo field) {
            if (NameType == NameType.ParameterName) {
                return field.Name;
            }

            var obj = field.GetValue(this);
            if (obj == null) {
                MaybeLogWarning($"Serializable field {name} has no value.");
                return "null";
            } else if (obj is ScriptableObject so) {
                return so.name;
            } else {
                MaybeLogWarning($"Value for serializable field {name} is not a Scriptable Object.");
                return "unknown";
            }
        }

        private AttrInfo FieldForName(string name) {
            return Attrs.FirstOrDefault(a => a.Name == name);
        }

        private Dictionary<string, object> ReadCurrentFieldValues() {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (FieldInfo field in GetFieldInfos()) {
                string fieldName = NameForField(field);
                var obj = field.GetValue(this);
                if (obj == null) {
                    MaybeLogWarning($"Field for name {field.Name} is unset. This is bad.");
                    continue;
                }
                if (obj is FloatReference fr) {
                    values[fieldName] = fr.Value;
                } else if (obj is IntReference ir) {
                    values[fieldName] = ir.Value;
                } else if (obj is StringReference sr) {
                    values[fieldName] = sr.Value;
                } else if (obj is BoolReference br) {
                    values[fieldName] = br.Value;
                } else if (obj is ColorReference cr) {
                    values[fieldName] = cr.Value;
                } else {
                    MaybeLogWarning($"Unsupported type {obj.GetType()}");
                }
            }
            return values;
        }

        private void SetCurrentFieldValues(Dictionary<string, object> values) {
            foreach (var entry in values) {
                AttrInfo info = FieldForName(entry.Key);
                if (info == null) {
                    MaybeLogWarning($"No field exists for key {entry.Key}. Skipping.");
                    continue;
                }
                ValueReference obj = info.Reference;
                if (obj == null) continue;
                if (entry.Value is float f && obj is FloatReference fr) {
                    fr.Value = f;
                } else if (entry.Value is int i && obj is IntReference ir) {
                    ir.Value = i;
                } else if (entry.Value is string s && obj is StringReference sr) {
                    sr.Value = s;
                } else if (entry.Value is bool b && obj is BoolReference br) {
                    br.Value = b;
                } else if (entry.Value is Color c && obj is ColorReference cr) { 
                    cr.Value = c; 
                } else {
                    MaybeLogWarning($"Entry type {entry.Value.GetType()} is not compatible with field type {obj.GetType()}");
                }
            }
        }

        public List<ValueReference> AllSerializedReferences() {
            return Attrs.Select(x => x.Reference).ToList();
        }

        public Ratferences.SaveType SaveTypeForReference(ValueReference reference) {
            var first = Attrs.Where(x => x.Reference == reference).FirstOrDefault();
            return first != null ? first.SaveType : Ratferences.SaveType.DoesntTrigger;
        }

        private class AttrInfo {
            public readonly string Name;
            public readonly ValueReference Reference;
            public readonly Ratferences.SaveType SaveType;

            public AttrInfo(string name, ValueReference reference, Ratferences.SaveType saveType) {
                Name = name;
                Reference = reference;
                SaveType = saveType;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MaybeLogWarning(string warning) {
#if UNITY_EDITOR
            Debug.LogWarning(warning);
#endif
        }

# if UNITY_EDITOR
        [ContextMenu("Create Default Settings Asset")]
        private void CreateDefaultTextAsset() {
            if (DefaultValues != null) {
                Debug.Log("Not creating default asset file because it's already hooked up and I assume that's a mistake. Clear it if you want to make a new file.");
                return;
            }
            string defaultPath = $"Assets/Misc/";
            if (!AssetDatabase.IsValidFolder(defaultPath)) {
                AssetDatabase.CreateFolder("Assets", "Misc");
            }
            string assetPath = $"Assets/Misc/{this.GetType().Name}.txt";
            if (File.Exists(assetPath)) {
                Debug.Log("File already exists. To replace, delete the original file first.");
                return;
            }
            File.WriteAllText(assetPath, new Serializer().Serialize(ReadCurrentFieldValues()));
            AssetDatabase.Refresh();
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null) {
                Debug.LogWarning("Something went wrong when creating the default file. It doesn't exist.");
                return;
            }
            DefaultValues = asset;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
            Debug.Log($"Created default settings at: {assetPath}");
        }

        [ContextMenu("Show File In Explorer")]
        private void OpenEditorToFile() {
            EditorUtility.RevealInFinder(SavePath());
        }

        [ContextMenu("Create Missing References as Sub-Assets")]
        private void CreateMissingReferences() {
            var fields = GetFieldInfos();
            bool changed = false;

            foreach (var field in fields) {
                if (!typeof(ValueReference).IsAssignableFrom(field.FieldType)) continue;

                var currentValue = field.GetValue(this) as ScriptableObject;

                if (currentValue == null) {
                    var newRef = ScriptableObject.CreateInstance(field.FieldType);
                    newRef.name = field.Name;

                    AssetDatabase.AddObjectToAsset(newRef, this);

                    field.SetValue(this, newRef);
                    changed = true;
                    Debug.Log($"Created sub-asset for: {field.Name}");
                }
            }

            if (changed) {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        [ContextMenu("Cleanup Unused Sub-Assets")]
        private void CleanupUnusedSubAssets() {
            string path = AssetDatabase.GetAssetPath(this);

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            HashSet<Object> referencedAssets = new HashSet<Object>();
            foreach (var field in GetFieldInfos()) {
                var val = field.GetValue(this) as Object;
                if (val != null) referencedAssets.Add(val);
            }

            bool changed = false;

            // Destroy unused subassets.
            foreach (var asset in allAssets) {
                if (asset == this) continue;

                if (asset is ValueReference && !referencedAssets.Contains(asset)) {
                    Debug.Log($"Deleting unused sub-asset: {asset.name}");
                    DestroyImmediate(asset, true);
                    changed = true;
                }
            }

            if (changed) {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path);
            } else {
                Debug.Log("No unused sub-assets found.");
            }
        }
#endif
    }
}