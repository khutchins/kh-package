using System.Collections;
using System.Collections.Generic;
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

        public void EnqueueAll(IEnumerable<LineSpec> specs) {
            foreach (LineSpec spec in specs) {
                Enqueue(spec);
			}
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
    }
}