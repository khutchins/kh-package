using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A generic ScriptableObject class that acts as a database for any type of Unity ScriptableObject.
/// </summary>
/// <typeparam name="T">The type of the ScriptableObject.</typeparam>
public abstract class RegistrySO<T> : ScriptableObject where T : ScriptableObject {

    [SerializeField] List<T> _items;

    private Dictionary<string, T> _itemsByName;

    public IEnumerable<string> AllItemNames => _itemsByName.Keys;

    private void OnEnable() {
        RebuildDictionary();
    }

    public void RebuildDictionary() {
        _itemsByName = (_items ?? new List<T>()).Where(x => x != null).ToDictionary(x => x.name, x => x, StringComparer.Ordinal);
    }

    /// <summary>
    /// Finds and returns an item by its asset name.
    /// </summary>
    public T GetItem(string name) {
        _itemsByName.TryGetValue(name, out var item);
        return item;
    }

#if UNITY_EDITOR
    public List<T> Items {
        set => _items = value;
    }
#endif
}