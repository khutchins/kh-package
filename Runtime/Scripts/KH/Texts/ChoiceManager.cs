using Menutee;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KH.Texts {
    public class ChoiceManager : MonoBehaviour {
        public delegate void ChoiceMade(ChoiceSpec choices, int index, ChoiceOptionSpec choice);

        public event ChoiceMade OnChoiceMade;

        [Tooltip("Object that the choice objects will be placed under.")]
        [SerializeField] GameObject ChoiceHolder;
        [SerializeField] ChoiceOptionManager SingleChoicePrefab;

        [SerializeField] UnityEvent OnShow;
        [SerializeField] UnityEvent OnHide;

        private ChoiceSpec _current;
        private List<ChoiceOptionManager> _optionManagers = new List<ChoiceOptionManager>();

        public bool IsDisplayingChoice => _current != null;

        public void ShowChoice(ChoiceSpec choice) {
            if (IsDisplayingChoice) {
                Debug.LogWarning($"ShowChoice ignored. Already showing choice.");
            }

            _current = choice;
            _current.OnChoiceSelected += OptionPicked;
            CreatePrefabsForChoice();
            OnShow?.Invoke();
        }

        private void CreatePrefabsForChoice() {
            if (_current.Options.Length <= 0) return;
            List<Selectable> selectableObjects = new List<Selectable>();
            foreach (var option in _current.Options) {
                ChoiceOptionManager manager = Instantiate(SingleChoicePrefab, ChoiceHolder.transform);
                manager.SetOption(option);
                _optionManagers.Add(manager);
                selectableObjects.Add(manager.Button);
            }
            MenuGenerator.SetVerticalNavigation(selectableObjects);
            MaybeSetDefaultSelectable(selectableObjects[0]);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(ChoiceHolder.transform as RectTransform);
        }

        private void MaybeSetDefaultSelectable(Selectable defaultMaybe) {
            var menu = GetComponent<MenuHook>();
            if (menu != null) {
                menu.DefaultSelectedGameObject = defaultMaybe.gameObject;
            }
        }

        private void OptionPicked(ChoiceSpec choice, int index, ChoiceOptionSpec selection) {
            if (choice != _current) {
                Debug.LogWarning($"Unexpected callback with selection '{ selection.OptionText }'. Ignoring.");
                return;
            }
            _current.OnChoiceSelected -= OptionPicked;
            ForceStopDisplay();
            OnChoiceMade?.Invoke(choice, index, selection);
        }

        public void ForceStopDisplay() {
            if (!IsDisplayingChoice) return;

            _current.OnChoiceSelected -= OptionPicked;
            _current = null;

            foreach (var go in _optionManagers) {
                Destroy(go.gameObject);
            }
            _optionManagers.Clear();
            OnHide?.Invoke();
        }
    }
}