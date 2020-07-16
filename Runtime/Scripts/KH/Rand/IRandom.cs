using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KH.Rand {
	public abstract class IRandom {
		public bool NextBool() {
			return WithOdds(1, 2);
		}

		/// <summary>
		/// Returns a random value with the given odds.
		/// 
		/// Ex: WithOdds(3, 5) has a 3/5ths chance of returning true.
		/// </summary>
		/// <param name="chance">Chance that true will be returned</param>
		/// <param name="outOf">Total numbers</param>
		/// <returns>Whether or not the chance was met.</returns>
		public bool WithOdds(int chance, int outOf) {
			return Next(0, outOf) < chance;
		}

		public bool WithPercentChance(double chance) {
			return NextDouble() < chance;
		}

		public T FromList<T>(IList<T> list) {
			return list[Next(0, list.Count)];
		}

		public T FromArray<T>(T[] array) {
			return array[Next(0, array.Length)];
		}

		public T FromArrayWithOdds<T>(T[] array, int[] odds) {
			if (array.Length != odds.Length) throw new ArgumentException("Array lengths do not match");
			if (array == null || array.Length == 0) return default;
			int allOdds = odds.Sum();
			if (allOdds < 1) return array[0];
			int num = Next(0, allOdds);

			int sum = 0;

			for (int i = 0; i < array.Length; i++) {
				sum += odds[i];
				if (num < sum) return array[i];
			}
			return array[array.Length - 1];
		}

		public IEnumerable<T> PickFromList<T>(IEnumerable<T> list, int amount) {
			List<T> selected = new List<T>();
			int total = list.Count();

			int idx = 0;
			foreach (T obj in list) {
				if (WithOdds(amount - selected.Count, total - idx)) {
					selected.Add(obj);
				}
				idx++;
				if (selected.Count >= amount) break;
			}
			return selected;
		}

		public T FromEnum<T>() {
			Array values = Enum.GetValues(typeof(T));
			return (T)values.GetValue(Next(values.Length));
		}

		public int Next(int maxExclusive) {
			return Next(0, maxExclusive);
		}

		abstract public int Next(int minInclusive, int maxExclusive);
		/// <summary>
		///  Gets a random double number between 0.0 (inclusive) and 1.0 (exclusive)
		/// </summary>
		/// <returns>Double between 0.0 (inclusive) and 1.0 (exclusive)</returns>
		abstract public double NextDouble();

		public double NextDouble(double min, double max) {
			return NextDouble() * (max - min) + min;
		}

		public float Next(float min, float max) {
			return (float)(NextDouble() * (max - min) + min);
		}

		abstract public IRandom ChildRandom();
	}
}