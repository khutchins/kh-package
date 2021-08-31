using System;

// This file taken from https://www.youtube.com/watch?v=6vmRwLYWNRo.
namespace KH {
	[Serializable]
	public struct RangedFloat {
		public float minValue;
		public float maxValue;

		public static RangedFloat One() {
			RangedFloat range;
			range.minValue = 1;
			range.maxValue = 1;
			return range;
		}
	}
}