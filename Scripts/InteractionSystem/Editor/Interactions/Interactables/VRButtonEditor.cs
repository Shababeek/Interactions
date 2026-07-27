using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Editor for <see cref="VRButton"/> that adds an authoring-time preview of the press travel:
    /// a slider that scrubs the button visual between its normal and pressed positions, capture
    /// helpers for those positions, and scene handles showing the travel range.
    /// </summary>
    /// <remarks>
    /// The preview only runs in edit mode. Scrubbing moves the button transform, so the editor
    /// restores it to the normal position when the inspector is closed or the preview is released,
    /// leaving no stray transform override behind. In play mode the slider becomes a read-only
    /// readout of the live press amount.
    /// </remarks>
    [CustomEditor(typeof(VRButton))]
    public class VRButtonEditor : Editor
    {
        private const string PreviewUndoName = "Preview Button Press";

        private VRButton _button;
        private SerializedProperty _buttonTransformProp;
        private SerializedProperty _normalPositionProp;
        private SerializedProperty _pressedPositionProp;

        private float _preview;
        private bool _previewing;

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        private void OnEnable()
        {
            _button = (VRButton)target;
            _buttonTransformProp = serializedObject.FindProperty("button");
            _normalPositionProp = serializedObject.FindProperty("normalPosition");
            _pressedPositionProp = serializedObject.FindProperty("pressedPosition");
        }

        private void OnDisable()
        {
            RestorePreview();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Press Preview", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                DrawRuntimeReadout();
                return;
            }

            var visual = ResolveButtonTransform();
            if (visual == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a Button transform (or give this object a child) to preview the press.",
                    MessageType.Info);
                return;
            }

            DrawPreviewSlider(visual);
            DrawPreviewButtons(visual);
            DrawCaptureButtons(visual);
        }

        private void DrawRuntimeReadout()
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(rect, _button.PressAmount, $"Pressed {_button.PressAmount:P0}");
        }

        private void DrawPreviewSlider(Transform visual)
        {
            EditorGUI.BeginChangeCheck();
            float value = EditorGUILayout.Slider(
                new GUIContent("Preview", "Scrubs the button between its normal and pressed positions. Edit mode only."),
                _preview, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
                ApplyPreview(visual, value);
        }

        private void DrawPreviewButtons(Transform visual)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Normal")) ApplyPreview(visual, 0f);
                if (GUILayout.Button("Pressed")) ApplyPreview(visual, 1f);
                using (new EditorGUI.DisabledScope(!_previewing))
                {
                    if (GUILayout.Button("Restore")) RestorePreview();
                }
            }
        }

        private void DrawCaptureButtons(Transform visual)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Set Normal From Current",
                        "Stores the button's current local position as the normal position.")))
                    CapturePosition(visual, _normalPositionProp);

                if (GUILayout.Button(new GUIContent("Set Pressed From Current",
                        "Stores the button's current local position as the pressed position.")))
                    CapturePosition(visual, _pressedPositionProp);
            }

            float travel = (_pressedPositionProp.vector3Value - _normalPositionProp.vector3Value).magnitude;
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.FloatField(new GUIContent("Travel Distance", "Distance between the two positions."), travel);
        }

        private void CapturePosition(Transform visual, SerializedProperty target)
        {
            Undo.RecordObject(_button, "Capture Button Position");
            target.vector3Value = visual.localPosition;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_button);
            _preview = target == _pressedPositionProp ? 1f : 0f;
            _previewing = true;
        }

        private void ApplyPreview(Transform visual, float value)
        {
            _preview = Mathf.Clamp01(value);
            _previewing = true;
            Undo.RecordObject(visual, PreviewUndoName);
            visual.localPosition = Vector3.Lerp(
                _normalPositionProp.vector3Value,
                _pressedPositionProp.vector3Value,
                _preview);
        }

        private void RestorePreview()
        {
            if (!_previewing || Application.isPlaying) return;
            _previewing = false;
            _preview = 0f;

            var visual = ResolveButtonTransform();
            if (visual == null) return;

            Undo.RecordObject(visual, PreviewUndoName);
            visual.localPosition = _normalPositionProp.vector3Value;
        }

        private Transform ResolveButtonTransform()
        {
            if (_buttonTransformProp.objectReferenceValue is Transform assigned) return assigned;
            return _button != null && _button.transform.childCount > 0 ? _button.transform.GetChild(0) : null;
        }

        private void OnSceneGUI()
        {
            var visual = ResolveButtonTransform();
            if (visual == null || visual.parent == null) return;

            Transform parent = visual.parent;
            Vector3 worldNormal = parent.TransformPoint(_normalPositionProp.vector3Value);
            Vector3 worldPressed = parent.TransformPoint(_pressedPositionProp.vector3Value);
            float size = HandleUtility.GetHandleSize(worldNormal) * 0.05f;

            Handles.color = Color.green;
            Handles.DrawDottedLine(worldNormal, worldPressed, 3f);
            Handles.SphereHandleCap(0, worldNormal, Quaternion.identity, size, EventType.Repaint);
            Handles.Label(worldNormal + Vector3.up * size * 2f, "Normal");

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, worldPressed, Quaternion.identity, size, EventType.Repaint);
            Handles.Label(worldPressed + Vector3.up * size * 2f, "Pressed");

            float t = Application.isPlaying ? _button.PressAmount : _preview;
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, Vector3.Lerp(worldNormal, worldPressed, t), Quaternion.identity,
                size * 0.8f, EventType.Repaint);

            Handles.color = Color.white;
        }
    }
}
