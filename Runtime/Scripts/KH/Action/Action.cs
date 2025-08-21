using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {

	public delegate void ActionFinishedHandler(Action action);

	public abstract class Action : MonoBehaviour {
		public event ActionFinishedHandler FinishedAction;

		public abstract void Begin();

		protected void Finished() {
			FinishedAction?.Invoke(this);
		}

        public IEnumerator BeginAndAwait() {
            bool finished = false;

            void handler(Action action) {
                if (action == this) finished = true;
            }

            // Have to subscribe before finishing in case the action completes immediately.
            FinishedAction += handler;
            Begin();

            // Only yield if not already finished.
            if (!finished) {
                while (!finished) {
                    yield return null;
                }
            }

            FinishedAction -= handler;
        }
    }
}