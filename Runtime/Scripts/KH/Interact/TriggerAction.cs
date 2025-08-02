using KH.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
    public class TriggerAction : Trigger {
        [SerializeField]
        public Action Action;
        [Tooltip("Time it takes before the trigger can occur again. Negative means no cooldown.")]
        [SerializeField] float RetriggerTime = -1;

        private float _lastTriggerTime = int.MinValue;

        protected override void PlayerEnteredInternal(IInteractionLockController mouseLook) {
            if (Time.time - _lastTriggerTime < RetriggerTime) return;
            _lastTriggerTime = Time.time;
            Action.Begin();
        }

        protected override void PlayerLeftInternal(IInteractionLockController mouseLook) {
        }
    }
}
