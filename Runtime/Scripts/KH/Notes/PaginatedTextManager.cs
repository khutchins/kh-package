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
    public abstract class PaginatedTextManager : MonoBehaviour, IMenu {
        [SerializeField] MenuInputMediator InputMediator;
        [SerializeField] Button Previous;
        [SerializeField] Button Next;
        [SerializeField] Button Exit;
        [SerializeField] bool PausesGame = true;

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

        public void Display() {
            if (_active) {
                MenuStack.Shared.PopAndCloseMenu(this);
            }
            OnWillDisplay();
            AudioEvent audio = DisplayAudio;
            if (audio != null) audio.PlayOneShot();
            MenuStack.Shared.PushAndShowMenu(this);
            OnDidDisplay();
            UpdateButtons(false);
        }

        protected virtual void OnWillDisplay() { }
        protected virtual void OnDidDisplay() { }
        protected virtual void OnWillClose() { }
        protected virtual void OnDidClose() { }
        protected abstract bool CanGoBack { get; }
        protected abstract void GoBack();
        protected abstract bool CanGoForward { get; }
        protected abstract bool AlwaysShowClose { get; }
        protected abstract void GoForward();
        protected virtual AudioEvent DisplayAudio { get => null; }
        protected virtual AudioEvent CloseAudio { get => null; }
        protected virtual AudioEvent ForwardAudio { get => null; }
        protected virtual AudioEvent BackAudio { get => null; }

        private void UpdateButtons(bool wentBack) {
            UpdateButtons(CanGoBack, CanGoForward, wentBack);
        }

        private void UpdateButtons(bool previous, bool next, bool wentBack) {
            bool showClose = (!next || AlwaysShowClose);
            Exit.gameObject.SetActive(showClose);
            Previous.gameObject.SetActive(previous);
            Next.gameObject.SetActive(next);

            List<Selectable> objs = new List<Selectable>();
            if (previous) objs.Add(Previous);
            if (next) objs.Add(Next);
            if (showClose) objs.Add(Exit);

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

            GameObject selected = wentBack ? objs[0].gameObject : (objs.Count > 1 ? objs[1] : objs[0]).gameObject;
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
            if (CanGoForward) GoForward();
            AudioEvent audio = ForwardAudio;
            if (audio != null) audio.PlayOneShot();
            UpdateButtons(false);
        }

        void PreviousPressed() {
            if (CanGoBack) GoBack();
            AudioEvent audio = BackAudio;
            if (audio != null) audio.PlayOneShot();
            UpdateButtons(false);
        }

        void ExitPressed() {
            Close();
        }

        public void Close() {
            if (!_active) return;
            OnWillClose();
            MenuStack.Shared.PopAndCloseMenu(this);
            AudioEvent audio = CloseAudio;
            if (audio != null) audio.PlayOneShot();
            OnDidClose();
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