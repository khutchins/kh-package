using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH;

namespace KH.Actions {
	public class ActionDisableObject : Action {

		public GameObject Object;

		public override void Begin() {
			if (Object == null) {
				Debug.LogWarning("No object on action " + this);
			} else {
				Object.SetActive(false);
			}
			Finished();
		}

		// Start is called before the first frame update
		void Start() {

		}

		// Update is called once per frame
		void Update() {

		}
	}
}
