using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
	public static class CoroutineHelpers {
		public static float PercentRange(float start, float end, float now) {
			return Mathf.Clamp((now - start) / (end - start), 0, 1);
		}

		public static float Percent(float start, float duration, float now) {
			return (now - start) / duration;
		}
	}

}