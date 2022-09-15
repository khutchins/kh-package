using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Texts {
    public delegate void LineCallback();

    public class LineSpec {
        public string Speaker;
		public Color SpeakerColor = Color.white;
		public string Line;
        public LineCallback Callback;

		public LineSpec(string speaker, string line, LineCallback callback = null) {
			Speaker = speaker;
			Line = line;
			Callback = callback;
		}

		public LineSpec(string speaker, string line, Color speakerColor, LineCallback callback = null) {
			Speaker = speaker;
			Line = line;
			Callback = callback;
			SpeakerColor = speakerColor;
		}
	}
}