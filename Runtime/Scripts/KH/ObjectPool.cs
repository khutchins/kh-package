using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {
    public static ObjectPool Instance;
    private static readonly bool DEBUG = false;

    private Dictionary<GameObject, List<GameObject>> _pool = new Dictionary<GameObject, List<GameObject>>();

    private void Awake() {
        Instance = this;
    }

    public GameObject GetObject(GameObject prefab) {
        List<GameObject> list;
        if (!_pool.ContainsKey(prefab)) {
            list = new List<GameObject>();
            _pool[prefab] = list;
        } else {
            list = _pool[prefab];
        }

        foreach (GameObject item in list) {
            if (!item.activeInHierarchy) {
                if (DEBUG) Debug.LogFormat("Cache hit for {0}", prefab);
                return item;
            }
        }
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        list.Add(obj);
        if (DEBUG) Debug.LogFormat("Expanded pool for {0} to {1}", prefab, list.Count);
        return obj;
    }

    public GameObject GetObject(GameObject prefab, Vector3 loc, Quaternion rot, Transform parent = null, bool local = false) {
        GameObject go = GetObject(prefab);
        go.transform.SetParent(parent);

        if (local) {
            go.transform.localPosition = loc;
            go.transform.localRotation = rot;
        } else {
            go.transform.position = loc;
            go.transform.rotation = rot;
        }
        return go;
    }
}
