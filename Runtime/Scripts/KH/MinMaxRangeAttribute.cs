using System;

// This file taken from https://www.youtube.com/watch?v=6vmRwLYWNRo.
namespace KH {
	public class MinMaxRangeAttribute : Attribute {
		public MinMaxRangeAttribute(float min, float max) {
			Min = min;
			Max = max;
		}
		public float Min { get; private set; }
		public float Max { get; private set; }
	}
}