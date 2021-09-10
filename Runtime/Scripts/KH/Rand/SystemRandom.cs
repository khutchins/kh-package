using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Rand {
	/// <summary>
	/// Wrapper of System.Random that subclasses IRandom.
	/// </summary>
	public class SystemRandom : IRandom {

		private System.Random _random;
		private int _seed;
		private object _rawSeed;

		public override int Seed => _seed;

		public override object RawSeed => _rawSeed;

		public SystemRandom() {
			_rawSeed = _seed = new System.Random().Next();
			_random = new System.Random(_seed);
		}

		public SystemRandom(int seed) {
			_rawSeed = _seed = seed;
			_random = new System.Random(_seed);
		}

		public SystemRandom(string seedStr) {
			_rawSeed = seedStr;
			_seed = StringToSeed(seedStr);
			_random = new System.Random(_seed);
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