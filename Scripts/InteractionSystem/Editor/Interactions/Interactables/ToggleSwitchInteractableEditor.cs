using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for ToggleSwitchInteractable with angle-range handles and step visualization.
    /// </summary>
    [CustomEditor(typeof(ToggleSwitchInteractable))]
    [CanEditMultipleObjects]
    public class ToggleSwitchInteractableEditor : ConstrainedInteractableEditor
    {
        private ToggleSwitchInteractable _toggle;
        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _angleRangeProp;
        private SerializedProperty _projectionDistanceProp;
        private SerializedProperty _numberOfStepsProp;
        private SerializedProperty _startingStepProp;
        private SerializedProperty _hapticOnStepProp;
        private SerializedProperty _hapticAmplitudeProp;
        private SerializedProperty _hapticDurationProp;
        private SerializedProperty _hapticPatternProp;
        private SerializedProperty _onStepChangedProp;
        private SerializedProperty _onStepConfirmedProp;
        private SerializedProperty _onValueChangedProp;
        private SerializedProperty _currentStepProp;
        private SerializedProperty _currentAngleProp;
        private SerializedProperty _currentNormalizedAngleProp;

        private static bool _editAngleRange = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            _toggle = (ToggleSwitchInteractable)target;
            _rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            _angleRangeProp = serializedObject.FindProperty("angleRange");
            _projectionDistanceProp = serializedObject.FindProperty("projectionDistance");
            _numberOfStepsProp = serializedObject.FindProperty("numberOfSteps");
            _startingStepProp = serializedObject.FindProperty("startingStep");
            _hapticOnStepProp = serializedObject.FindProperty("hapticOnStep");
            _hapticAmplitudeProp = serializedObject.FindProperty("hapticAmplitude");
            _hapticDurationProp = serializedObject.FindProperty("hapticDuration");
            _hapticPatternProp = serializedObject.FindProperty("hapticPattern");
            _onStepChangedProp = serializedObject.FindProperty("onStepChanged");
            _onStepConfirmedProp = serializedObject.FindProperty("onStepConfirmed");
            _onValueChangedProp = serializedObject.FindProperty("onValueChanged");
            _currentStepProp = serializedObject.FindProperty("currentStep");
            _currentAngleProp = serializedObject.FindProperty("currentAngle");
            _currentNormalizedAngleProp = serializedObject.FindProperty("currentNormalizedAngle");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Grabbable toggle switch that rotates like a lever and snaps to discrete steps on release. " +
                "Rotary counterpart to the Slider.",
                MessageType.Info);
        }

        protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Toggle Switch Settings", EditorStyles.boldLabel);

            base.DrawCustomProperties();

            if (_rotationAxisProp != null)
                EditorGUILayout.PropertyField(_rotationAxisProp,
                    new GUIContent("Rotation Axis", "The axis around which the switch rotates"));

            if (_projectionDistanceProp != null)
                EditorGUILayout.PropertyField(_projectionDistanceProp,
                    new GUIContent("Projection Distance", "Reference distance for angle calculation (affects sensitivity)"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);
            DrawAngleRangeSlider();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);
            if (_numberOfStepsProp != null)
                EditorGUILayout.PropertyField(_numberOfStepsProp, new GUIContent("Number Of Steps", "Number of discrete positions"));
            if (_startingStepProp != null)
                EditorGUILayout.PropertyField(_startingStepProp, new GUIContent("Starting Step", "Step the switch starts in (0-based)"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Haptics", EditorStyles.boldLabel);
            if (_hapticOnStepProp != null)
                EditorGUILayout.PropertyField(_hapticOnStepProp, new GUIContent("Haptic On Step"));
            if (_hapticOnStepProp != null && _hapticOnStepProp.boolValue)
            {
                if (_hapticPatternProp != null)
                    EditorGUILayout.PropertyField(_hapticPatternProp, new GUIContent("Pattern (optional)"));
                EditorGUI.BeginDisabledGroup(_hapticPatternProp != null && _hapticPatternProp.objectReferenceValue != null);
                if (_hapticAmplitudeProp != null)
                    EditorGUILayout.PropertyField(_hapticAmplitudeProp, new GUIContent("Amplitude"));
                if (_hapticDurationProp != null)
                    EditorGUILayout.PropertyField(_hapticDurationProp, new GUIContent("Duration"));
                EditorGUI.EndDisabledGroup();
            }
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Toggle Switch Events", EditorStyles.boldLabel);

            if (_onStepChangedProp != null)
                EditorGUILayout.PropertyField(_onStepChangedProp, new GUIContent("On Step Changed", "Fired when the step changes"));
            if (_onStepConfirmedProp != null)
                EditorGUILayout.PropertyField(_onStepConfirmedProp, new GUIContent("On Step Confirmed", "Fired when a step is committed"));
            if (_onValueChangedProp != null)
                EditorGUILayout.PropertyField(_onValueChangedProp, new GUIContent("On Value Changed", "Fired continuously (0-1)"));

            EditorGUILayout.Space();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Toggle Switch Debug", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                if (_currentStepProp != null)
                    EditorGUILayout.PropertyField(_currentStepProp, new GUIContent("Current Step"));
                if (_currentAngleProp != null)
                    EditorGUILayout.PropertyField(_currentAngleProp, new GUIContent("Current Angle"));
                if (_currentNormalizedAngleProp != null)
                    EditorGUILayout.PropertyField(_currentNormalizedAngleProp, new GUIContent("Normalized Angle"));
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("◀ Step")) _toggle.DecrementStep();
                if (GUILayout.Button("Step ▶")) _toggle.IncrementStep();
                EditorGUILayout.EndHorizontal();
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
            EditorGUILayout.LabelField("Min:", GUILayout.Width(35));
            minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(50));
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, -180f, 180f);
            EditorGUILayout.LabelField("Max:", GUILayout.Width(35));
            maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Range: {minValue:F1}° to {maxValue:F1}°", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            minValue = Mathf.Clamp(minValue, -180f, maxValue - 1f);
            maxValue = Mathf.Clamp(maxValue, minValue + 1f, 180f);
            _angleRangeProp.vector2Value = new Vector2(minValue, maxValue);
            EditorGUILayout.Space(2);
        }

        private void DrawEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editAngleRange ? "d_EditCollider" : "EditCollider");
            var style = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prev = GUI.color;
            if (_editAngleRange) GUI.color = Color.green;
            if (GUILayout.Button(icon, style)) _editAngleRange = !_editAngleRange;
            GUI.color = prev;
            EditorGUILayout.Space();
        }

        private void OnSceneGUI()
        {
            if (_toggle == null || _toggle.InteractableObject == null) return;

            Transform t = _toggle.InteractableObject;
            Vector3 pivot = t.position;
            var (axis, up) = _toggle.GetRotationAxis();
            float radius = HandleUtility.GetHandleSize(pivot) * 1.2f;

            Vector2 angleRange = _toggle.AngleRange;
            float minAngle = angleRange.x;
            float maxAngle = angleRange.y;

            Vector3 minDir = Quaternion.AngleAxis(minAngle, axis) * up;
            Vector3 maxDir = Quaternion.AngleAxis(maxAngle, axis) * up;
            Vector3 minPos = pivot + minDir * radius;
            Vector3 maxPos = pivot + maxDir * radius;

            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.Label(minPos, $"Min ({minAngle:F1}°)");
            Handles.Label(maxPos, $"Max ({maxAngle:F1}°)");

            int steps = _numberOfStepsProp != null ? Mathf.Max(2, _numberOfStepsProp.intValue) : 2;
            for (int i = 0; i < steps; i++)
            {
                float tNorm = (float)i / (steps - 1);
                float angle = Mathf.Lerp(minAngle, maxAngle, tNorm);
                Vector3 dir = Quaternion.AngleAxis(angle, axis) * up;
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(pivot + dir * radius, axis, radius * 0.04f);
            }

            if (!_editAngleRange) return;

            Undo.RecordObject(_toggle, "Edit Toggle Switch Limits");

            EditorGUI.BeginChangeCheck();
            Vector3 newMinPos = Handles.FreeMoveHandle(minPos,
                HandleUtility.GetHandleSize(minPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                float newMin = Vector3.SignedAngle(up, (newMinPos - pivot).normalized, axis);
                angleRange.x = Mathf.Clamp(newMin, -180f, angleRange.y - 1f);
                _angleRangeProp.vector2Value = angleRange;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_toggle);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newMaxPos = Handles.FreeMoveHandle(maxPos,
                HandleUtility.GetHandleSize(maxPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                float newMax = Vector3.SignedAngle(up, (newMaxPos - pivot).normalized, axis);
                angleRange.y = Mathf.Clamp(newMax, angleRange.x + 1f, 180f);
                _angleRangeProp.vector2Value = angleRange;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_toggle);
            }
        }
    }
}
