using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.References;

namespace KH {
	public class FaceGameObject : MonoBehaviour {
		public GameObjectReference ObjectReference;

		void Update() {
			if (ObjectReference.Value == null) return;

			Vector3 dir = ObjectReference.Value.transform.position - this.transform.position;
			dir.y = 0;

			// Don't face game object if they are overlapping: it makes Unity sad.
			if (dir.sqrMagnitude == 0) {
				return;
			}
			this.transform.rotation = Quaternion.LookRotation(-dir);
		}
	}
}