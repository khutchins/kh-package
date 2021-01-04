using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ASHelper
{
	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f) {
		GameObject audioGameObject = new GameObject("TempAudio");
		audioGameObject.transform.position = pos;
		AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.Play();
		UnityEngine.Object.Destroy(audioGameObject, clip.length);
		return audioSource;
	}
}
