using KH.Audio;
using KH.Input;
using Ratferences;
using Menutee;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KH.Notes {
    public class NoteManager : PaginatedTextManager {
        [SerializeField] TMP_Text Text;
        [SerializeField] Image BackgroundImage;
        [SerializeField] NoteReference NoteReference;

        private Note _currentNote;
        private int _currentIdx;

        protected override bool CanGoBack => 
            _currentIdx > 0 && 
            _currentNote != null && 
            _currentNote.Pages != null && 
            _currentNote.Pages.Length > 0;

        protected override bool CanGoForward => 
            _currentNote != null && 
            _currentNote.Pages != null && 
            _currentIdx < _currentNote.Pages.Length - 1;

        protected override bool AlwaysShowClose => false;

        protected override AudioEvent DisplayAudio => NoteReference == null ? null : (NoteReference.Value == null ? null : NoteReference.Value.PickUpAudio);
        protected override AudioEvent ForwardAudio => NoteReference == null ? null : (NoteReference.Value == null ? null : NoteReference.Value.PageTurnAudio);
        protected override AudioEvent BackAudio => NoteReference == null ? null : (NoteReference.Value == null ? null : NoteReference.Value.PageTurnAudio);

        private void OnEnable() {
            NoteReference.ValueChanged += ShowNote;
        }

        private void OnDisable() {
            NoteReference.ValueChanged -= ShowNote;
        }

        protected override void OnDidDisplay() {
            if (_currentNote != null && (_currentNote.Pages == null || _currentNote.Pages.Length == 0)) {
                // Bad note that we have to clear. Wait a frame, since 
                // doing this inline can break some behavior (for instance,
                // registering a listener on the note reference after setting
                // it, which this would happen before).
                ClearNoteOnDelay();
                return;
            }

            if (_currentNote == null) return;
            BackgroundImage.enabled = _currentNote.Image != null;
            BackgroundImage.sprite = _currentNote.Image;
            SetPage(0);
        }

        protected override void OnDidClose() {
            NoteReference.SetValue(null);
        }

        void ShowNote(Note note) {
            _currentNote = note;
            if (note != null) {
                Display();
            } else {
                Close();
            }
        }

        private void ClearNoteOnDelay() {
            StartCoroutine(Clear());
        }

        IEnumerator Clear() {
            yield return null;
            NoteReference.SetValue(null);
        }

        private void SetPage(int idx) {
            if (idx >= _currentNote.Pages.Length) idx = _currentNote.Pages.Length - 1;
            if (idx < 0) idx = 0;

            // Bad note with no pages.
            if (idx >= _currentNote.Pages.Length) {
                Text.text = "";
                return;
            }

            _currentIdx = idx;
            Text.text = _currentNote.Pages[_currentIdx];
        }

        protected override void GoBack() {
            SetPage(_currentIdx - 1);
        }

        protected override void GoForward() {
            SetPage(_currentIdx + 1);
        }
    }
}