using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Input {
    public abstract class SingleInputMediator : ScriptableObject {
        public abstract bool InputDown();
        public abstract bool InputJustDown();
    }
}