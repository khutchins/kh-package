using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
    public class UpdateBoolReference : MonoBehaviour {
        public BoolReference Reference;

        public void UpdateReference(bool newValue) {
            Reference.Value = newValue;
		}
    }
}