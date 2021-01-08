using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class RegisterGameObject : MonoBehaviour {
    public GameObjectVariable Variable;

    // Start is called before the first frame update
    void Awake() {
        Variable.SetValue(this.gameObject);
    }
}
