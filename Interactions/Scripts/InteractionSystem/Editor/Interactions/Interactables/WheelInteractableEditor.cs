using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the WheelInteractable component with enhanced visualization and interactive editing.
    /// </summary>
    [CustomEditor(typeof(WheelInteractable))]
    [CanEditMultipleObjects]
    public class WheelInteractableEditor : InteractableBaseEditor
    {
        private WheelInteractable _wheelComponent;

        private SerializedProperty _rotationAxis;
        private SerializedProperty _returnToStart;
        private SerializedProperty _returnSpeed;
        private SerializedProperty _interactableObject;

        // Events
        private SerializedProperty _onWheelAngleChanged;
        private SerializedProperty _onWheelRotated;

        // Debug
        private SerializedProperty _currentAngle;
        private SerializedProperty _currentRotation;

        private void OnEnable()
        {
            base.OnEnable();
            
            _wheelComponent = (WheelInteractable)target;
            _rotationAxis = base.serializedObject.FindProperty("rotationAxis");
            _returnToStart = base.serializedObject.FindProperty("returnToStart");
            _returnSpeed = base.serializedObject.FindProperty("returnSpeed");
            _interactableObject = base.serializedObject.FindProperty("interactableObject");
            _onWheelAngleChanged = base.serializedObject.FindProperty("onWheelAngleChanged");
            _onWheelRotated = base.serializedObject.FindProperty("onWheelRotated");
            _currentAngle = base.serializedObject.FindProperty("currentAngle");
            _currentRotation = base.serializedObject.FindProperty("currentRotation");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Wheel-style interactable that tracks rotation around a single axis. Provides smooth wheel rotation tracking with full rotation counting.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Wheel Settings", EditorStyles.boldLabel);
            
            if (_interactableObject != null)
                EditorGUILayout.PropertyField(_interactableObject, new GUIContent("Interactable Object", "The visual part of the wheel object that rotates"));
            
            if (_rotationAxis != null)
                EditorGUILayout.PropertyField(_rotationAxis, new GUIContent("Rotation Axis", "The axis around which the wheel rotates."));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Wheel Behavior", EditorStyles.boldLabel);
            
            if (_returnToStart != null)
                EditorGUILayout.PropertyField(_returnToStart, new GUIContent("Return to Start", "Return to starting position when deselected."));
            
            if (_returnSpeed != null)
                EditorGUILayout.PropertyField(_returnSpeed, new GUIContent("Return Speed", "Speed of return animation (degrees per second)."));
        }

        protected override void DrawCommonEvents()
        {
            EditorGUILayout.LabelField("Wheel Events", EditorStyles.boldLabel);
            
            if (_onWheelAngleChanged != null)
                EditorGUILayout.PropertyField(_onWheelAngleChanged, new GUIContent("On Wheel Angle Changed", "Event raised when the wheel rotates (provides current angle)."));
            
            if (_onWheelRotated != null)
                EditorGUILayout.PropertyField(_onWheelRotated, new GUIContent("On Wheel Rotated", "Event raised when the wheel completes a full rotation (provides rotation count)."));
            
            EditorGUILayout.Space();
            
            // Call base to show common events
            base.DrawCommonEvents();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Wheel Debug", EditorStyles.boldLabel);
            
            if (_currentAngle != null)
                EditorGUILayout.PropertyField(_currentAngle, new GUIContent("Current Angle", "Current rotation angle in degrees (read-only)."));
            
            if (_currentRotation != null)
                EditorGUILayout.PropertyField(_currentRotation, new GUIContent("Current Rotation", "Total number of full rotations completed (read-only)."));
        }

        private void OnSceneGUI()
        {
            if (_wheelComponent == null || _wheelComponent.InteractableObject == null) return;
            
            var position = _wheelComponent.InteractableObject.position;
            var rotationAxisVector = GetRotationAxisVector();
            var radius = HandleUtility.GetHandleSize(position) * 0.8f;
            
            // Draw wheel visualization
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidDisc(position, rotationAxisVector, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(position, rotationAxisVector, radius);
            
            // Draw rotation direction indicator
            var forward = GetForwardVector();
            var right = Vector3.Cross(rotationAxisVector, forward).normalized;
            var up = Vector3.Cross(right, rotationAxisVector).normalized;
            
            Handles.color = Color.green;
            Handles.DrawLine(position, position + up * radius * 0.3f);
            Handles.Label(position + up * radius * 0.4f, "Forward");
            
            Handles.color = Color.red;
            Handles.DrawLine(position, position + right * radius * 0.3f);
            Handles.Label(position + right * radius * 0.4f, "Right");
            
            Handles.color = Color.white;
        }

        private Vector3 GetRotationAxisVector()
        {
            return _wheelComponent.RotationAxis switch
            {
                Axis.X => _wheelComponent.transform.right,
                Axis.Y => _wheelComponent.transform.up,
                Axis.Z => _wheelComponent.transform.forward,
                _ => _wheelComponent.transform.forward
            };
        }

        private Vector3 GetForwardVector()
        {
            return _wheelComponent.RotationAxis switch
            {
                Axis.X => _wheelComponent.transform.forward,
                Axis.Y => _wheelComponent.transform.forward,
                Axis.Z => _wheelComponent.transform.up,
                _ => _wheelComponent.transform.up
            };
        }
    }
}