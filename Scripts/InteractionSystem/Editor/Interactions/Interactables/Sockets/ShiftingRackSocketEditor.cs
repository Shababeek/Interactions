using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Hides the inherited multi-mask <c>acceptedCategories</c> field on <see cref="ShiftingRackSocket"/>.
    /// The rack uses its own single-category field instead.
    /// </summary>
    [CustomEditor(typeof(ShiftingRackSocket))]
    [CanEditMultipleObjects]
    public class ShiftingRackSocketEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "acceptedCategories");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
