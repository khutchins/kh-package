using KH.Texts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLineSpecPlayer : MonoBehaviour
{
    [SerializeField] string[] Speakers;
    [SerializeField] [TextArea] string[] Lines;

    [SerializeField] LineSpecQueue Queue;

    void Start() {
        for (int i = 0; i < Lines.Length; i++) {
            LineSpec spec = new LineSpec(
                Speakers.Length > i ? Speakers[i] : "",
                Lines[i],
                i == Lines.Length - 1 ? LineCallback : null
            );
            Queue.Enqueue(spec);
		}
    }

    void LineCallback() {
        Debug.Log("Texts finished");
	}
}
