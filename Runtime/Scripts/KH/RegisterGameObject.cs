using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.References;

public class RegisterGameObject : MonoBehaviour {
    public GameObjectReference Variable;

    // Start is called before the first frame update
    void Awake() {
        Variable.Value = this.gameObject;
    }
}
