using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KH.Actions;

public class ActionUnityEvent : Action {
    public UnityEvent<Action> Event;

	public bool FinishAfterTimeout = true;
    public float TimeToWaitBeforeFinish = 1f;

	public override void Begin() {
		Event?.Invoke(this);
		if (FinishAfterTimeout) {

		}
	}

	private IEnumerable WaitCoroutine() {
		yield return new WaitForSeconds(TimeToWaitBeforeFinish);
		Finished();
	}
}
