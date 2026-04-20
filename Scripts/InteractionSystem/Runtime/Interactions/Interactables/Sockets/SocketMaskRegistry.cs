using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Central registry of socket mask bit names (up to 32). Acts like Unity's TagManager for sockets.
    /// Assigned on <see cref="Core.Config"/> so the whole project shares one source of truth.
    /// </summary>
    [CreateAssetMenu(fileName = "SocketMaskRegistry", menuName = "Shababeek/Interactions/Socket Mask Registry", order = 0)]
    public class SocketMaskRegistry : ScriptableObject
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
                arr[i] = string.IsNullOrEmpty(n) ? $"<Mask {i}>" : n;
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
