using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Array that contains max maxEntries entries and removes the oldest entry if the cap is reached.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FixedArray<T> {
    private List<T> _list;
    private readonly int _maxSize;
    private int _start;

    public FixedArray(int maxSize) {
        _maxSize = maxSize;
        _list = new List<T>(_maxSize);
        _start = 0;
    }

    public int Count {
        get { return _list.Count; }
    }

    public T this[int i] {
        get { return _list[(_start + _list.Count - 1 - i) % _list.Count]; }
    }

    public T Last {
        get {
            if (_list.Count == 0) return default;
            else if (_list.Count < _maxSize) return _list[^1];
            else return _list[(_start - 1 + _maxSize) % _maxSize];
        }
    }

    public void Add(T item) {
        if (_maxSize == 0) return;
        if (_list.Count == _maxSize) {
            _list[_start] = item;
            _start = (_start + 1) % _maxSize;
        } else {
            _list.Add(item);
        }
    }
}
