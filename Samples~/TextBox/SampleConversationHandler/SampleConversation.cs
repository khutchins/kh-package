using KH.Texts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleConversation : MonoBehaviour {
    [SerializeField] LineSpecQueue LineQueue;
    [SerializeField] ChoiceSpecQueue ChoiceQueue;

    void Start() {
        StartCoroutine(DoTheThing());
    }

    IEnumerator DoTheThing() {
        while (true) {
            yield return LineQueue.EnqueueAndAwait(new LineSpec("Other.", "This is the conversation that never ends."));
            ChoiceSpec choice = new ChoiceSpec(new ChoiceOptionSpec("Yeah right."), new ChoiceOptionSpec("No way!"), new ChoiceOptionSpec("A multi-line\nchoice!"));
            yield return ChoiceQueue.EnqueueAndAwait(choice);
            // This is the easiest way to retrieve the choice if you want everything to happen inline.
            // You can also add a listener to the ChoiceSpec and handle it in the callback, but that's
            // more verbose for this trivial example.
            int choiceIndex = choice.LastIndex;
            string line = choiceIndex switch {
                0 => "Yes, it goes on and on, my friend.",
                1 => "Some people started singing it not knowing what it was.",
                2 => "And they'll continue singing it forever just because.",
                _ => "You shouldn't even be able to trigger this dialogue!",
            };
            yield return LineQueue.EnqueueAndAwait(new LineSpec("Other.", line));
        }
    }
}
