using KH.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Script {
    [CreateAssetMenu(menuName = "KH/Script/Command Channel", fileName = "CommandChannel")]
    public class CommandChannel : ScriptableObject {
        public event Action<Command> OnRegister;
        public event Action<Command> OnUnregister;
        public event Action<object> OnUnregisterRegistrar;

        public void Register(Command command) {
            OnRegister?.Invoke(command);
        }

        public void UnregisterRegistrar(object registrar) {
            OnUnregisterRegistrar?.Invoke(registrar);
        }

        public void Unregister(Command command) {
            OnUnregister?.Invoke(command);
        }
    }
}