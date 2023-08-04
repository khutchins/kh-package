using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KH.Texts {
    public delegate void LineAdded();
    public delegate void FirstLineAdded();

    [CreateAssetMenu(menuName = "Text/Line Spec Queue")]
    public class LineSpecQueue : ScriptableObject {
        private Queue<LineSpec> _lineSpecs = new Queue<LineSpec>();

        public event LineAdded LineAdded;
        public event FirstLineAdded FirstLineAdded;

        public void Clear() {
            _lineSpecs.Clear();
        }

        public IEnumerator EnqueueAllAndAwait(IEnumerable<LineSpec> specs) {
            yield return new LineSpecAwaiter(specs).WaitForLineToFinish(this);
        }

        public void EnqueueAll(IEnumerable<LineSpec> specs) {
            foreach (LineSpec spec in specs) {
                Enqueue(spec);
            }
        }

        public IEnumerator EnqueueAndAwait(LineSpec spec) {
            yield return EnqueueAllAndAwait(new LineSpec[] { spec });
        }

        public void Enqueue(LineSpec spec) {
            bool firstLine = _lineSpecs.Count == 0;
            _lineSpecs.Enqueue(spec);
            if (firstLine) FirstLineAdded?.Invoke();
            LineAdded?.Invoke();
        }

        public LineSpec Dequeue() {
            if (_lineSpecs.Count <= 0) {
                return null;
            }

            return _lineSpecs.Dequeue();
        }

        class LineSpecAwaiter {
            private bool _waiting = true;
            private List<LineSpec> _specs;
            private LineCallback _existingCallback;

            public LineSpecAwaiter(IEnumerable<LineSpec> specs) {
                List<LineSpec> specList = specs.ToList();
                if (specList.Count == 0) {
                    _waiting = false;
                    return;
                }

                LineSpec last = specList[specList.Count - 1];
                _existingCallback = last.Callback;
                last.Callback = LineFinished;
            }

            public void LineFinished() {
                _waiting = false;
                _existingCallback?.Invoke();
            }

            public IEnumerator WaitForLineToFinish(LineSpecQueue queue) {
                queue.EnqueueAll(_specs);
                while (_waiting) yield return null;
            }
        }
    }
}