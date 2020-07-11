﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KH.UI {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TextUpdater : MonoBehaviour {

		private TextMeshProUGUI Text;
		public Slider Slider;

		private void Awake() {
			Text = GetComponent<TextMeshProUGUI>();
		}

		void Start() {
			Slider.onValueChanged.AddListener(UpdateTextFromNumber);
			UpdateTextFromNumber(Slider.value);
		}

		public void UpdateTextFromNumber(float num) {
			Text.text = string.Format("{0:0.00}", num);
		}
	}
}