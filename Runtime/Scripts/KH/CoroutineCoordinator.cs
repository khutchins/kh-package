using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    public class CoroutineCoordinator {

        private int _coroutineCount;
        private int _coroutineTotal;
        private List<IEnumerator> _enumerators = new List<IEnumerator>();

        public static IEnumerator RunAll(MonoBehaviour runner, List<IEnumerator> enumerators) {

            CoroutineCoordinator coord = new CoroutineCoordinator(enumerators);
            coord.Run(runner);
            while (!coord.IsOver) yield return null;
        }

        public CoroutineCoordinator(List<IEnumerator> coroutines) {
            _coroutineCount = 0;
            _coroutineTotal = coroutines.Count;
            _enumerators = coroutines;
        }

        private bool IsOver {
            get => _coroutineCount == _coroutineTotal;
        }

        private void Run(MonoBehaviour runner) {
            foreach (IEnumerator enumerator in _enumerators) {
                runner.StartCoroutine(wrapped(enumerator));
            }
        }

        private IEnumerator wrapped(IEnumerator coroutine) {
            yield return coroutine;
            _coroutineCount++;
        }
    }
}