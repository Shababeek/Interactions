using System.Reflection;
using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(DrawerInteractable))]
    [CanEditMultipleObjects]
    public class DrawerInteractableEditor : Editor
    {
        private static bool _editDrawerRange = false;
        private bool _showEvents = true;

        // Editable properties
        private SerializedProperty _interactionHandProp;
        private SerializedProperty _selectionButtonProp;
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _snapDistanceProp;
        private SerializedProperty _localStartProp;
        private SerializedProperty _localEndProp;
        private SerializedProperty _returnToOriginalProp;
        private SerializedProperty _returnSpeedProp;

        // Events
        private SerializedProperty _onSelectedProp;
        private SerializedProperty _onDeselectedProp;
        private SerializedProperty _onHoverStartProp;
        private SerializedProperty _onHoverEndProp;
        private SerializedProperty _onActivatedProp;
        private SerializedProperty _onMovedProp;

        private SerializedProperty _onLimitReachedProp;

        // Read-only
        private SerializedProperty _isSelectedProp;
        private SerializedProperty _currentInteractorProp;
        private SerializedProperty _currentStateProp;

        protected  void OnEnable()
        {
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");
            _interactableObjectProp = serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = serializedObject.FindProperty("snapDistance");
            _localStartProp = serializedObject.FindProperty("_localStart");
            _localEndProp = serializedObject.FindProperty("_localEnd");
            _returnToOriginalProp = serializedObject.FindProperty("returnToOriginal");
            _returnSpeedProp = serializedObject.FindProperty("returnSpeed");
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onActivatedProp = serializedObject.FindProperty("onActivated");
            _onMovedProp = serializedObject.FindProperty("onMoved");
            _onLimitReachedProp = serializedObject.FindProperty("onLimitReached");
            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Place the object you want to move inside the 'Interactable Object' field. This object should be a child of this component.\n\n" +
                "Pose constraints are automatically handled by the Pose Constrainer component (automatically added). Use it to configure hand poses and positioning.",
                MessageType.Info
            );
            serializedObject.Update();
            DoEditButton();
            if (_interactionHandProp != null)
                EditorGUILayout.PropertyField(_interactionHandProp);
            if (_selectionButtonProp != null)
                EditorGUILayout.PropertyField(_selectionButtonProp);
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp);
            if (_localStartProp != null)
                EditorGUILayout.PropertyField(_localStartProp);
            if (_localEndProp != null)
                EditorGUILayout.PropertyField(_localEndProp);
            if (_returnToOriginalProp != null)
                EditorGUILayout.PropertyField(_returnToOriginalProp, new GUIContent("Return to Original Position"));
            if (_returnSpeedProp != null)
                EditorGUILayout.PropertyField(_returnSpeedProp, new GUIContent("Return Speed"));
            // Events foldout
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
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
                if (_onMovedProp != null)
                    EditorGUILayout.PropertyField(_onMovedProp);
                if (_onLimitReachedProp != null)
                    EditorGUILayout.PropertyField(_onLimitReachedProp);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            #region ReadOnly
            EditorGUI.BeginDisabledGroup(true);
            if (_isSelectedProp != null)

                EditorGUILayout.PropertyField(_isSelectedProp);

            if (_currentInteractorProp != null)

                EditorGUILayout.PropertyField(_currentInteractorProp);

            if (_currentStateProp != null)

                EditorGUILayout.PropertyField(_currentStateProp);

            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
            #endregion

        }

        private static void DoEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editDrawerRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (_editDrawerRange)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                _editDrawerRange = !_editDrawerRange;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        protected  void OnSceneGUI()
        {
            if (!_editDrawerRange) return;
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