using KH.Texts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLineSpecPlayer : MonoBehaviour
{
    public string[] Speakers;
    [TextArea]
    public string[] Lines;

    public LineSpecQueue Queue;

    void Start() {
        for (int i = 0; i < Lines.Length; i++) {
            LineSpec spec = new LineSpec(Speakers.Length > i ? Speakers[i] : "", Lines[i]);
            if (i == Lines.Length - 1) spec.Callback = LineCallback;
            Queue.Enqueue(spec);
		}
    }

    void LineCallback() {
        Debug.Log("Texts finished");
	}
}
