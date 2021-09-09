using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
	public static class EZTween {

		public static IEnumerator DoPercentAction(Action<float> action, float duration) {
			return DoPercentAction(action, duration, Curve.Linear);
		}

		public static IEnumerator DoPercentAction(Action<float> action, float duration, Func<float, float> curve) {
			float startTime = Time.time;

			float percent;
			while ((percent = (Time.time - startTime) / duration) < 1) {
				action(curve(percent));
				yield return null;
			}
			action(curve(1));
		}

		public static class Curve {
			static float PI = Mathf.PI;
			static float PI_2 = Mathf.PI / 2f;

			public static Func<float, float> Linear = (float t) => {
				return t;
			};

			public static Func<float, float> SinEaseOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return Mathf.Sin(t * PI_2);
			};

			public static Func<float, float> SinEaseIn = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return Mathf.Sin((t - 1) * PI_2) + 1;
			};

			public static Func<float, float> SinEaseInOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return .5F * (1 - Mathf.Cos(t * PI));
			};

			public static Func<float, float> CubicEaseOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				t--;
				return (t * t * t + 1);
			};

			public static Func<float, float> CubicEaseInOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				if (t < 0.5) return 4 * t * t * t;
				else {
					float f = 2 * (t - 1);
					return .5f * f * f * f + 1;
				}
			};

			public static Func<float, float> CubicEaseIn = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return t * t * t;
			};

			public static Func<float, float> QuinticEaseOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				t--;
				return (t * t * t * t * t + 1);
			};

			public static Func<float, float> QuinticEaseIn = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return t * t * t * t * t;
			};

			public static Func<float, float> QuinticEaseInOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				if (t < 0.5) return 16 * t * t * t * t * t;
				else {
					float f = 2 * (t - 1);
					return .5f * f * f * f * f * f + 1;
				}
			};

			public static Func<float, float> ElasticEaseIn = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return Mathf.Sin(13 * PI_2 * t) * Mathf.Pow(2, 10 * (t - 1));
			};

			public static Func<float, float> ElasticEaseOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return -1 * Mathf.Sin(13 * PI_2 * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
			};

			public static Func<float, float> Bezier = (float t) => {
				return t * t * (3f - 2f * t);
			};
		}
	}
}