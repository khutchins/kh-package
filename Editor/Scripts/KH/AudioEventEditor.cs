using UnityEngine;
using System.Collections;
using UnityEditor;

namespace KH.Audio {
	[CustomEditor(typeof(AudioEvent), true)]
	public class AudioEventEditor : UnityEditor.Editor {

		[SerializeField] private AudioSource _previewer;

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
			if (GUILayout.Button("Preview")) {
				((AudioEvent)target).Prepare().PlayUsingSource(_previewer);
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}