using UnityEngine;

namespace KH {
	public static class AnimationCurves {

		private static float PI = Mathf.PI;
		private static float PI_2 = Mathf.PI / 2F;

		public static float Linear(float t) {
			return Mathf.Clamp(t, 0, 1);
	    }

		public static float Sin(float t) {
			return Mathf.Sin(t * 2 * PI);
		}

		public static float Cos(float t) {
			return Mathf.Cos(t * 2 * PI);
		}

		public static float SinEaseOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return Mathf.Sin(t * PI_2);
		}

		public static float SinEaseIn(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return Mathf.Sin((t - 1) * PI_2) + 1;
		}

		public static float SinEaseInOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return .5F * (1 - Mathf.Cos(t * PI));
		}

		public static float CubicEaseOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			t--;
			return (t * t * t + 1);
		}

		public static float CubicEaseInOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			if (t < 0.5) return 4 * t * t * t;
			else {
				float f = 2 * (t - 1);
				return .5f * f * f * f + 1;
			}
		}

		public static float CubicEaseIn(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return t * t * t;
		}

		public static float QuinticEaseOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			t--;
			return (t * t * t * t * t + 1);
		}

		public static float QuinticEaseIn(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return t * t * t * t * t;
		}

		public static float QuinticEaseInOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			if(t < 0.5) return 16 * t * t * t * t * t;
			else {
				float f = 2 * (t - 1);
				return .5f * f * f * f * f * f + 1;
			}
		}

		public static float ElasticEaseIn(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return Mathf.Sin(13 * PI_2 * t) * Mathf.Pow(2, 10 * (t - 1));
		}

		public static float ElasticEaseOut(float t) {
			t = Mathf.Clamp(t, 0, 1);
			return -1 * Mathf.Sin(13 * PI_2 * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
		}
	}
}
