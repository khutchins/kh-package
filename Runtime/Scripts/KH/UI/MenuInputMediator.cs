using UnityEngine;

namespace KH.UI {
	public abstract class MenuInputMediator : ScriptableObject {
		public abstract float UIX();
		public abstract float UIY();

		public abstract bool UISubmitDown();
		public abstract bool UICancelDown();
		public abstract bool PauseDown();
	}
}