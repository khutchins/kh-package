using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Input {
	public abstract class InputMediator : ScriptableObject {

		public abstract float MoveX();
		public abstract float MoveY();

		public abstract float LookX();
		public abstract float LookY();

		public abstract bool Sprint();
		public abstract bool SprintDown();

		public abstract bool Crouch();
		public abstract bool CrouchDown();

		public abstract bool Jump();
		public abstract bool JumpDown();

		public abstract float UIX();
		public abstract float UIY();

		public abstract bool UISubmitDown();
		public abstract bool UICancelDown();
		public abstract bool PauseDown();

		public abstract bool Interact();
	}
}