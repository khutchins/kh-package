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

			public enum Type {
				Linear,
				SinEaseOut,
				SinEaseIn,
				SinEaseInOut,
				CubicEaseOut,
				CubicEaseIn,
				CubicEaseInOut,
				QuinticEaseOut,
				QuinticEaseIn,
				QuinticEaseInOut,
				ElasticEaseIn,
				ElasticEaseOut,
				EaseInBack,
				EaseOutBack,
				EaseInOutBack,
				Bezier
			}

			public static Func<float, float> CurveForType(Type type) {
				return type switch {
					Type.Linear => Linear,
					Type.SinEaseOut => SinEaseOut,
					Type.SinEaseIn => SinEaseIn,
					Type.SinEaseInOut => SinEaseInOut,
					Type.CubicEaseOut => CubicEaseOut,
					Type.CubicEaseIn => CubicEaseIn,
					Type.CubicEaseInOut => CubicEaseInOut,
					Type.QuinticEaseOut => QuinticEaseOut,
					Type.QuinticEaseIn => QuinticEaseIn,
					Type.QuinticEaseInOut => QuinticEaseInOut,
					Type.ElasticEaseIn => ElasticEaseIn,
					Type.ElasticEaseOut => ElasticEaseOut,
					Type.EaseInBack => EaseInBack,
					Type.EaseOutBack => EaseOutBack,
					Type.EaseInOutBack => EaseInOutBack,
					Type.Bezier => Bezier,
					_ => Linear,
				};
			}

			const float PI = Mathf.PI;
			const float PI_2 = Mathf.PI / 2f;
			const float PI_2_3 = Mathf.PI * 2f / 3f;

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
				return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * PI_2_3);
			};

			public static Func<float, float> ElasticEaseOut = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * PI_2_3) + 1;
			};

			const float C1 = 1.70158f;
			const float C2 = C1 * 1.525f;
			const float C3 = C1 + 1;

			public static Func<float, float> EaseInBack = (float t) => {
				t = Mathf.Clamp(t, 0, 1);
				return C3 * t * t * t - C1 * t * t;
			};

			public static Func<float, float> EaseOutBack = (float t) => {
				t = 1 - Mathf.Clamp(t, 0, 1);
				return 1 + C3 * t * t * t + C1 * t * t;
			};

			public static Func<float, float> EaseInOutBack = (float t) => {
				return t < 0.5f
					? (Mathf.Pow(2 * t, 2) * ((C2 + 1) * 2 * t - C2)) / 2f
					: (Mathf.Pow(2 * t - 2, 2) * ((C2 + 1) * (t * 2 - 2) + C2) + 2) / 2f;
			};

			public static Func<float, float> Bezier = (float t) => {
				return t * t * (3f - 2f * t);
			};
		}
	}
}