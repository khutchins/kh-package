using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace KH.Audio {

	public enum SubtitleType {
		Speech = 1 << 0,
		ClosedCaptions = 1 << 1,
	}

	public class Subtitle {
		public readonly string Speaker;
		public readonly Color? Color;
		public readonly string Message;
		public readonly float Length;
		public readonly SubtitleType Type;

		public Subtitle(string speaker, string message, SubtitleType type, float length, Color? color) {
			Speaker = speaker;
			Color = color;
			Length = length;
			Message = message;
			Type = type;
		}

		public static Subtitle ForVoiceLine(string speaker, string message, float length, Color? color) {
			return new Subtitle(speaker, message, SubtitleType.Speech, length, color);
		}

		public static Subtitle ForSoundEffect(string effect, float length) {
			return new Subtitle(null, effect, SubtitleType.ClosedCaptions, length, null);
		}
	}

	public class SubtitleManager : MonoBehaviour {
		public static SubtitleManager INSTANCE;

		class SubtitleInternal {
			public readonly Subtitle Sub;
			public readonly string CachedLine;
			public readonly float StartTime;
			public float EndTime;

			public SubtitleInternal(Subtitle sub, string cachedLine, float startTime, float endTime) {
				Sub = sub;
				CachedLine = cachedLine;
				StartTime = startTime;
				EndTime = endTime;
			}
		}

		/// <summary>
		/// Shows subtitle if there is a manager in the scene.
		/// If not, will throw a warning and drop it.
		/// </summary>
		/// <param name="subtitle"></param>
		public static void AddSubtitleToDefaultManager(Subtitle subtitle) {
			if (INSTANCE == null) {
				Debug.LogWarning("No subtitle manager present in scene.");
				return;
			}
			INSTANCE.AddSubtitle(subtitle);
		}

		/// <summary>
		/// Shows subtitle if there is a manager in the scene.
		/// If not, will throw a warning and drop it.
		/// </summary>
		public static void AddSubtitleToDefaultManager(SubtitleObject subtitle) {
			AddSubtitleToDefaultManager(subtitle.AsSubtitle());
		}

		public TextMeshProUGUI Text;
		[Tooltip("Template that all subtitles will be placed in.")]
		public string Template = "<font=\"clacon2 SDF\"><mark=#000000 padding=\"20,20,20,20\">{0}</mark></font>";
		[Tooltip("Template for voice lines with a speaker. {0} is the message, {1} is the speaker name, and {2} is the speaker color.")]
		public string TemplateVoiceWithSpeaker = "<color=#{2}>{1}: {0}</color>";
		[Tooltip("Template for voice lines with a speaker. {0} is the message and {1} is the speaker color.")]
		public string TemplateVoiceWithoutSpeaker = "<color=#{1}>{0}</color>";
		[Tooltip("Template for SFX lines. {0} is the message.")]
		public string TemplateSFX = "<i>[{0}]</i>";
		public float MinimumSubtitleLength = 1f;
		public float TimeToReshowSpeaker = 5f;
		public bool ShowSpeech { get; set; } = true;
		public bool ShowClosedCaptions { get; set; } = true;

		private List<SubtitleInternal> _currentSubtitles = new List<SubtitleInternal>();
		private Dictionary<string, float> _speakerNameCache = new Dictionary<string, float>();

		private void Awake() {
			INSTANCE = this;
			Text.text = "";
		}

		public void AddSubtitle(Subtitle subtitle) {
			if (subtitle.Type == SubtitleType.ClosedCaptions && !ShowClosedCaptions) return;
			if (subtitle.Type == SubtitleType.Speech && !ShowSpeech) return;
			_currentSubtitles.Add(new SubtitleInternal(subtitle, FormatLine(subtitle), Time.time, Time.time + Mathf.Max(MinimumSubtitleLength, subtitle.Length)));
			RefreshDisplay(true);
		}

		private string FormatLine(Subtitle subtitle) {
			bool showSpeaker = CheckShouldShowSpeakerAndCacheTime(subtitle);
			if (subtitle.Type == SubtitleType.ClosedCaptions) return string.Format(TemplateSFX, subtitle.Message);
			else if (showSpeaker) return string.Format(TemplateVoiceWithSpeaker, subtitle.Message, subtitle.Speaker, ColorUtility.ToHtmlStringRGB(subtitle.Color ?? Color.white));
			else return string.Format(TemplateVoiceWithoutSpeaker, subtitle.Message, ColorUtility.ToHtmlStringRGB(subtitle.Color ?? Color.white));
		}

		private void Update() {
			RefreshDisplay(false);
		}

		private void RefreshDisplay(bool newLineAdded) {
			if (_currentSubtitles.Count <= 0) return;
			// Only refresh if there is a new line or the oldest line timed out.
			if (_currentSubtitles[0].EndTime < Time.time || newLineAdded) {
				while (_currentSubtitles.Count > 0 && _currentSubtitles[0].EndTime < Time.time) {
					_currentSubtitles.RemoveAt(0);
				}
				Text.text = string.Format(Template, string.Join("\n", _currentSubtitles.Select(x => x.CachedLine)));
			}
		}

		private bool CheckShouldShowSpeakerAndCacheTime(Subtitle subtitle) {
			if (subtitle.Speaker == null) return false;
			bool shouldShow = _speakerNameCache.ContainsKey(subtitle.Speaker) ? _speakerNameCache[subtitle.Speaker] + TimeToReshowSpeaker < Time.time : true;
			_speakerNameCache[subtitle.Speaker] = Time.time;
			return shouldShow;
		}
	}
}