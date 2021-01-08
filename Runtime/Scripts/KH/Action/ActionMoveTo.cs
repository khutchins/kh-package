using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace KH.Actions {
	public class ActionMoveTo : Action {
		public GameObjectVariable ObjectToMove;
		public Transform Destination;
		public float MoveSpeed;
		public bool Blocking = true;

		public override void Begin() {
			StartCoroutine(Move());
			if (!Blocking) {
				Finished();
			}
		}

		IEnumerator Move() {
			GameObject obj = ObjectToMove.Value;
			if (obj == null) {
				if (Blocking) {
					Finished();
				}
				yield break;
			}
			Transform transform = obj.transform;

			Vector3 start = transform.position;
			float dist = Vector3.Distance(transform.position, Destination.position);
			float time = dist / MoveSpeed;

			float startTime = Time.time;

			while (Time.time - startTime < time) {
				transform.position = Vector3.Lerp(start, Destination.position, (Time.time - startTime) / time);
				yield return null;
			}

			transform.position = Destination.position;

			if (Blocking) {
				Finished();
			}
		}
	}
}