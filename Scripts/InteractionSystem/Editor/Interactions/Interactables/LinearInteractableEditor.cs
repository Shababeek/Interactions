using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Shared editor base for linear interactables. Draws localStart/localEnd, the rail visualization,
    /// and an optional edit mode for moving the start/end points in the scene view.
    /// </summary>
    [CustomEditor(typeof(LinearInteractableBase))]
    public abstract class LinearInteractableEditor : ConstrainedInteractableEditor
    {
        private SerializedProperty _localStartProp;
        private SerializedProperty _localEndProp;
        private SerializedProperty _currentNormalizedProp;

        private bool _editRailRange;

        protected override void OnEnable()
        {
            base.OnEnable();
            _localStartProp = serializedObject.FindProperty("localStart");
            _localEndProp = serializedObject.FindProperty("localEnd");
            _currentNormalizedProp = serializedObject.FindProperty("currentNormalized");
        }

        protected override void DrawCustomProperties()
        {
            base.DrawCustomProperties();

            EditorGUILayout.LabelField("Movement Range", EditorStyles.boldLabel);
            if (_localStartProp != null)
                EditorGUILayout.PropertyField(_localStartProp,
                    new GUIContent("Local Start", "Local-space position when normalized = 0."));
            if (_localEndProp != null)
                EditorGUILayout.PropertyField(_localEndProp,
                    new GUIContent("Local End", "Local-space position when normalized = 1."));
        }

        protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }

        protected override void DrawCustomDebugInfo()
        {
            if (_currentNormalizedProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_currentNormalizedProp,
                    new GUIContent("Current Normalized", "Current handle position along the rail (0-1)."));
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editRailRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };

            Color prevColor = GUI.color;
            if (_editRailRange) GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                _editRailRange = !_editRailRange;
            GUI.color = prevColor;

            EditorGUILayout.Space();
        }

        protected void DrawRailHandles()
        {
            var component = (LinearInteractableBase)target;
            if (component == null || component.InteractableObject == null) return;

            var worldStart = component.transform.TransformPoint(component.LocalStart);
            var worldEnd = component.transform.TransformPoint(component.LocalEnd);
            var radius = HandleUtility.GetHandleSize(worldStart) * 0.1f;

            Handles.color = Color.green;
            Handles.DrawLine(worldStart, worldEnd);

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, worldStart, Quaternion.identity, radius * 0.5f, EventType.Repaint);
            Handles.SphereHandleCap(0, worldEnd, Quaternion.identity, radius * 0.5f, EventType.Repaint);

            Handles.Label(worldStart + Vector3.up * radius * 1.5f, "Start");
            Handles.Label(worldEnd + Vector3.up * radius * 1.5f, "End");

            if (_editRailRange)
            {
                EditorGUI.BeginChangeCheck();
                Handles.color = Color.yellow;
                var newStart = Handles.FreeMoveHandle(worldStart, radius, Vector3.zero, Handles.SphereHandleCap);
                var newEnd = Handles.FreeMoveHandle(worldEnd, radius, Vector3.zero, Handles.SphereHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(component, "Edit Rail Range");
                    _localStartProp.vector3Value = component.transform.InverseTransformPoint(newStart);
                    _localEndProp.vector3Value = component.transform.InverseTransformPoint(newEnd);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(component);
                }
            }

            // Current handle position
            if (component.InteractableObject != null)
            {
                var localObjPos = component.InteractableObject.transform.localPosition;
                var direction = component.LocalEnd - component.LocalStart;
                if (direction.sqrMagnitude > 1e-6f)
                {
                    var projected = Vector3.Project(localObjPos - component.LocalStart, direction) + component.LocalStart;
                    var worldProjected = component.transform.TransformPoint(projected);
                    Handles.color = Color.cyan;
                    Handles.SphereHandleCap(0, worldProjected, Quaternion.identity, radius * 0.5f, EventType.Repaint);
                    Handles.Label(worldProjected + Vector3.up * radius * 1.2f, "Current");
                }
            }

            Handles.color = Color.white;
        }
    }
}
