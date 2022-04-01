using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Single With Subtitle")]
	public class SingleAudioEventSub : SingleAudioEvent {
		public SubtitleObject Subtitle;

		public override AudioSource Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
			if (Subtitle != null) SubtitleManager.AddSubtitleToDefaultManager(Subtitle);
			return base.Play(source, volumeMod, pitchMod);
		}

		public override AudioSource PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
			if (Subtitle != null) SubtitleManager.AddSubtitleToDefaultManager(Subtitle);
			return base.PlayClipAtPoint(position, volumeMod, pitchMod);
		}

		public override AudioSource PlayOneShot(float volumeMod = 1f, float pitchMod = 1f) {
			if (Subtitle != null) SubtitleManager.AddSubtitleToDefaultManager(Subtitle);
			return base.PlayOneShot(volumeMod, pitchMod);
		}
	}
}