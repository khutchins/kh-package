using Ratferences;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace KH.Save {
    public class SerializableSOAttribute : PreserveAttribute {
        public SaveType SaveType;

        public SerializableSOAttribute(SaveType saveType = SaveType.AfterDelay) {
            SaveType = saveType;
        }
    }
}