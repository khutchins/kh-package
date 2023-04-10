using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    /// <summary>
    /// Class that ensures that there's a single instance of a coroutine being run
    /// and cleans up after itself.
    /// </summary>
    public class SingleCoroutineManager {
        System.Action _onCancelCleanup;
        System.Action _onFinally;
        readonly MonoBehaviour _owner;

        private Coroutine _instance;

        /// <summary>
        /// Invokes a coroutine, enforcing that any preexisting one is cancelled by a new run.
        /// </summary>
        /// <param name="owner">Script responsible for the coroutines. What would be starting them itself.</param>
        public SingleCoroutineManager(MonoBehaviour owner) {
            _owner = owner;
        }

        public void StartCoroutine(IEnumerator routine, System.Action onFinally = null, System.Action onCancelCleanup = null) {
            MaybeCancelCoroutine();
            _onFinally = onFinally;
            _onCancelCleanup = onCancelCleanup;
            _instance = _owner.StartCoroutine(Coroutine(routine));
        }

        public void StopCoroutine() {
            MaybeCancelCoroutine();
        }

        private void MaybeCancelCoroutine() {
            if (_instance != null) {
                _owner.StopCoroutine(_instance);
                _instance = null;
                _onCancelCleanup?.Invoke();
                _onFinally?.Invoke();
            }
        }

        private IEnumerator Coroutine(IEnumerator routine) {
            yield return routine;
            _onFinally?.Invoke();
            _instance = null;
        }
    }
}