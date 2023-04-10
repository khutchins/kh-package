using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ratferences;

public class RegisterGameObject : MonoBehaviour {
    public GameObjectReference Variable;

    // Start is called before the first frame update
    void Awake() {
        Variable.Value = this.gameObject;
    }
}
