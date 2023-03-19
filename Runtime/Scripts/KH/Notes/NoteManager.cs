using KH.Audio;
using KH.Input;
using KH.References;
using Menutee;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KH.Notes {
    public class NoteManager : MonoBehaviour, IMenu {
        public MenuInputMediator InputMediator;
        public Button Previous;
        public Button Next;
        public Button Exit;
        public TextMeshProUGUI Text;
        public Image BackgroundImage;
        public BoolReference Paused;
        public NoteReference NoteReference;
        public bool PausesGame = true;

        private Note _currentNote;
        private int _currentIdx;
        private GameObject _defaultObject;
        private GameObject _cachedSelection;
        private Canvas _canvas;
        private bool _active;

        private void Awake() {
            Previous.onClick.AddListener(PreviousPressed);
            Next.onClick.AddListener(NextPressed);
            Exit.onClick.AddListener(ExitPressed);
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }

        private void OnEnable() {
            NoteReference.ValueChanged += ShowNote;
        }

        private void OnDisable() {
            NoteReference.ValueChanged -= ShowNote;
        }

        public void ShowNote(Note note) {
            _currentNote = note;
            if (_currentNote == null) {
                if (_active) {
                    MenuStack.Shared.PopAndCloseMenu(this);
                }
                return;
            }
            MenuStack.Shared.PushAndShowMenu(this);
            _currentNote.PickUpAudio?.PlayOneShot();
            SetPage(0, false);
        }

        private void SetPage(int idx, bool allowAudio = true) {
            if (idx >= _currentNote.Pages.Length) idx = _currentNote.Pages.Length - 1;
            if (idx < 0) idx = 0;

            if (allowAudio) _currentNote.PageTurnAudio?.PlayOneShot();

            bool back = _currentIdx > idx;
            _currentIdx = idx;

            Text.text = _currentNote.Pages[_currentIdx];
            BackgroundImage.enabled = _currentNote.Image != null;
            BackgroundImage.sprite = _currentNote.Image;

            UpdateButtons(idx != 0, idx != _currentNote.Pages.Length - 1, back);
        }

        private void UpdateButtons(bool previous, bool next, bool wentBack) {
            Exit.gameObject.SetActive(!next);
            Previous.gameObject.SetActive(previous);
            Next.gameObject.SetActive(next);

            List<Selectable> objs = new List<Selectable>();
            if (previous) objs.Add(Previous);
            objs.Add(next ? Next : Exit);

            EventSystem.current.SetSelectedGameObject(null);

            // Hook up navigation with elements with selectable objects.
            for (int i = 0; i < objs.Count; i++) {
                // Make new one to avoid potential property strangeness.
                Navigation navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnLeft = i > 0 ? objs[i - 1] : null;
                navigation.selectOnRight = i < objs.Count - 1 ? objs[i + 1] : null;
                objs[i].navigation = navigation;
            }

            GameObject selected = wentBack ? objs[0].gameObject : objs[objs.Count - 1].gameObject;
            _defaultObject = selected;
            EventSystem.current.SetSelectedGameObject(selected);
        }

        private void Update() {
            if (!_active) return;
            if (_defaultObject != null && EventSystem.current.currentSelectedGameObject == null
                && (Mathf.Abs(InputMediator.UIX()) > 0.1 || Mathf.Abs(InputMediator.UIY()) > 0.1)) {
                EventSystem.current.SetSelectedGameObject(_defaultObject);
            }
        }

        void NextPressed() {
            SetPage(_currentIdx + 1);
        }

        void PreviousPressed() {
            SetPage(_currentIdx - 1);
        }

        void ExitPressed() {
            NoteReference.SetValue(null);

            MenuStack.Shared.PopAndCloseMenu(this);
        }

        public MenuAttributes GetMenuAttributes() {
            return PausesGame ? MenuAttributes.StandardPauseMenu() : MenuAttributes.StandardNonPauseMenu();
        }

        public void SetMenuUp(bool newUp) {
            _canvas.enabled = newUp;
        }

        public void SetMenuOnTop(bool isTop) {
            if (!isTop) {
                _cachedSelection = EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(null);
            }
            if (isTop && _cachedSelection != null) EventSystem.current.SetSelectedGameObject(_cachedSelection);
            _active = isTop;
        }
    }
}