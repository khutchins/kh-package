using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
    public class ValueReference<T> : ScriptableObject {

        public delegate void OnValueChanged(T newValue);

        public OnValueChanged ValueChanged;

        [SerializeField]
        private T _value;

        public T Value {
            get {
                return _value;
            }
            set {
                _value = value;
                ValueChanged?.Invoke(Value);
            }
        }
    }
}