using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for LeverInteractable with MinMaxSlider controls and enhanced visualization.
    /// </summary>
    [CustomEditor(typeof(LeverInteractable))]
    [CanEditMultipleObjects]
    public class LeverInteractableEditor : ConstrainedInteractableEditor
    {
        private LeverInteractable _leverComponent;
        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _angleRangeProp;
        private SerializedProperty _projectionDistanceProp;
        private SerializedProperty _onLeverChangedProp;
        private SerializedProperty _currentAngleProp;
        private SerializedProperty _currentNormalizedAngleProp;

        private static bool _editLeverRange = false;
        private static bool _previewFiresEvents = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            _leverComponent = (LeverInteractable)target;
            _rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            _angleRangeProp = serializedObject.FindProperty("angleRange");
            _projectionDistanceProp = serializedObject.FindProperty("projectionDistance");
            _onLeverChangedProp = serializedObject.FindProperty("onLeverChanged");
            _currentAngleProp = serializedObject.FindProperty("currentAngle");
            _currentNormalizedAngleProp = serializedObject.FindProperty("currentNormalizedAngle");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Lever-style interactable with constrained rotation around a single axis. Provides smooth rotation control with configurable limits.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Lever Settings", EditorStyles.boldLabel);

            base.DrawCustomProperties();

            if (_rotationAxisProp != null)
            {
                EditorGUILayout.PropertyField(_rotationAxisProp,
                    new GUIContent("Rotation Axis", "The axis around which the lever rotates"));
            }

            if (_projectionDistanceProp != null)
            {
                EditorGUILayout.PropertyField(_projectionDistanceProp,
                    new GUIContent("Projection Distance", "Reference distance for angle calculation (affects sensitivity)"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);

            DrawAngleRangeSlider();

            DrawPreviewSection();
        }

        protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Lever Events", EditorStyles.boldLabel);

            if (_onLeverChangedProp != null)
            {
                EditorGUILayout.PropertyField(_onLeverChangedProp,
                    new GUIContent("On Lever Changed", "Event fired when lever position changes (0-1)"));
            }

            EditorGUILayout.Space();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Lever Debug", EditorStyles.boldLabel);

            if (_currentAngleProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_currentAngleProp,
                    new GUIContent("Current Angle", "Current rotation angle in degrees"));
                EditorGUI.EndDisabledGroup();
            }

            if (_currentNormalizedAngleProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_currentNormalizedAngleProp,
                    new GUIContent("Normalized Angle", "Normalized rotation value (0-1)"));
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawAngleRangeSlider()
        {
            if (_angleRangeProp == null) return;

            Vector2 range = _angleRangeProp.vector2Value;
            float minValue = range.x;
            float maxValue = range.y;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Angle Range", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            // Min value field
            EditorGUILayout.LabelField("Min:", GUILayout.Width(35));
            minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(50));

            // MinMaxSlider
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, -180f, 180f);

            // Max value field
            EditorGUILayout.LabelField("Max:", GUILayout.Width(35));
            maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(50));

            EditorGUILayout.EndHorizontal();

            // Display range
            EditorGUILayout.LabelField($"Range: {minValue:F1}° to {maxValue:F1}°", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            // Apply clamping
            minValue = Mathf.Clamp(minValue, -180f, maxValue - 1f);
            maxValue = Mathf.Clamp(maxValue, minValue + 1f, 180f);

            _angleRangeProp.vector2Value = new Vector2(minValue, maxValue);

            EditorGUILayout.Space(2);
        }

        /// <summary>
        /// Slider that poses the lever between its limits without entering play mode, so the
        /// travel can be checked in the scene view. In play mode the same slider drives the live
        /// lever through its normal API.
        /// </summary>
        private void DrawPreviewSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_leverComponent == null || _leverComponent.InteractableObject == null)
            {
                EditorGUILayout.HelpBox("Assign an Interactable Object to preview the lever.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            Vector2 range = _leverComponent.AngleRange;
            float currentAngle = Application.isPlaying
                ? _leverComponent.CurrentAngle
                : _leverComponent.PreviewAngle;
            float normalized = Mathf.InverseLerp(range.x, range.y, currentAngle);

            EditorGUI.BeginChangeCheck();
            normalized = EditorGUILayout.Slider(
                new GUIContent("Position", "Poses the lever between its limits (0 = min, 1 = max)"),
                normalized, 0f, 1f);
            bool sliderChanged = EditorGUI.EndChangeCheck();

            EditorGUILayout.LabelField($"Angle: {Mathf.Lerp(range.x, range.y, normalized):F1}°", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            bool goMin = GUILayout.Button("Min");
            bool goMid = GUILayout.Button("Center");
            bool goMax = GUILayout.Button("Max");
            EditorGUILayout.EndHorizontal();

            _previewFiresEvents = EditorGUILayout.ToggleLeft(
                new GUIContent("Fire Events While Previewing",
                    "Also raise On Lever Changed so listeners react to the previewed position"),
                _previewFiresEvents);

            using (new EditorGUI.DisabledScope(Application.isPlaying || !_leverComponent.IsPreviewingPose))
            {
                if (GUILayout.Button("Reset To Rest Pose"))
                    ClearPreview();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);

            if (goMin) ApplyPreview(0f);
            else if (goMid) ApplyPreview(0.5f);
            else if (goMax) ApplyPreview(1f);
            else if (sliderChanged) ApplyPreview(normalized);
        }

        private void ApplyPreview(float normalized)
        {
            foreach (var obj in targets)
            {
                if (obj is not LeverInteractable lever || lever.InteractableObject == null) continue;

                if (Application.isPlaying)
                {
                    lever.SetNormalizedAngle(normalized);
                    continue;
                }

                Undo.RecordObject(lever, "Preview Lever");
                Undo.RecordObject(lever.InteractableObject, "Preview Lever");
                lever.SetPreviewNormalizedAngle(normalized, _previewFiresEvents);
                EditorUtility.SetDirty(lever);
                EditorUtility.SetDirty(lever.InteractableObject);
            }

            // The component was changed directly, so refresh the cached serialized state before
            // OnInspectorGUI applies it back over the new values.
            serializedObject.Update();
            SceneView.RepaintAll();
        }

        private void ClearPreview()
        {
            foreach (var obj in targets)
            {
                if (obj is not LeverInteractable lever || !lever.IsPreviewingPose) continue;

                Undo.RecordObject(lever, "Reset Lever Preview");
                if (lever.InteractableObject != null)
                    Undo.RecordObject(lever.InteractableObject, "Reset Lever Preview");

                lever.ClearPreviewPose();
                EditorUtility.SetDirty(lever);
                if (lever.InteractableObject != null)
                    EditorUtility.SetDirty(lever.InteractableObject);
            }

            serializedObject.Update();
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Draws the lever's current position inside the range arc and a handle that scrubs it,
        /// so the travel can be dragged straight in the scene view.
        /// </summary>
        private void DrawPositionNeedle(Vector3 pivot, Vector3 axis, Vector3 restUp, float radius)
        {
            float angle = _leverComponent.CurrentAngle;
            Vector3 dir = Quaternion.AngleAxis(angle, axis) * restUp;
            Vector3 tip = pivot + dir * radius;

            Handles.color = Color.yellow;
            Handles.DrawLine(pivot, tip, 3f);
            Handles.Label(tip, $"{angle:F1}° ({_leverComponent.CurrentNormalizedAngle:F2})");

            EditorGUI.BeginChangeCheck();
            Vector3 dragged = Handles.FreeMoveHandle(tip,
                HandleUtility.GetHandleSize(tip) * 0.07f, Vector3.zero, Handles.SphereHandleCap);

            if (!EditorGUI.EndChangeCheck()) return;

            Vector2 range = _leverComponent.AngleRange;
            float newAngle = Vector3.SignedAngle(restUp, (dragged - pivot).normalized, axis);
            ApplyPreview(Mathf.InverseLerp(range.x, range.y, Mathf.Clamp(newAngle, range.x, range.y)));
        }

        private void DrawEditButton()
        {
            EditorGUILayout.Space();

            var icon = EditorGUIUtility.IconContent(_editLeverRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };

            Color prevColor = GUI.color;
            if (_editLeverRange)
                GUI.color = Color.green;

            if (GUILayout.Button(icon, iconButtonStyle))
                _editLeverRange = !_editLeverRange;

            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        private void OnSceneGUI()
        {
            if (_leverComponent == null || _leverComponent.InteractableObject == null) return;

            Transform t = _leverComponent.InteractableObject;
            Vector3 pivot = t.position;
            // Rest frame, so the range arc stays put while the body swings under preview/play.
            var (axis, up) = _leverComponent.GetRestRotationAxis();
            float radius = HandleUtility.GetHandleSize(pivot) * 1.2f;

            Vector2 angleRange = _leverComponent.AngleRange;
            float minAngle = angleRange.x;
            float maxAngle = angleRange.y;

            // Calculate world positions for min/max
            Quaternion minRot = Quaternion.AngleAxis(minAngle, axis);
            Quaternion maxRot = Quaternion.AngleAxis(maxAngle, axis);
            Vector3 minDir = minRot * up;
            Vector3 maxDir = maxRot * up;
            Vector3 minPos = pivot + minDir * radius;
            Vector3 maxPos = pivot + maxDir * radius;

            // Draw arc visualization
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.Label(minPos, $"Min ({minAngle:F1}°)");
            Handles.Label(maxPos, $"Max ({maxAngle:F1}°)");

            DrawPositionNeedle(pivot, axis, up, radius);

            if (!_editLeverRange) return;

            Undo.RecordObject(_leverComponent, "Edit Lever Limits");

            // Min handle
            Handles.color = Handles.preselectionColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newMinPos = Handles.FreeMoveHandle(minPos, 
                HandleUtility.GetHandleSize(minPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 from = up;
                Vector3 to = (newMinPos - pivot).normalized;
                float newMin = Vector3.SignedAngle(from, to, axis);
                angleRange.x = Mathf.Clamp(newMin, -180f, angleRange.y - 1f);
                _angleRangeProp.vector2Value = angleRange;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_leverComponent);
            }

            // Max handle
            Handles.color = Handles.preselectionColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newMaxPos = Handles.FreeMoveHandle(maxPos, 
                HandleUtility.GetHandleSize(maxPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 from = up;
                Vector3 to = (newMaxPos - pivot).normalized;
                float newMax = Vector3.SignedAngle(from, to, axis);
                angleRange.y = Mathf.Clamp(newMax, angleRange.x + 1f, 180f);
                _angleRangeProp.vector2Value = angleRange;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_leverComponent);
            }

            Handles.color = Color.white;
        }
    }
}