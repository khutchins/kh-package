using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionTeleport : Action {

		public Transform ObjectToTeleport;
		public Transform LocationTransform;
		public Vector3 Location;

		public override void Begin() {
			if (LocationTransform != null) {
				ObjectToTeleport.position = LocationTransform.position;
			} else {
				ObjectToTeleport.position = Location;
			}
			Finished();
		}
	}
}