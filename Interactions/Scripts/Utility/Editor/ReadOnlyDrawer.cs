using UnityEngine;
using Shababeek.Utilities;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom property drawer for ReadOnly attribute that disables the field in the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnly))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Gets the height of the property field.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <summary>
        /// Renders the property field as disabled (read-only).
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}