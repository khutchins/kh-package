using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ratferences;

namespace KH.Actions {
	public class ActionTeleportPlayer : Action {

		public GameObjectReference GameObjectRef;
		public Transform LocationTransform;
		public Vector3 Location;

		public override void Begin() {
			GameObject go = GameObjectRef.Value;
			if (go != null) {
				if (LocationTransform != null) {
					go.transform.position = LocationTransform.position;
					go.transform.forward = LocationTransform.forward;
				} else {
					go.transform.position = Location;
				}
			}
			Finished();
		}
	}
}