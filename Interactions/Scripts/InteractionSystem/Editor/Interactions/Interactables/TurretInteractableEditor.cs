using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(TurretInteractable))]
    [CanEditMultipleObjects]
    public class TurretInteractableEditor : ConstrainedInteractableEditor
    {
        // Editable properties
        private SerializedProperty limitXRotationProp;
        private SerializedProperty minXAngleProp;
        private SerializedProperty maxXAngleProp;
        private SerializedProperty limitYRotationProp;
        private SerializedProperty minYAngleProp;
        private SerializedProperty maxYAngleProp;
        private SerializedProperty limitZRotationProp;
        private SerializedProperty minZAngleProp;
        private SerializedProperty maxZAngleProp;
        private SerializedProperty returnToOriginalProp;
        private SerializedProperty returnSpeedProp;
        private SerializedProperty interactionHandProp;
        private SerializedProperty selectionButtonProp;
        private SerializedProperty interactableObjectProp;
        private SerializedProperty snapDistanceProp;
        private SerializedProperty constraintsTypeProp;
        private SerializedProperty smoothHandTransitionProp;

        // Events
        private SerializedProperty onRotationChangedProp;
        private SerializedProperty onXRotationChangedProp;
        private SerializedProperty onYRotationChangedProp;
        private SerializedProperty onZRotationChangedProp;
        private SerializedProperty onSelectedProp;
        private SerializedProperty onDeselectedProp;
        private SerializedProperty onHoverStartProp;
        private SerializedProperty onHoverEndProp;
        private SerializedProperty onActivatedProp;

        // Read-only
        private SerializedProperty currentRotationProp;
        private SerializedProperty normalizedRotationProp;
        private SerializedProperty isSelectedProp;
        private SerializedProperty currentInteractorProp;
        private SerializedProperty currentStateProp;

        private bool showEvents = true;
        private bool showRotationLimits = true;
        private static bool editRotationLimits = false;

        protected override void OnEnable()
        {
            limitXRotationProp = serializedObject.FindProperty("limitXRotation");
            minXAngleProp = serializedObject.FindProperty("minXAngle");
            maxXAngleProp = serializedObject.FindProperty("maxXAngle");
            limitYRotationProp = serializedObject.FindProperty("limitYRotation");
            minYAngleProp = serializedObject.FindProperty("minYAngle");
            maxYAngleProp = serializedObject.FindProperty("maxYAngle");
            limitZRotationProp = serializedObject.FindProperty("limitZRotation");
            minZAngleProp = serializedObject.FindProperty("minZAngle");
            maxZAngleProp = serializedObject.FindProperty("maxZAngle");
            returnToOriginalProp = serializedObject.FindProperty("returnToOriginal");
            returnSpeedProp = serializedObject.FindProperty("returnSpeed");
            interactionHandProp = serializedObject.FindProperty("interactionHand");
            selectionButtonProp = serializedObject.FindProperty("selectionButton");
            interactableObjectProp = serializedObject.FindProperty("interactableObject");
            snapDistanceProp = serializedObject.FindProperty("snapDistance");
            constraintsTypeProp = serializedObject.FindProperty("constraintsType");
            smoothHandTransitionProp = serializedObject.FindProperty("smoothHandTransition");
            
            onRotationChangedProp = serializedObject.FindProperty("onRotationChanged");
            onXRotationChangedProp = serializedObject.FindProperty("onXRotationChanged");
            onYRotationChangedProp = serializedObject.FindProperty("onYRotationChanged");
            onZRotationChangedProp = serializedObject.FindProperty("onZRotationChanged");
            onSelectedProp = serializedObject.FindProperty("onSelected");
            onDeselectedProp = serializedObject.FindProperty("onDeselected");
            onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            onActivatedProp = serializedObject.FindProperty("onActivated");
            
            currentRotationProp = serializedObject.FindProperty("currentRotation");
            normalizedRotationProp = serializedObject.FindProperty("normalizedRotation");
            isSelectedProp = serializedObject.FindProperty("isSelected");
            currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            currentStateProp = serializedObject.FindProperty("currentState");
            
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "The TurretInteractable component allows objects to rotate around multiple axes like a camera tripod or turret gun.",
                MessageType.Info
            );
            
            serializedObject.Update();
            DoEditButton();
            
            // Basic settings
            if (interactionHandProp != null)
                EditorGUILayout.PropertyField(interactionHandProp);
            if (selectionButtonProp != null)
                EditorGUILayout.PropertyField(selectionButtonProp);
            if (interactableObjectProp != null)
                EditorGUILayout.PropertyField(interactableObjectProp, new GUIContent("Interactable Object"));
            if (snapDistanceProp != null)
                EditorGUILayout.PropertyField(snapDistanceProp);
            if (constraintsTypeProp != null)
                EditorGUILayout.PropertyField(constraintsTypeProp);
            if (smoothHandTransitionProp != null)
                EditorGUILayout.PropertyField(smoothHandTransitionProp);
            
            // Return behavior
            if (returnToOriginalProp != null)
                EditorGUILayout.PropertyField(returnToOriginalProp, new GUIContent("Return to Original Position"));
            if (returnSpeedProp != null)
                EditorGUILayout.PropertyField(returnSpeedProp, new GUIContent("Return Speed"));
            
            // Rotation limits foldout
            showRotationLimits = EditorGUILayout.BeginFoldoutHeaderGroup(showRotationLimits, "Rotation Limits");
            if (showRotationLimits)
            {
                EditorGUI.indentLevel++;
                
                // X Rotation
                if (limitXRotationProp != null)
                    EditorGUILayout.PropertyField(limitXRotationProp, new GUIContent("Limit X Rotation"));
                if (limitXRotationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (minXAngleProp != null)
                        EditorGUILayout.PropertyField(minXAngleProp, new GUIContent("Min X (°)"));
                    if (maxXAngleProp != null)
                        EditorGUILayout.PropertyField(maxXAngleProp, new GUIContent("Max X (°)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                // Y Rotation
                if (limitYRotationProp != null)
                    EditorGUILayout.PropertyField(limitYRotationProp, new GUIContent("Limit Y Rotation"));
                if (limitYRotationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (minYAngleProp != null)
                        EditorGUILayout.PropertyField(minYAngleProp, new GUIContent("Min Y (°)"));
                    if (maxYAngleProp != null)
                        EditorGUILayout.PropertyField(maxYAngleProp, new GUIContent("Max Y (°)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                // Z Rotation
                if (limitZRotationProp != null)
                    EditorGUILayout.PropertyField(limitZRotationProp, new GUIContent("Limit Z Rotation"));
                if (limitZRotationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (minZAngleProp != null)
                        EditorGUILayout.PropertyField(minZAngleProp, new GUIContent("Min Z (°)"));
                    if (maxZAngleProp != null)
                        EditorGUILayout.PropertyField(maxZAngleProp, new GUIContent("Max Z (°)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Events foldout
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
                if (onRotationChangedProp != null)
                    EditorGUILayout.PropertyField(onRotationChangedProp);
                if (onXRotationChangedProp != null)
                    EditorGUILayout.PropertyField(onXRotationChangedProp);
                if (onYRotationChangedProp != null)
                    EditorGUILayout.PropertyField(onYRotationChangedProp);
                if (onZRotationChangedProp != null)
                    EditorGUILayout.PropertyField(onZRotationChangedProp);
                if (onSelectedProp != null)
                    EditorGUILayout.PropertyField(onSelectedProp);
                if (onDeselectedProp != null)
                    EditorGUILayout.PropertyField(onDeselectedProp);
                if (onHoverStartProp != null)
                    EditorGUILayout.PropertyField(onHoverStartProp);
                if (onHoverEndProp != null)
                    EditorGUILayout.PropertyField(onHoverEndProp);
                if (onActivatedProp != null)
                    EditorGUILayout.PropertyField(onActivatedProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Read-only fields
            EditorGUI.BeginDisabledGroup(true);
            if (currentRotationProp != null)
                EditorGUILayout.PropertyField(currentRotationProp);
            if (normalizedRotationProp != null)
                EditorGUILayout.PropertyField(normalizedRotationProp);
            if (isSelectedProp != null)
                EditorGUILayout.PropertyField(isSelectedProp);
            if (currentInteractorProp != null)
                EditorGUILayout.PropertyField(currentInteractorProp);
            if (currentStateProp != null)
                EditorGUILayout.PropertyField(currentStateProp);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        private static void DoEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(editRotationLimits ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (editRotationLimits)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                editRotationLimits = !editRotationLimits;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            
            if (!editRotationLimits) return;
            
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

            if (!editRotationLimits) return;
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