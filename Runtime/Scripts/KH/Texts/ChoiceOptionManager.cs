using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KH.Texts {
    public class ChoiceOptionManager : MonoBehaviour {
        [Tooltip("Button to listen on. If none is set, will attempt to get it from the object.")]
        public Button Button;
        [SerializeField] TMP_Text ChoiceText;

        private ChoiceOptionSpec _option;

        private void Awake() {
            if (Button == null) {
                Button = GetComponent<Button>();
            }
            Button.onClick.AddListener(ButtonWasPressed);
        }

        public void SetOption(ChoiceOptionSpec option) {
            _option = option;
            ChoiceText.text = option.OptionText;
        }

        void ButtonWasPressed() {
            if (_option == null) {
                Debug.LogWarning("Choice option selected with no option.");
            }

            _option?.Selected();
        }
    }
}