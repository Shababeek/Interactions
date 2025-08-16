using System.Reflection;
using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(DrawerInteractable))]
    [CanEditMultipleObjects]
    public class DrawerInteractableEditor : InteractableBaseEditor
    {
        private static bool _editDrawerRange = false;

        // Custom properties specific to DrawerInteractable
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _snapDistanceProp;
        private SerializedProperty _localStartProp;
        private SerializedProperty _localEndProp;
        private SerializedProperty _returnToOriginalProp;
        private SerializedProperty _returnSpeedProp;
        private SerializedProperty _onMovedProp;
        private SerializedProperty _onLimitReachedProp;
        private SerializedProperty _onValueChangedProp;
        private SerializedProperty _currentValueProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Find custom properties
            _interactableObjectProp = base.serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = base.serializedObject.FindProperty("_snapDistance");
            _localStartProp = base.serializedObject.FindProperty("_localStart");
            _localEndProp = base.serializedObject.FindProperty("_localEnd");
            _returnToOriginalProp = base.serializedObject.FindProperty("returnToOriginal");
            _returnSpeedProp = base.serializedObject.FindProperty("returnSpeed");
            _onMovedProp = base.serializedObject.FindProperty("onMoved");
            _onLimitReachedProp = base.serializedObject.FindProperty("onLimitReached");
            _onValueChangedProp = base.serializedObject.FindProperty("onValueChanged");
            _currentValueProp = base.serializedObject.FindProperty("currentValue");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Place the object you want to move inside the 'Interactable Object' field. This object should be a child of this component.\n\n" +
                "Pose constraints are automatically handled by the Pose Constrainer component (automatically added). Use it to configure hand poses and positioning.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Drawer Settings", EditorStyles.boldLabel);

            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object", "The object that will be moved by the drawer"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp, new GUIContent("Snap Distance", "Distance threshold for snapping to positions"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Movement Range", EditorStyles.boldLabel);
            if (_localStartProp != null)
                EditorGUILayout.PropertyField(_localStartProp, new GUIContent("Local Start", "Local position where the drawer starts"));
            if (_localEndProp != null)
                EditorGUILayout.PropertyField(_localEndProp, new GUIContent("Local End", "Local position where the drawer ends"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Behavior", EditorStyles.boldLabel);
            if (_returnToOriginalProp != null)
                EditorGUILayout.PropertyField(_returnToOriginalProp, new GUIContent("Return To Original", "Whether the drawer returns to its original position when released"));
            if (_returnSpeedProp != null)
                EditorGUILayout.PropertyField(_returnSpeedProp, new GUIContent("Return Speed", "Speed of return animation"));

            // Draw range editing button
        }

        protected override void DrawCommonEvents()
        {
            EditorGUILayout.LabelField("Drawer Events", EditorStyles.boldLabel);

            if (_onMovedProp != null)
                EditorGUILayout.PropertyField(_onMovedProp, new GUIContent("On Moved", "Event raised when the drawer moves"));
            if (_onLimitReachedProp != null)
                EditorGUILayout.PropertyField(_onLimitReachedProp, new GUIContent("On Limit Reached", "Event raised when the drawer reaches its limits"));
            if (_onValueChangedProp != null)
                EditorGUILayout.PropertyField(_onValueChangedProp, new GUIContent("On Value Changed", "Event raised when the drawer value changes"));

            EditorGUILayout.Space();

            // Call base to show common events
            base.DrawCommonEvents();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Drawer Debug", EditorStyles.boldLabel);

            if (_currentValueProp != null)
                EditorGUILayout.PropertyField(_currentValueProp, new GUIContent("Current Value", "Current drawer position (0-1)"));
        }

        private void DrawEditButton()
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

        private void OnSceneGUI()
        {
            var drawer = (DrawerInteractable)target;
            if (drawer == null || drawer.InteractableObject == null) return;

            var worldStart = drawer.transform.TransformPoint(drawer.LocalStart);
            var worldEnd = drawer.transform.TransformPoint(drawer.LocalEnd);
            var direction = worldEnd - worldStart;
            var radius = HandleUtility.GetHandleSize(worldStart) * 0.1f;

            // Draw movement path
            Handles.color = Color.green;
            Handles.DrawLine(worldStart, worldEnd);

            // Draw start and end points
            Handles.color = Color.red;
            Handles.DrawSolidDisc(worldStart, Vector3.up, radius);
            Handles.DrawSolidDisc(worldEnd, Vector3.up, radius);

            // Draw labels
            Handles.Label(worldStart + Vector3.up * radius * 1.5f, "Start");
            Handles.Label(worldEnd + Vector3.up * radius * 1.5f, "End");

            if (_editDrawerRange)
            {
                // Draw interactive handles for editing the range
                EditorGUI.BeginChangeCheck();

                Handles.color = Color.yellow;
                var newStart = Handles.FreeMoveHandle(worldStart, HandleUtility.GetHandleSize(worldStart) * 0.1f, Vector3.zero, Handles.SphereHandleCap);
                var newEnd = Handles.FreeMoveHandle(worldEnd, HandleUtility.GetHandleSize(worldEnd) * 0.1f, Vector3.zero, Handles.SphereHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(drawer, "Edit Drawer Range");

                    var localStart = drawer.transform.InverseTransformPoint(newStart);
                    var localEnd = drawer.transform.InverseTransformPoint(newEnd);

                    // Use reflection to set private fields
                    var startField = typeof(DrawerInteractable).GetField("_localStart", BindingFlags.NonPublic | BindingFlags.Instance);
                    var endField = typeof(DrawerInteractable).GetField("_localEnd", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (startField != null) startField.SetValue(drawer, localStart);
                    if (endField != null) endField.SetValue(drawer, localEnd);

                    EditorUtility.SetDirty(drawer);
                }
            }

            // Draw current position indicator
            if (drawer.InteractableObject != null)
            {
                var localObjPos = drawer.InteractableObject.transform.localPosition;
                var projectedPoint = Vector3.Project(localObjPos - drawer.LocalStart, drawer.LocalEnd - drawer.LocalStart) + drawer.LocalStart;
                var worldProjected = drawer.transform.TransformPoint(projectedPoint);

                Handles.color = Color.cyan;
                Handles.DrawSolidDisc(worldProjected, Vector3.up, radius * 0.5f);
                Handles.Label(worldProjected + Vector3.up * radius * 1.2f, "Current");
            }

            Handles.color = Color.white;
        }

        protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }
    }

}