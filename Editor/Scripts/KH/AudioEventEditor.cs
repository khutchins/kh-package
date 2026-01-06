using UnityEngine;
using System.Collections;
using UnityEditor;

namespace KH.Audio {
	[CustomEditor(typeof(AudioEvent), true)]
	public class AudioEventEditor : UnityEditor.Editor {

		[SerializeField] private AudioSource _previewer;
        private AudioPlaybackHandle _activeHandle;

        public void OnEnable() {
            GameObject go = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource), typeof(AudioProxy));
            _previewer = go.GetComponent<AudioSource>();
        }

		public void OnDisable() {
			DestroyImmediate(_previewer.gameObject);
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            AudioEvent audioEvent = (AudioEvent)target;
            if (_activeHandle == null || !_activeHandle.IsPlaying) {
                if (GUILayout.Button("Preview / Start Sequence")) {
                    if (_activeHandle != null) _activeHandle.StopImmediate();
                    _activeHandle = audioEvent.Prepare().PlayUsingSource(_previewer);
                }
            } else {
                if (GUILayout.Button("Stop (Graceful)")) {
                    _activeHandle.Stop();
                }
                if (GUILayout.Button("Stop (Immediate)")) {
                    _activeHandle.StopImmediate();
                }
            }
            EditorGUI.EndDisabledGroup();
		}
    }
}