using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KH.Texts {
    public class ChoiceSpec : IHandlerQueueItem<ChoiceSpec> {
        public delegate void ChoiceSelectedCallback(ChoiceSpec choice, int index,  ChoiceOptionSpec selection);

        public event ChoiceSelectedCallback OnChoiceSelected;
        public event ItemProcessed<ChoiceSpec> OnItemProcessed;

        public readonly ChoiceOptionSpec[] Options;
        public ChoiceOptionSpec LastChoice;
        public int LastIndex;

        public ChoiceSpec(params ChoiceOptionSpec[] options) : this(null, options) {}
        public ChoiceSpec(IEnumerable<ChoiceOptionSpec> options) : this(null, options.ToArray()) { }
        public ChoiceSpec(ChoiceSelectedCallback callback, IEnumerable<ChoiceOptionSpec> options) : this(callback, options.ToArray()) { }
        public ChoiceSpec(ChoiceSelectedCallback callback, params ChoiceOptionSpec[] options) {
            Options = options;
            if (callback != null) {
                OnChoiceSelected += callback;
            }
            foreach (var option in options) {
                option.OnChoiceSelected += SelectionMade;
            }
        }

        private void SelectionMade(ChoiceOptionSpec option) {
            int idx = Array.IndexOf(Options, option);
            if (idx == -1) {
                Debug.LogWarning($"Choice not associated with this ChoiceSpec: {option.OptionText}. I don't know how this could happen. Choosing first option instead.");
                option = Options[0];
                idx = 0;
            }
            LastChoice = option;
            LastIndex = idx;
            OnChoiceSelected?.Invoke(this, idx, option);
            OnItemProcessed?.Invoke(this);
        }
    }


    public class ChoiceOptionSpec {
        public delegate void ChoiceOptionSelectedCallback(ChoiceOptionSpec selection);
        public event ChoiceOptionSelectedCallback OnChoiceSelected;

        public string OptionText {
            get;
        }

        public ChoiceOptionSpec(string optionText, ChoiceOptionSelectedCallback callback = null) {
            OptionText = optionText;
            if (callback != null) {
                OnChoiceSelected += callback;
            }
        }

        public void Selected() {
            OnChoiceSelected?.Invoke(this);
        }
    }
}