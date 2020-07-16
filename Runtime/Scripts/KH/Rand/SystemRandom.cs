using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Rand {
	public class SystemRandom : IRandom {

		private System.Random _random;

		public SystemRandom() {
			_random = new System.Random();
		}

		public SystemRandom(int seed) {
			_random = new System.Random(seed);
		}

		public override int Next(int minInclusive, int maxExclusive) {
			return _random.Next(minInclusive, maxExclusive);
		}

		public override double NextDouble() {
			return _random.NextDouble();
		}

		public override IRandom ChildRandom() {
			return new SystemRandom(Next(0, int.MaxValue));
		}
	}
}