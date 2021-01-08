using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public abstract class Conditional : ScriptableObject {

		public enum Comparator {
			LessThan,
			LessThanEqual,
			Equal,
			GreaterThanEqual,
			GreaterThan,
			NotEqual,
		}

		public abstract bool isTrue();
	}
}