using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ASHelper {
	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 pos, bool is2D, float volume = 1f, float pitch = 1f) {
		GameObject audioGameObject = new GameObject("TempAudio");
		audioGameObject.transform.position = pos;
		AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.spatialBlend = is2D ? 0 : 1;
		audioSource.Play();
		UnityEngine.Object.Destroy(audioGameObject, clip.length / Mathf.Max(0.001f, pitch));
		return audioSource;
	}
}
