using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KH.Texts {
    public delegate void ItemProcessed<T>(IHandlerQueueItem<T> item);

    public interface IHandlerQueueItem<T> {
        event ItemProcessed<T> OnItemProcessed;
    }

    public class HandlerQueue<T> : ScriptableObject where T : IHandlerQueueItem<T> {

        public delegate void ItemAdded();
        public delegate void FirstItemAdded();

        public event ItemAdded OnItemAdded;
        public event FirstItemAdded OnFirstItemAdded;

        protected Queue<T> _items = new Queue<T>();

        public void Clear() {
            _items.Clear();
        }

        public IEnumerator EnqueueAndAwait(params T[] specs) {
            yield return new Awaiter(specs).WaitForLineToFinish(this);
        }

        public IEnumerator EnqueueAllAndAwait(IEnumerable<T> specs) {
            yield return new Awaiter(specs).WaitForLineToFinish(this);
        }

        public void EnqueueAll(IEnumerable<T> specs) {
            foreach (T spec in specs) {
                Enqueue(spec);
            }
        }

        public void Enqueue(params T[] specs) {
            foreach (T spec in specs) {
                Enqueue(spec);
            }
        }

        public void Enqueue(T spec) {
            bool wasEmpty = _items.Count == 0;
            _items.Enqueue(spec);
            if (wasEmpty) OnFirstItemAdded?.Invoke();
            OnItemAdded?.Invoke();
        }

        public T Dequeue() {
            if (_items.Count <= 0) {
                return default;
            }

            return _items.Dequeue();
        }

        class Awaiter {
            private bool _waiting = true;
            private List<T> _specs;

            public Awaiter(params T[] specs) : this((IEnumerable<T>)specs) { }

            public Awaiter(IEnumerable<T> specs) {
                _specs = specs.ToList();
                if (_specs.Count == 0) {
                    _waiting = false;
                    return;
                }

                T last = _specs[_specs.Count - 1];

                last.OnItemProcessed += ItemProcessed;
            }

            private void ItemProcessed(IHandlerQueueItem<T> item) {
                _waiting = false;
                item.OnItemProcessed -= ItemProcessed;
            }

            public IEnumerator WaitForLineToFinish(HandlerQueue<T> queue) {
                queue.EnqueueAll(_specs);
                while (_waiting) yield return null;
            }
        }
    }
}