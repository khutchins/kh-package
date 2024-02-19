using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Texts {

    public class LineSpec : IHandlerQueueItem<LineSpec> {
		public delegate void LineCallback();
		public event ItemProcessed<LineSpec> OnItemProcessed;
		public event LineCallback OnLineFinished;

		public string Speaker;
		public Color SpeakerColor = Color.white;
		public string Line;

		private LineCallback _callback;

		[Obsolete("Callback is deprecated. Please set on constructor or OnLineFinished instead.")]
		public LineCallback Callback {
			get => _callback;
			set {
				if (_callback != null) {
					OnLineFinished -= _callback;
				}
				_callback = value;
				OnLineFinished += _callback;
			}
		}

		public LineSpec(string speaker, string line, LineCallback callback = null) : this(speaker, line, Color.white, callback) { }

		public LineSpec(string speaker, string line, Color speakerColor, LineCallback callback = null) {
			Speaker = speaker;
			Line = line;
			SpeakerColor = speakerColor;
			_callback = callback;
			if (callback != null) {
				OnLineFinished += callback;
			}
		}

		public void LineFinished() {
			OnLineFinished?.Invoke();
			OnItemProcessed?.Invoke(this);
		}
	}
}