using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Shared editor base for rotary interactables. Draws control scheme, rotation axis,
    /// and conditionally hides grab mode when the wrist-rotation scheme is selected.
    /// </summary>
    [CustomEditor(typeof(RotaryInteractableBase))]
    public abstract class RotaryInteractableEditor : ConstrainedInteractableEditor
    {
        private SerializedProperty _controlSchemeProp;
        private SerializedProperty _grabModeProp;
        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _currentAngleProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _controlSchemeProp = serializedObject.FindProperty("controlScheme");
            _grabModeProp = serializedObject.FindProperty("grabMode");
            _rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            _currentAngleProp = serializedObject.FindProperty("currentAngle");
        }

        protected override void DrawCustomProperties()
        {
            base.DrawCustomProperties();

            if (_controlSchemeProp != null)
            {
                EditorGUILayout.PropertyField(_controlSchemeProp,
                    new GUIContent("Control Scheme",
                        "How rotation input is gathered: hand position around the pivot, or wrist twist around the axis."));

                var scheme = (RotaryControlScheme)_controlSchemeProp.enumValueIndex;
                string helpText = scheme == RotaryControlScheme.HandPosition
                    ? "Hand Position: hand moves around the pivot to rotate the object (large wheels, big dials)."
                    : "Hand Rotation: wrist twists around the axis to rotate the object (small dials, knobs).";
                EditorGUILayout.HelpBox(helpText, MessageType.None);
            }

            // Grab mode is only meaningful for Hand Position scheme.
            if (_grabModeProp != null && IsHandPositionScheme())
            {
                EditorGUILayout.PropertyField(_grabModeProp, new GUIContent("Grab Mode"));
            }

            if (_rotationAxisProp != null)
            {
                EditorGUILayout.PropertyField(_rotationAxisProp, new GUIContent("Rotation Axis"));
            }
        }

        protected override void DrawCustomDebugInfo()
        {
            if (_currentAngleProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_currentAngleProp, new GUIContent("Current Angle"));
                EditorGUI.EndDisabledGroup();
            }
        }

        protected bool IsHandPositionScheme()
        {
            return _controlSchemeProp != null
                && _controlSchemeProp.enumValueIndex == (int)RotaryControlScheme.HandPosition;
        }
    }
}
