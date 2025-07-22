using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(LeverInteractable))]
    [CanEditMultipleObjects]
    public class LeverInteractableEditor : ConstrainedInteractableEditor
    {
        // Editable properties
        private SerializedProperty returnToOriginalProp;
        private SerializedProperty minProp;
        private SerializedProperty maxProp;
        private SerializedProperty rotationAxisProp;
        private SerializedProperty interactionHandProp;
        private SerializedProperty selectionButtonProp;
        private SerializedProperty interactableObjectProp;
        private SerializedProperty snapDistanceProp;
        private SerializedProperty constraintsTypeProp;
        private SerializedProperty smoothHandTransitionProp;
        // Events
        private SerializedProperty onLeverChangedProp;
        private SerializedProperty onSelectedProp;
        private SerializedProperty onDeselectedProp;
        private SerializedProperty onHoverStartProp;
        private SerializedProperty onHoverEndProp;
        private SerializedProperty onActivatedProp;
        // Read-only
        private SerializedProperty currentNormalizedAngleProp;
        private SerializedProperty isSelectedProp;
        private SerializedProperty currentInteractorProp;
        private SerializedProperty currentStateProp;
        private bool showEvents = true;
        private static bool editLeverRange = false;

        protected override void OnEnable()
        {
            returnToOriginalProp = serializedObject.FindProperty("returnToOriginal");
            minProp = serializedObject.FindProperty("min");
            maxProp = serializedObject.FindProperty("max");
            rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            interactionHandProp = serializedObject.FindProperty("interactionHand");
            selectionButtonProp = serializedObject.FindProperty("selectionButton");
            interactableObjectProp = serializedObject.FindProperty("interactableObject");
            snapDistanceProp = serializedObject.FindProperty("snapDistance");
            constraintsTypeProp = serializedObject.FindProperty("constraintsType");
            smoothHandTransitionProp = serializedObject.FindProperty("smoothHandTransition");
            onLeverChangedProp = serializedObject.FindProperty("onLeverChanged");
            onSelectedProp = serializedObject.FindProperty("onSelected");
            onDeselectedProp = serializedObject.FindProperty("onDeselected");
            onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            onActivatedProp = serializedObject.FindProperty("onActivated");
            currentNormalizedAngleProp = serializedObject.FindProperty("currentNormalizedAngle");
            isSelectedProp = serializedObject.FindProperty("isSelected");
            currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            currentStateProp = serializedObject.FindProperty("currentState");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "The LeverInteractable component allows objects to be rotated like a lever with configurable limits and events.",
                MessageType.Info
            );
            serializedObject.Update();
            DoEditButton();
            // Editable properties
            if (returnToOriginalProp != null)
                EditorGUILayout.PropertyField(returnToOriginalProp, new GUIContent("Return to Original Position"));
            if (rotationAxisProp != null)
                EditorGUILayout.PropertyField(rotationAxisProp, new GUIContent("Rotation Axis"));
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
            // Min/Max editable fields in a single row before events
            if (minProp != null && maxProp != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(minProp, new GUIContent("Min (°)"));
                EditorGUILayout.PropertyField(maxProp, new GUIContent("Max (°)"));
                EditorGUILayout.EndHorizontal();
            }
            // Events foldout
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
                if (onLeverChangedProp != null)
                    EditorGUILayout.PropertyField(onLeverChangedProp);
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
            // Min/Max after events
            if (minProp != null)
                EditorGUILayout.PropertyField(minProp);
            if (maxProp != null)
                EditorGUILayout.PropertyField(maxProp);
            // Read-only fields at the end
            EditorGUI.BeginDisabledGroup(true);
            if (currentNormalizedAngleProp != null)
                EditorGUILayout.PropertyField(currentNormalizedAngleProp);
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

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
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

            if (!editLeverRange) return;
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
            var icon = EditorGUIUtility.IconContent(editLeverRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (editLeverRange)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                editLeverRange = !editLeverRange;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }
    }
} 