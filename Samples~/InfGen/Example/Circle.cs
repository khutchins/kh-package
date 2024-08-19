using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour {
    [SerializeField] float Radius = 50;
    [SerializeField] float SpeedMod = 0.25f;
    private Vector3 _startPos;

    // Start is called before the first frame update
    void Start() {
        _startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update() {
        transform.localPosition = _startPos + new Vector3(Mathf.Sin(Time.time * SpeedMod) * Radius, 0, Mathf.Cos(Time.time * SpeedMod) * Radius);
    }
}
