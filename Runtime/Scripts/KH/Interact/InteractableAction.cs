﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Actions;

namespace KH.Interact {
	public class InteractableAction : Interactable {

		public Action Action;
		public bool LockPlayer = true;
		public bool Exclusive = true;

		public override bool ExclusiveInteraction() {
			return Exclusive;
		}

		public override bool LocksMouse() {
			return LockPlayer;
		}

		public override bool LocksMovement() {
			return LockPlayer;
		}

		protected override bool StartInteractingInner(Interactor interactor) {
			if (Action != null) {
				Action.FinishedAction += Action_FinishedAction;
				Action.Begin();
				return true;
			} else {
				return false;
			}
		}

		private void Action_FinishedAction(Action action) {
			Action.FinishedAction -= Action_FinishedAction;
			ForceStopInteraction();
		}

		protected override void StopInteractingInner(Interactor interactor) {
			
		}
	}
}