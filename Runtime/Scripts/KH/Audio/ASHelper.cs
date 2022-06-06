using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ASHelper {
	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 pos, bool is2D, float volume = 1f, float pitch = 1f, bool shouldDestroy = true) {
		GameObject audioGameObject = new GameObject("TempAudio");
		AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
		RepurposeAudioSource(audioSource, clip, pos, is2D, volume, pitch);
		audioSource.Play();
		if (shouldDestroy) Object.Destroy(audioGameObject, clip.length / Mathf.Max(0.001f, pitch));
		return audioSource;
	}

	public static void RepurposeAudioSource(AudioSource source, AudioClip clip, Vector3 pos, bool is2D, float volume = 1f, float pitch = 1f) {
		source.gameObject.transform.position = pos;
		source.clip = clip;
		source.volume = volume;
		source.pitch = pitch;
		source.spatialBlend = is2D ? 0 : 1;
	}
}
