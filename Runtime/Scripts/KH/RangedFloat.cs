using System;

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