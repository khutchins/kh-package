﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ASHelper {

	public static AudioSource MakeAudioSource() {
		GameObject audioGameObject = new GameObject("TempAudio");
		AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
		return audioSource;
	}

	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 pos, bool is2D, float volume = 1f, float pitch = 1f, bool shouldDestroy = true) {
		AudioSource audioSource = MakeAudioSource();
		RepurposeAudioSource(audioSource, clip, pos, is2D, volume, pitch);
		audioSource.Play();
		if (shouldDestroy) ScheduleSourceDestruction(audioSource);
		return audioSource;
	}

	public static void RepurposeAudioSource(AudioSource source, AudioClip clip, Vector3 pos, bool is2D, float volume = 1f, float pitch = 1f) {
		source.gameObject.transform.position = pos;
		source.clip = clip;
		source.volume = volume;
		source.pitch = pitch;
		source.spatialBlend = is2D ? 0 : 1;
	}

	public static void ScheduleSourceDestruction(AudioSource source) {
		Object.Destroy(source.gameObject, source.clip.length / Mathf.Max(0.001f, source.pitch));
    }
}
