using KH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ratferences;

namespace KH.Input {
	[CreateAssetMenu(menuName = "Input Mediator/Pausable")]
	public class PausableInputMediator : InputMediator {

		public BoolReference Paused;
		public InputMediator Source;

		// UI Elements (not paused)
		public override bool UISubmitDown() {
			return Source.UISubmitDown();
		}

		public override bool UICancelDown() {
			return Source.UICancelDown();
		}

		public override float UIX() {
			return Source.UIX();
		}

		public override float UIY() {
			return Source.UIY();
		}

		// Game elements (pausable)
		public override bool PauseDown() {
			return Source.PauseDown();
		}

		public override bool Crouch() {
			return Paused.Value ? false : Source.Crouch();
		}

		public override bool CrouchDown() {
			return Paused.Value ? false : Source.CrouchDown();
		}

		public override bool Interact() {
			return Paused.Value ? false : Source.Interact();
		}

		public override bool Sprint() {
			return Paused.Value ? false : Source.Sprint();
		}

		public override bool SprintDown() {
			return Paused.Value ? false : Source.SprintDown();
		}

		public override bool Jump() {
			return Paused.Value ? false : Source.Jump();
		}

		public override bool JumpDown() {
			return Paused.Value ? false : Source.JumpDown();
		}

		public override float LookX() {
			return Paused.Value ? 0 : Source.LookX();
		}

		public override float LookY() {
			return Paused.Value ? 0 : Source.LookY();
		}

		public override float MoveX() {
			return Paused.Value ? 0 : Source.MoveX();
		}

		public override float MoveY() {
			return Paused.Value ? 0 : Source.MoveY();
		}
	}
}