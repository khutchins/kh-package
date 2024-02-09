using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Input;

namespace KH.Texts {
	/// <summary>
	/// Reads LineSpecs from a ScriptableObject and passes them along to a TextAnimator on the same object.
	/// Only one TextPrompter should service a TextAnimator, or else race conditions may occur.
	/// </summary>
	[RequireComponent(typeof(ChoiceManager))]
	public class ChoicePrompter : MonoBehaviour {
		[Tooltip("The choice spec queue the choice prompter will read from. Must match that of the choice spec sources.")]
		public ChoiceSpecQueue Queue;

		private ChoiceManager _choiceManager;
		private ChoiceSpec _current;

		// Start is called before the first frame update
		void Awake() {
			_choiceManager = GetComponent<ChoiceManager>();
			Queue.Clear();
		}

		private void OnEnable() {
			Queue.OnFirstItemAdded += FirstItemAdded;
			_choiceManager.OnChoiceMade += ChoiceMade;
		}

		private void OnDisable() {
			Queue.OnFirstItemAdded -= FirstItemAdded;
			_choiceManager.OnChoiceMade -= ChoiceMade;
		}

		private void TryHandleNextItem() {
			if (_choiceManager.IsDisplayingChoice) return;
			ChoiceSpec nextChoice = Queue.Dequeue();
			if (nextChoice == null) return;

			_current = nextChoice;
			_choiceManager.ShowChoice(_current);
		}

		private void ChoiceMade(ChoiceSpec spec, int index, ChoiceOptionSpec option) {
			DoneWithChoice();
		}

		private void DoneWithChoice() {
			_current = null;
			TryHandleNextItem();
		}

		private void FirstItemAdded() {
			TryHandleNextItem();
		}
	}
}