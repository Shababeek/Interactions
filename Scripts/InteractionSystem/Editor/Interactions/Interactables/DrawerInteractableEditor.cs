using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(DrawerInteractable))]
    [CanEditMultipleObjects]
    public class DrawerInteractableEditor : LinearInteractableEditor
    {
        private SerializedProperty _onMovedProp;
        private SerializedProperty _onOpenedProp;
        private SerializedProperty _onClosedProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _onMovedProp = serializedObject.FindProperty("onMoved");
            _onOpenedProp = serializedObject.FindProperty("onOpened");
            _onClosedProp = serializedObject.FindProperty("onClosed");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Drawer interactable. Drops the handle into the rail between Local Start and Local End.\n" +
                "Pose constraints are configured on the Pose Constrainer component.",
                MessageType.Info);
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Drawer Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onMovedProp);
            EditorGUILayout.PropertyField(_onOpenedProp);
            EditorGUILayout.PropertyField(_onClosedProp);
        }

        private void OnSceneGUI()
        {
            DrawRailHandles();
        }
    }
}
