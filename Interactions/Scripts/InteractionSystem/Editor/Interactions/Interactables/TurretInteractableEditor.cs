using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(TurretInteractable))]
    [CanEditMultipleObjects]
    public class TurretInteractableEditor : Editor
    {
        // Editable properties
        private SerializedProperty _limitXRotationProp;
        private SerializedProperty _minXAngleProp;
        private SerializedProperty _maxXAngleProp;
        private SerializedProperty _limitYRotationProp;
        private SerializedProperty _minYAngleProp;
        private SerializedProperty _maxYAngleProp;
        private SerializedProperty _limitZRotationProp;
        private SerializedProperty _minZAngleProp;
        private SerializedProperty _maxZAngleProp;
        private SerializedProperty _returnToOriginalProp;
        private SerializedProperty _returnSpeedProp;
        private SerializedProperty _interactionHandProp;
        private SerializedProperty _selectionButtonProp;
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _snapDistanceProp;
        //events
        private SerializedProperty _onRotationChangedProp;
        private SerializedProperty _onXRotationChangedProp;
        private SerializedProperty _onYRotationChangedProp;
        private SerializedProperty _onZRotationChangedProp;
        private SerializedProperty _onSelectedProp;
        private SerializedProperty _onDeselectedProp;
        private SerializedProperty _onHoverStartProp;
        private SerializedProperty _onHoverEndProp;
        private SerializedProperty _onActivatedProp;

        // Read-only
        private SerializedProperty _currentRotationProp;
        private SerializedProperty _normalizedRotationProp;
        private SerializedProperty _isSelectedProp;
        private SerializedProperty _currentInteractorProp;
        private SerializedProperty _currentStateProp;

        private bool _showEvents = true;
        private bool _showRotationLimits = true;
        private static bool _editRotationLimits = false;

        protected  void OnEnable()
        {
            _limitXRotationProp = serializedObject.FindProperty("limitXRotation");
            _minXAngleProp = serializedObject.FindProperty("minXAngle");
            _maxXAngleProp = serializedObject.FindProperty("maxXAngle");
            _limitYRotationProp = serializedObject.FindProperty("limitYRotation");
            _minYAngleProp = serializedObject.FindProperty("minYAngle");
            _maxYAngleProp = serializedObject.FindProperty("maxYAngle");
            _limitZRotationProp = serializedObject.FindProperty("limitZRotation");
            _minZAngleProp = serializedObject.FindProperty("minZAngle");
            _maxZAngleProp = serializedObject.FindProperty("maxZAngle");
            _returnToOriginalProp = serializedObject.FindProperty("returnToOriginal");
            _returnSpeedProp = serializedObject.FindProperty("returnSpeed");
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");
            _interactableObjectProp = serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = serializedObject.FindProperty("snapDistance");
            
            _onRotationChangedProp = serializedObject.FindProperty("onRotationChanged");
            _onXRotationChangedProp = serializedObject.FindProperty("onXRotationChanged");
            _onYRotationChangedProp = serializedObject.FindProperty("onYRotationChanged");
            _onZRotationChangedProp = serializedObject.FindProperty("onZRotationChanged");
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onActivatedProp = serializedObject.FindProperty("onActivated");
            
            _currentRotationProp = serializedObject.FindProperty("currentRotation");
            _normalizedRotationProp = serializedObject.FindProperty("normalizedRotation");
            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
            
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Configure the turret's rotation limits for each axis. The turret will rotate to follow the hand position.\n\n" +
                "Pose constraints are automatically handled by the UnifiedPoseConstraintSystem component (automatically added). Use it to configure hand poses and positioning.",
                MessageType.Info
            );
            
            serializedObject.Update();
            DoEditButton();
            
            // Basic settings
            if (_interactionHandProp != null)
                EditorGUILayout.PropertyField(_interactionHandProp);
            if (_selectionButtonProp != null)
                EditorGUILayout.PropertyField(_selectionButtonProp);
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp);
            
            // Return behavior
            if (_returnToOriginalProp != null)
                EditorGUILayout.PropertyField(_returnToOriginalProp, new GUIContent("Return to Original Position"));
            if (_returnSpeedProp != null)
                EditorGUILayout.PropertyField(_returnSpeedProp, new GUIContent("Return Speed"));
            
            // Rotation limits foldout
            _showRotationLimits = EditorGUILayout.BeginFoldoutHeaderGroup(_showRotationLimits, "Rotation Limits");
            if (_showRotationLimits)
            {
                EditorGUI.indentLevel++;
                
                // X Rotation
                if (_limitXRotationProp != null)
                    EditorGUILayout.PropertyField(_limitXRotationProp, new GUIContent("Limit X Rotation"));
                if (_limitXRotationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (_minXAngleProp != null)
                        EditorGUILayout.PropertyField(_minXAngleProp, new GUIContent("Min X (°)"));
                    if (_maxXAngleProp != null)
                        EditorGUILayout.PropertyField(_maxXAngleProp, new GUIContent("Max X (°)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                // Y Rotation
                if (_limitYRotationProp != null)
                    EditorGUILayout.PropertyField(_limitYRotationProp, new GUIContent("Limit Y Rotation"));
                if (_limitYRotationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (_minYAngleProp != null)
                        EditorGUILayout.PropertyField(_minYAngleProp, new GUIContent("Min Y (°)"));
                    if (_maxYAngleProp != null)
                        EditorGUILayout.PropertyField(_maxYAngleProp, new GUIContent("Max Y (°)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                // Z Rotation
                if (_limitZRotationProp != null)
                    EditorGUILayout.PropertyField(_limitZRotationProp, new GUIContent("Limit Z Rotation"));
                if (_limitZRotationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (_minZAngleProp != null)
                        EditorGUILayout.PropertyField(_minZAngleProp, new GUIContent("Min Z (°)"));
                    if (_maxZAngleProp != null)
                        EditorGUILayout.PropertyField(_maxZAngleProp, new GUIContent("Max Z (°)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Events foldout
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                if (_onRotationChangedProp != null)
                    EditorGUILayout.PropertyField(_onRotationChangedProp);
                if (_onXRotationChangedProp != null)
                    EditorGUILayout.PropertyField(_onXRotationChangedProp);
                if (_onYRotationChangedProp != null)
                    EditorGUILayout.PropertyField(_onYRotationChangedProp);
                if (_onZRotationChangedProp != null)
                    EditorGUILayout.PropertyField(_onZRotationChangedProp);
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
            
            // Read-only fields
            EditorGUI.BeginDisabledGroup(true);
            if (_currentRotationProp != null)
                EditorGUILayout.PropertyField(_currentRotationProp);
            if (_normalizedRotationProp != null)
                EditorGUILayout.PropertyField(_normalizedRotationProp);
            if (_isSelectedProp != null)
                EditorGUILayout.PropertyField(_isSelectedProp);
            if (_currentInteractorProp != null)
                EditorGUILayout.PropertyField(_currentInteractorProp);
            if (_currentStateProp != null)
                EditorGUILayout.PropertyField(_currentStateProp);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        private static void DoEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editRotationLimits ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (_editRotationLimits)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                _editRotationLimits = !_editRotationLimits;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        protected  void OnSceneGUI()
        {
            
            if (!_editRotationLimits) return;
            
            var turret = (TurretInteractable)target;
            Transform t = turret.transform;
            Vector3 pivot = t.position;
            float radius = HandleUtility.GetHandleSize(pivot) * 1.5f;
            
            // Draw rotation limit visualization
            DrawRotationLimits(turret, pivot, radius);
        }

        private void DrawRotationLimits(TurretInteractable turret, Vector3 pivot, float radius)
        {
            // Draw X rotation limits (Red)
            if (turret.LimitXRotation)
            {
                DrawAxisLimits(turret, pivot, radius, Vector3.right, Vector3.forward, 
                    turret.MinXAngle, turret.MaxXAngle, Color.red, "X");
            }
            
            // Draw Y rotation limits (Green)
            if (turret.LimitYRotation)
            {
                DrawAxisLimits(turret, pivot, radius, Vector3.up, Vector3.forward, 
                    turret.MinYAngle, turret.MaxYAngle, Color.green, "Y");
            }
            
            // Draw Z rotation limits (Blue)
            if (turret.LimitZRotation)
            {
                DrawAxisLimits(turret, pivot, radius, Vector3.forward, Vector3.up, 
                    turret.MinZAngle, turret.MaxZAngle, Color.blue, "Z");
            }
        }
        
        private void DrawAxisLimits(TurretInteractable turret, Vector3 pivot, float radius, 
            Vector3 axis, Vector3 up, float minAngle, float maxAngle, Color color, string axisName)
        {
            // Calculate world positions for min/max
            Quaternion minRot = Quaternion.AngleAxis(minAngle, axis);
            Quaternion maxRot = Quaternion.AngleAxis(maxAngle, axis);
            Vector3 minDir = minRot * up;
            Vector3 maxDir = maxRot * up;
            Vector3 minPos = pivot + minDir * radius;
            Vector3 maxPos = pivot + maxDir * radius;

            // Draw arc for visual feedback
            Handles.color = new Color(color.r, color.g, color.b, 0.2f);
            Handles.DrawSolidArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.color = color;
            Handles.DrawWireArc(pivot, axis, minDir, maxAngle - minAngle, radius);
            Handles.Label(minPos, $"Min {axisName} ({minAngle:F1}°)");
            Handles.Label(maxPos, $"Max {axisName} ({maxAngle:F1}°)");

            if (!_editRotationLimits) return;
            Undo.RecordObject(turret, $"Edit {axisName} Rotation Limits");

            // Draw and move min handle
            Handles.color = Handles.preselectionColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newMinPos = Handles.FreeMoveHandle(minPos, HandleUtility.GetHandleSize(minPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 from = up;
                Vector3 to = (newMinPos - pivot).normalized;
                float newMin = Vector3.SignedAngle(from, to, axis);
                
                // Update the appropriate property based on axis
                switch (axisName)
                {
                    case "X":
                        turret.SetMinXAngle(Mathf.Clamp(newMin, -180, turret.MaxXAngle - 1f));
                        break;
                    case "Y":
                        turret.SetMinYAngle(Mathf.Clamp(newMin, -180, turret.MaxYAngle - 1f));
                        break;
                    case "Z":
                        turret.SetMinZAngle(Mathf.Clamp(newMin, -180, turret.MaxZAngle - 1f));
                        break;
                }
                EditorUtility.SetDirty(turret);
            }

            // Draw and move max handle
            Handles.color = Handles.preselectionColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newMaxPos = Handles.FreeMoveHandle(maxPos, HandleUtility.GetHandleSize(maxPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 from = up;
                Vector3 to = (newMaxPos - pivot).normalized;
                float newMax = Vector3.SignedAngle(from, to, axis);
                
                // Update the appropriate property based on axis
                switch (axisName)
                {
                    case "X":
                        turret.SetMaxXAngle(Mathf.Clamp(newMax, turret.MinXAngle + 1f, 180));
                        break;
                    case "Y":
                        turret.SetMaxYAngle(Mathf.Clamp(newMax, turret.MinYAngle + 1f, 180));
                        break;
                    case "Z":
                        turret.SetMaxZAngle(Mathf.Clamp(newMax, turret.MinZAngle + 1f, 180));
                        break;
                }
                EditorUtility.SetDirty(turret);
            }
        }
    }
} 