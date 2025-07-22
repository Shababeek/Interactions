using System.Reflection;
using UnityEditor;
using UnityEngine;
using Shababeek.InteractionSystem.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(DrawerInteractable))]
    [CanEditMultipleObjects]
    public class DrawerInteractableEditor : ConstrainedInteractableEditor
    {
        private static bool editDrawerRange = false;
        private bool showEvents = true;

        // Editable properties
        private SerializedProperty interactionHandProp;
        private SerializedProperty selectionButtonProp;
        private SerializedProperty interactableObjectProp;
        private SerializedProperty snapDistanceProp;
        private SerializedProperty constraintsTypeProp;
        private SerializedProperty smoothHandTransitionProp;
        private SerializedProperty localStartProp;
        private SerializedProperty localEndProp;
        private SerializedProperty returnToOriginalProp;
        private SerializedProperty returnSpeedProp;

        // Events
        private SerializedProperty onSelectedProp;
        private SerializedProperty onDeselectedProp;
        private SerializedProperty onHoverStartProp;
        private SerializedProperty onHoverEndProp;
        private SerializedProperty onActivatedProp;
        private SerializedProperty onMovedProp;

        private SerializedProperty onLimitReachedProp;

        // Read-only
        private SerializedProperty isSelectedProp;
        private SerializedProperty currentInteractorProp;
        private SerializedProperty currentStateProp;

        protected override void OnEnable()
        {
            interactionHandProp = serializedObject.FindProperty("interactionHand");
            selectionButtonProp = serializedObject.FindProperty("selectionButton");
            interactableObjectProp = serializedObject.FindProperty("interactableObject");
            snapDistanceProp = serializedObject.FindProperty("snapDistance");
            constraintsTypeProp = serializedObject.FindProperty("constraintsType");
            smoothHandTransitionProp = serializedObject.FindProperty("smoothHandTransition");
            localStartProp = serializedObject.FindProperty("_localStart");
            localEndProp = serializedObject.FindProperty("_localEnd");
            returnToOriginalProp = serializedObject.FindProperty("returnToOriginal");
            returnSpeedProp = serializedObject.FindProperty("returnSpeed");
            onSelectedProp = serializedObject.FindProperty("onSelected");
            onDeselectedProp = serializedObject.FindProperty("onDeselected");
            onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            onActivatedProp = serializedObject.FindProperty("onActivated");
            onMovedProp = serializedObject.FindProperty("onMoved");
            onLimitReachedProp = serializedObject.FindProperty("onLimitReached");
            isSelectedProp = serializedObject.FindProperty("isSelected");
            currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            currentStateProp = serializedObject.FindProperty("currentState");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Place the object you want to move inside the 'Interactable Object' field. This object should be a child of this component.",
                MessageType.Info
            );
            serializedObject.Update();
            DoEditButton();
            // Editable properties
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
            if (localStartProp != null)
                EditorGUILayout.PropertyField(localStartProp);
            if (localEndProp != null)
                EditorGUILayout.PropertyField(localEndProp);
            if (returnToOriginalProp != null)
                EditorGUILayout.PropertyField(returnToOriginalProp, new GUIContent("Return to Original Position"));
            if (returnSpeedProp != null)
                EditorGUILayout.PropertyField(returnSpeedProp, new GUIContent("Return Speed"));
            // Events foldout
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
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
                if (onMovedProp != null)
                    EditorGUILayout.PropertyField(onMovedProp);
                if (onLimitReachedProp != null)
                    EditorGUILayout.PropertyField(onLimitReachedProp);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            #region ReadOnly
            EditorGUI.BeginDisabledGroup(true);
            if (isSelectedProp != null)

                EditorGUILayout.PropertyField(isSelectedProp);

            if (currentInteractorProp != null)

                EditorGUILayout.PropertyField(currentInteractorProp);

            if (currentStateProp != null)

                EditorGUILayout.PropertyField(currentStateProp);

            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
            #endregion

            base.OnInspectorGUI();
        }

        private static void DoEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(editDrawerRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (editDrawerRange)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                editDrawerRange = !editDrawerRange;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        protected override void OnSceneGUI()
        {
            if (!editDrawerRange) return;
            base.OnSceneGUI();
            var drawer = (DrawerInteractable)target;
            Transform t = drawer.transform;
            Undo.RecordObject(drawer, "Move Drawer Points");
            // Draw and move localStart
            Vector3 worldStart = t.TransformPoint(drawer.LocalStart);
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldStart = Handles.PositionHandle(worldStart, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                drawer.LocalStart = t.InverseTransformPoint(newWorldStart);
                EditorUtility.SetDirty(drawer);
            }

            // Draw and move localEnd
            Vector3 worldEnd = t.TransformPoint(drawer.LocalEnd);
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldEnd = Handles.PositionHandle(worldEnd, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                drawer.LocalEnd = t.InverseTransformPoint(newWorldEnd);
                EditorUtility.SetDirty(drawer);
            }
        }
    }
}