using KH.Interact;

namespace KH.Notes {
	public class NoteInteractable : Interactable {
		public Note Note;
		public NoteReference NoteManagerNote;

		public override bool LocksMouse() {
			return true;
		}

		public override bool LocksMovement() {
			return true;
		}

		public override bool ExclusiveInteraction() {
			return true;
		}

		protected override void StartInteractingInner(Interactor interactor) {
			ReadNote();
			NoteManagerNote.ValueChanged += NoteChanged;
		}

		protected override void StopInteractingInner(Interactor interactor) {
			NoteManagerNote.ValueChanged -= NoteChanged;
		}

		// Note canvas will set the note reference to null once finished.
		void NoteChanged(Note newNote) {
			ForceStopInteraction();
		}

		protected virtual void ReadNote() {
			NoteManagerNote.SetValue(Note);
		}
	}
}