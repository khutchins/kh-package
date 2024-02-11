using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleRotator : MonoBehaviour {
    // Update is called once per frame
    void Update() {
        this.transform.localEulerAngles = new Vector3(
            Mathf.Sin(Time.time * 1f/16 * Mathf.PI) * 720, 
            Mathf.Cos(Time.time * 1f/4 * Mathf.PI) * 360, 
            Mathf.Sin(Time.time * 1f/32 * Mathf.PI) * 180);
    }
}
