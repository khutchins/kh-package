using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Texts;

public class SimpleTextPlayer : MonoBehaviour {
    [SerializeField] string Speaker = "Kevin";
    [SerializeField][TextArea] string Line = "<color=blue><speed=.05>(What<i><speed=.5><color=white> will </color></speed></i>happen next<pause=.5/>?)</color><shake=0.5/>";
    [SerializeField] LineSpecQueue Queue;


    void Start() {
        Queue.Enqueue(new LineSpec(Speaker, Line));
    }
}
