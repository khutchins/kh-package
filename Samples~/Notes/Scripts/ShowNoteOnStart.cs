using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Notes {
    public class ShowNoteOnStart : MonoBehaviour {
        public NoteReference NoteReference;
		public Note Note;

		private void Start() {
			// NoteManager listens on the note reference and displays itself
			// if the note reference is populated.
			NoteReference.Value = Note;
		}
	}
}