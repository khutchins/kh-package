using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
	public class Billboard : MonoBehaviour {
		[Tooltip("Camera to face. If null, will point at main camera.")]
		public Camera CameraToFace;

		public bool FreezeX = false;
		public bool FreezeY = true;
		public bool FreezeZ = false;

		void Update() {
			Camera camera = CameraToFace != null ? CameraToFace : Camera.main;

			Vector3 dir = camera.transform.position - this.transform.position;
			dir.x = FreezeX ? 0 : dir.x;
			dir.y = FreezeY ? 0 : dir.y;
			dir.z = FreezeZ ? 0 : dir.z;

			// Don't face game object if they are overlapping: it makes Unity sad.
			if (dir.sqrMagnitude == 0) {
				return;
			}
			this.transform.rotation = Quaternion.LookRotation(-dir);
		}

		public void SetCamera(Camera camera) {
			CameraToFace = camera;
		}
	}
}