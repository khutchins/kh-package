using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KH.Texts {
    public class ChoiceManager : MonoBehaviour {
        public delegate void ChoiceMade(ChoiceSpec choices, int index, ChoiceOptionSpec choice);

        public event ChoiceMade OnChoiceMade;

        [Tooltip("Object that holds the choice layout. Must not be this object or a parent of it, as it will disable the object.")]
        [SerializeField] GameObject ChoiceHolder;
        [SerializeField] ChoiceOptionManager SingleChoicePrefab;

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

        }

        private void CreatePrefabsForChoice() {
            foreach (var option in _current.Options) {
                ChoiceOptionManager manager = Instantiate(SingleChoicePrefab, ChoiceHolder.transform);
                manager.SetOption(option);
                _optionManagers.Add(manager);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(ChoiceHolder.transform as RectTransform);
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
        }
    }
}