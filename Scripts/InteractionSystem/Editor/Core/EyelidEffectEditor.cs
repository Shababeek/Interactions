using DG.Tweening;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(EyelidEffect))]
    public class EyelidEffectEditor : UnityEditor.Editor
    {
        private float _previewDuration = 2f;
        private float _holdDuration = 1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var effect = (EyelidEffect)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            _previewDuration = EditorGUILayout.Slider("Duration", _previewDuration, 0.05f, 3f);
            _holdDuration = EditorGUILayout.Slider("Hold (Blink)", _holdDuration, 0f, 2f);

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Close"))
                {
                    EnsureDOTween();
                    effect.Close(_previewDuration);
                }

                if (GUILayout.Button("Open"))
                {
                    EnsureDOTween();
                    effect.Open(_previewDuration);
                }

                if (GUILayout.Button("Blink"))
                {
                    EnsureDOTween();
                    effect.Blink(_previewDuration, _holdDuration, _previewDuration);
                }
            }

            EditorGUILayout.Space(2);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Snap Open"))
                    effect.SetOpen();

                if (GUILayout.Button("Snap Closed"))
                    effect.SetClosed();
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Animated preview only works in Play Mode. Use Snap buttons in Edit Mode.", MessageType.Info);
            }
        }

        private static void EnsureDOTween()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[EyelidEffect] Enter Play Mode to preview animations.");
                return;
            }
            DOTween.Init();
        }
    }
}
