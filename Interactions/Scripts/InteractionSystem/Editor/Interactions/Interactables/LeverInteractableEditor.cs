using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(LeverInteractable))]
    [CanEditMultipleObjects]
    public class LeverInteractableEditor : Editor
    {
        // Editable properties
        private SerializedProperty _returnToOriginalProp;
        private SerializedProperty _minProp;
        private SerializedProperty _maxProp;
        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _interactionHandProp;
        private SerializedProperty _selectionButtonProp;
        private SerializedProperty _interactableObjectProp;

        private SerializedProperty _snapDistanceProp;

        // Events
        private SerializedProperty _onLeverChangedProp;
        private SerializedProperty _onSelectedProp;
        private SerializedProperty _onDeselectedProp;
        private SerializedProperty _onHoverStartProp;
        private SerializedProperty _onHoverEndProp;

        private SerializedProperty _onActivatedProp;

        // Read-only
        private SerializedProperty _currentNormalizedAngleProp;
        private SerializedProperty _isSelectedProp;
        private SerializedProperty _currentInteractorProp;
        private SerializedProperty _currentStateProp;
        private bool _showEvents = true;
        private static bool _editLeverRange = false;

        protected  void OnEnable()
        {
            _returnToOriginalProp = serializedObject.FindProperty("returnToOriginal");
            _minProp = serializedObject.FindProperty("min");
            _maxProp = serializedObject.FindProperty("max");
            _rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");
            _interactableObjectProp = serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = serializedObject.FindProperty("snapDistance");
            _onLeverChangedProp = serializedObject.FindProperty("onLeverChanged");
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onActivatedProp = serializedObject.FindProperty("onActivated");
            _currentNormalizedAngleProp = serializedObject.FindProperty("currentNormalizedAngle");
            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Configure the lever's rotation limits and behavior. The lever will rotate based on hand position.\n\n" +
                "Pose constraints are automatically handled by the UnifiedPoseConstraintSystem component (automatically added). Use it to configure hand poses and positioning.",
                MessageType.Info
            );
            serializedObject.Update();
            DoEditButton();
            // Editable properties
            if (_returnToOriginalProp != null)
                EditorGUILayout.PropertyField(_returnToOriginalProp, new GUIContent("Return to Original Position"));
            if (_rotationAxisProp != null)
                EditorGUILayout.PropertyField(_rotationAxisProp, new GUIContent("Rotation Axis"));
            if (_interactionHandProp != null)
                EditorGUILayout.PropertyField(_interactionHandProp);
            if (_selectionButtonProp != null)
                EditorGUILayout.PropertyField(_selectionButtonProp);
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp);
            // Min/Max editable fields in a single row before events
            if (_minProp != null && _maxProp != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_minProp, new GUIContent("Min (°)"));
                EditorGUILayout.PropertyField(_maxProp, new GUIContent("Max (°)"));
                EditorGUILayout.EndHorizontal();
            }
            // Events foldout
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                if (_onLeverChangedProp != null)
                    EditorGUILayout.PropertyField(_onLeverChangedProp);
                if (_onSelectedProp != null)
                    EditorGUILayout.PropertyField(_onSelectedProp);
                if (_onDeselectedProp != null)
                    EditorGUILayout.PropertyField(_onDeselectedProp);
                if (_onHoverStartProp != null)
                    EditorGUILayout.PropertyField(_onHoverStartProp);
                if (_onHoverEndProp != null)
                    EditorGUILayout.PropertyField(_onHoverEndProp);
                if (_onActivatedProp != null)
                    EditorGUILayout.PropertyField(_onActivatedProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            // Min/Max after events
            if (_minProp != null)
                EditorGUILayout.PropertyField(_minProp);
            if (_maxProp != null)
                EditorGUILayout.PropertyField(_maxProp);
            // Read-only fields at the end
            EditorGUI.BeginDisabledGroup(true);
            if (_currentNormalizedAngleProp != null)
                EditorGUILayout.PropertyField(_currentNormalizedAngleProp);
            if (_isSelectedProp != null)
                EditorGUILayout.PropertyField(_isSelectedProp);
            if (_currentInteractorProp != null)
                EditorGUILayout.PropertyField(_currentInteractorProp);
            if (_currentStateProp != null)
                EditorGUILayout.PropertyField(_currentStateProp);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        protected  void OnSceneGUI()
        {
            var lever = (LeverInteractable)target;
            Transform t = lever.transform;
            Vector3 pivot = t.position;
            Vector3 axis = lever.GetRotationAxis().plane;
            Vector3 up = GetUpVector(lever);
            float radius = HandleUtility.GetHandleSize(pivot) * 1.2f;
            float minAngle = lever.Min;
            float maxAngle = lever.Max;

            // Calculate world positions for min/max
            Quaternion minRot = Quaternion.AngleAxis(minAngle, axis);
            Quaternion maxRot = Quaternion.AngleAxis(maxAngle, axis);
            Vector3 minDir = minRot * up;
            Vector3 maxDir = maxRot * up;
            Vector3 minPos = pivot + minDir * radius;
            Vector3 maxPos = pivot + maxDir * radius;

            // Always draw arc for visual feedback
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.Label(minPos, $"Min ({lever.Min:F1}°)");
            Handles.Label(maxPos, $"Max ({lever.Max:F1}°)");

            if (!_editLeverRange) return;
            Undo.RecordObject(lever, "Edit Lever Limits");

            // Draw and move min handle (blue circle)
            Handles.color = Handles.preselectionColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newMinPos = Handles.FreeMoveHandle(minPos, HandleUtility.GetHandleSize(minPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 from = up;
                Vector3 to = (newMinPos - pivot).normalized;
                float newMin = Vector3.SignedAngle(from, to, axis);
                lever.Min = Mathf.Clamp(newMin, -180, lever.Max - 1f);
                EditorUtility.SetDirty(lever);
            }

            // Draw and move max handle (blue circle)
            Handles.color = Handles.preselectionColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newMaxPos = Handles.FreeMoveHandle(maxPos, HandleUtility.GetHandleSize(maxPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 from = up;
                Vector3 to = (newMaxPos - pivot).normalized;
                float newMax = Vector3.SignedAngle(from, to, axis);
                lever.Max = Mathf.Clamp(newMax, lever.Min + 1f, 180);
                EditorUtility.SetDirty(lever);
            }

            Handles.color = Color.white;
        }

        private Vector3 GetUpVector(LeverInteractable lever)
        {
            switch (lever.rotationAxis)
            {
                case RotationAxis.Right:
                    return lever.transform.up;
                case RotationAxis.Up:
                    return lever.transform.forward;
                case RotationAxis.Forward:
                    return lever.transform.up;
                default:
                    return lever.transform.up;
            }
        }

        private static void DoEditButton()
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
    }
} 