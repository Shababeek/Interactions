using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Central registry of socket category names (up to 32). Acts like Unity's TagManager for sockets.
    /// One asset per project; the <see cref="SocketMask"/> property drawer resolves it via AssetDatabase.
    /// </summary>
    [CreateAssetMenu(fileName = "SocketCategoryRegistry", menuName = "Shababeek/Interactions/Socket Category Registry", order = 0)]
    public class SocketCategoryRegistry : ScriptableObject
    {
        public const int MaxCategories = 32;

        [SerializeField] private string[] names = new string[MaxCategories];

        public string GetName(int index)
        {
            if (index < 0 || index >= MaxCategories) return string.Empty;
            if (names == null || index >= names.Length) return string.Empty;
            return names[index] ?? string.Empty;
        }

        /// <summary>
        /// Returns a 32-entry display-name array for use with <see cref="UnityEditor.EditorGUI.MaskField(UnityEngine.Rect, UnityEngine.GUIContent, int, string[])"/>.
        /// Empty slots are filled with a placeholder so unnamed bits remain selectable.
        /// </summary>
        public string[] GetDisplayNames()
        {
            var arr = new string[MaxCategories];
            for (int i = 0; i < MaxCategories; i++)
            {
                var n = GetName(i);
                arr[i] = string.IsNullOrEmpty(n) ? $"<Category {i}>" : n;
            }
            return arr;
        }

        private void OnValidate()
        {
            if (names == null || names.Length != MaxCategories)
            {
                var old = names;
                names = new string[MaxCategories];
                if (old != null)
                {
                    int copy = Mathf.Min(old.Length, MaxCategories);
                    for (int i = 0; i < copy; i++) names[i] = old[i];
                }
            }
        }
    }
}
