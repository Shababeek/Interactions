using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(LeverInteractable))]
    [CanEditMultipleObjects]
    public class LeverInteractableEditor : InteractableBaseEditor
    {
        // Editable properties
        private SerializedProperty _returnToOriginalProp;
        private SerializedProperty _minProp;
        private SerializedProperty _maxProp;
        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _snapDistanceProp;

        // Events
        private SerializedProperty _onLeverChangedProp;

        // Read-only
        private SerializedProperty _currentNormalizedAngleProp;
        
        private static bool _editLeverRange = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Find custom properties
            _returnToOriginalProp = base.serializedObject.FindProperty("returnToOriginal");
            _minProp = base.serializedObject.FindProperty("min");
            _maxProp = base.serializedObject.FindProperty("max");
            _rotationAxisProp = base.serializedObject.FindProperty("rotationAxis");
            _interactableObjectProp = base.serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = base.serializedObject.FindProperty("_snapDistance");
            _onLeverChangedProp = base.serializedObject.FindProperty("onLeverChanged");
            _currentNormalizedAngleProp = base.serializedObject.FindProperty("currentNormalizedAngle");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "The LeverInteractable component creates a lever that can be pulled to different positions.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Lever Settings", EditorStyles.boldLabel);
            
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object", "The visual part of the lever object that rotates"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp, new GUIContent("Snap Distance", "Distance threshold for snapping to positions"));
            
            EditorGUILayout.Space();
            
            if (_rotationAxisProp != null)
                EditorGUILayout.PropertyField(_rotationAxisProp, new GUIContent("Rotation Axis", "The axis around which the lever rotates"));
            if (_returnToOriginalProp != null)
                EditorGUILayout.PropertyField(_returnToOriginalProp, new GUIContent("Return To Original", "Whether the lever returns to its original position when released"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);
            if (_minProp != null && _maxProp != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_minProp, new GUIContent("Min (°)"));
                EditorGUILayout.PropertyField(_maxProp, new GUIContent("Max (°)"));
                EditorGUILayout.EndHorizontal();
            }
            
            
        }

        protected override void DrawCustomEvents()
        {
            if (_onLeverChangedProp != null)
                EditorGUILayout.PropertyField(_onLeverChangedProp, new GUIContent("Current Normalized Angle", "Current lever position (0-1)"));
        }


        protected override void DrawCustomDebugInfo()
        {
            if (_currentNormalizedAngleProp != null)
                EditorGUILayout.PropertyField(_currentNormalizedAngleProp, new GUIContent("Current Normalized Angle", "Current lever position (0-1)"));
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
            var lever = (LeverInteractable)target;
            if (lever == null || lever.InteractableObject == null) return;
            
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

        protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }
    }
} 