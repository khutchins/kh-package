using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace KH {
	public class FaceGameObject : MonoBehaviour {
		public GameObjectReference ObjectReference;

		void Update() {
			if (ObjectReference.Value == null) return;

			Vector3 dir = ObjectReference.Value.transform.position - this.transform.position;
			dir.y = 0;
			this.transform.rotation = Quaternion.LookRotation(-dir);
		}
	}
}