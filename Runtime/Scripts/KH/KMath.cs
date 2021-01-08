using System;
using UnityEngine;

namespace KH {
	public class KMath {
		public static float Fract(float val) {
			return Mathf.Repeat(val, 1F);
		}

		/// <summary>
		/// Linearly interpolates between startVal and endVal by percent.
		/// </summary>
		/// <param name="startVal">start of range</param>
		/// <param name="endVal">end of range</param>
		/// <param name="percent">percent through range. If > 1, will go past bound.</param>
		/// <returns></returns>
		public static float Mix(float startVal, float endVal, float percent) {
			return (endVal - startVal) * percent + startVal;
		}

		public static float HermiteMix(float startVal, float endVal, float x) {
			x = Mathf.Clamp((x - startVal) / (endVal - startVal), 0F, 1F);
			return x * x * (3F - 2F * x);
		}

		public static float QuinticMix(float startVal, float endVal, float x) {
			x = Mathf.Clamp((x - startVal) / (endVal - startVal), 0F, 1F);
			return x * x * x * (x * (x * 6F - 15F) + 10F);
		}
	}
}
