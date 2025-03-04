using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.KVBDSL {
    public class DeserializeHelper {
        private readonly Dictionary<string, object> _source;

        public DeserializeHelper(Dictionary<string, object> source) { _source = source; }

        /// <summary>
        /// Try to get the value of the dictionary, returning the value if it's the
        /// expected type and non-null, otherwise defaultValue.
        /// </summary>
        /// <returns></returns>
        public T TryGet<T>(string key, T defaultValue) {
            if (!_source.TryGetValue(key, out var value) && value != null && value is T) {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Try to get the value of the dictionary, returning the value if it's the
        /// expected type and non-null, otherwise runs defaultValue to get the value.
        /// </summary>
        /// <returns></returns>
        public T TryGet<T>(string key, Func<T> defaultValue) {
            if (!_source.TryGetValue(key, out var value) && value != null && value is T) {
                return (T)value;
            }
            return defaultValue();
        }

        /// <summary>
        /// Passes the extracted value to objectInterpreter, which can either translate that
        /// to a T or return null, depending on if it's valid or not. If key does not exist
        /// or it's invalid, returns defaultValue.
        /// </summary>
        public T TryGet<T>(string key, Func<object, T> objectInterpreter, T defaultValue) {
            if (_source.TryGetValue(key, out var value)) {
                var interpreted = objectInterpreter(value);
                if (interpreted != null) {
                    return interpreted;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Passes the extracted value to objectInterpreter, which can either translate that
        /// to a T or return null, depending on if it's valid or not. If key does not exist
        /// or it's invalid, runs defaultValue to get the value.
        /// </summary>
        public T TryGet<T>(string key, Func<object, T> objectInterpreter, Func<T> defaultValue) {
            if (_source.TryGetValue(key, out var value)) {
                var interpreted = objectInterpreter(value);
                if (interpreted != null) {
                    return interpreted;
                }
            }

            return defaultValue();
        }
    }
}