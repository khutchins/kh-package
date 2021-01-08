using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Texts {
    public delegate void LineCallback();

    public class LineSpec {
        public string Speaker;
        public string Line;
        public LineCallback Callback;

		public LineSpec(string speaker, string line, LineCallback callback = null) {
			Speaker = speaker;
			Line = line;
			Callback = callback;
		}
	}
}