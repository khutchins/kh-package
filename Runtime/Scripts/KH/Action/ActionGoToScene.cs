using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KH.Actions {
	public class ActionGoToScene : Action {

		public string Scene;

		public override void Begin() {
			SceneManager.LoadScene(Scene);
			Finished();
		}
	}
}