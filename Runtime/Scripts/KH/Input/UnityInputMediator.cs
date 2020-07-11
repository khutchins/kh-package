using UnityEngine;
using System.Collections;

namespace KH.Input {
	[CreateAssetMenu(menuName = "Input Mediator/Unity")]
	public class UnityInputMediator : InputMediator {

		public string xAxis = "Horizontal";
		public string yAxis = "Vertical";
		public string xMouse = "Mouse X";
		public string yMouse = "Mouse Y";
		public string crouch = "Fire1";
		public string jump = "Jump";
		public string sprint = "Fire3";
		public string uiInputCancel = "Cancel";
		public string pause = "Pause";
		public string submit = "Submit";

		public float Sensitivity = 180;

		public override float LookX() {
			return UnityEngine.Input.GetAxis(xMouse) * Sensitivity;
		}

		public override float LookY() {
			return UnityEngine.Input.GetAxis(yMouse) * Sensitivity;
		}

		public override float MoveX() {
			return UnityEngine.Input.GetAxisRaw(xAxis);
		}

		public override float MoveY() {
			return UnityEngine.Input.GetAxisRaw(yAxis);
		}

		public override bool Crouch() {
			return UnityEngine.Input.GetButton(crouch);
		}

		public override bool CrouchDown() {
			return UnityEngine.Input.GetButtonDown(crouch);
		}

		public override bool Jump() {
			return UnityEngine.Input.GetButton(jump);
		}

		public override bool JumpDown() {
			return UnityEngine.Input.GetButtonDown(jump);
		}

		public override bool Sprint() {
			return UnityEngine.Input.GetButton(sprint);
		}

		public override bool SprintDown() {
			return UnityEngine.Input.GetButtonDown(sprint);
		}

		public override float UIX() {
			return UnityEngine.Input.GetAxisRaw(xAxis);
		}

		public override float UIY() {
			return UnityEngine.Input.GetAxisRaw(yAxis);
		}

		public override bool UICancelDown() {
			return UnityEngine.Input.GetButtonDown(uiInputCancel);
		}

		public override bool UISubmitDown() {
			return UnityEngine.Input.GetButtonDown(submit);
		}

		public override bool PauseDown() {
			return UnityEngine.Input.GetButtonDown(pause);
		}

		public override bool Interact() {
			return UnityEngine.Input.GetButtonDown(submit);
		}
	}
}