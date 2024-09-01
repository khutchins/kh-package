using Ratferences;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace KH.Save {
    public class SerializableSOManager : SaveDelayer {
        [SerializeField] SerializableSO ThingToSave;

        private void Awake() {
            if (ThingToSave == null) {
                Debug.LogWarning("No hookup bound. Disabling.");
                this.enabled = false;
                return;
            }
            ThingToSave.Load();
        }

        void ValueUpdated(ValueReference value) {
            ScheduleSave(ThingToSave.SaveTypeForReference(value));
        }

        private void OnEnable() {
            if (ThingToSave == null) return;
            ThingToSave.AllSerializedReferences().ForEach(reference => {
                reference.ValueChangedSignal += ValueUpdated;
            });
        }

        private void OnDisable() {
            if (ThingToSave == null) return;
            ThingToSave.AllSerializedReferences().ForEach(reference => {
                reference.ValueChangedSignal -= ValueUpdated;
            });
        }

        protected override void PerformSave() {
            ThingToSave.Save();
        }
    }
}