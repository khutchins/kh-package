using KH.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
    public class TriggerAction : Trigger {
        [SerializeField]
        public Action Action;

        protected override void PlayerEnteredInternal(IInteractionLockController mouseLook) {
            Action.Begin();
        }

        protected override void PlayerLeftInternal(IInteractionLockController mouseLook) {
        }
    }
}