using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Texts;

public class SimpleTextPlayer : MonoBehaviour {
    [TextArea]
    public string Line = "<color=blue><speed=.05>(What<i><speed=.5><color=white> will </color></speed></i>happen next<pause=.5/>?)</color><shake=0.5/>";

    void Start() {
        TextAnimator.SharedAnimator.PlayText("Kevin", Color.white, Line);
    }
}
