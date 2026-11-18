using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Base class for feedback property drawers that provides common functionality
    /// </summary>
    public abstract class FeedbackDrawerBase : PropertyDrawer
    {
        /// <summary>
        /// Gets the feedback style sheet from config or falls back to the default path
        /// </summary>
        /// <returns>The StyleSheet to use for feedback UI styling</returns>
        protected static StyleSheet GetFeedbackStyleSheet()
        {
            // Try to get from config first
            var configs = AssetDatabase.FindAssets("t:Shababeek.Interactions.Core.Config");
            if (configs.Length > 0)
            {
                var configPath = AssetDatabase.GUIDToAssetPath(configs[0]);
                var config = AssetDatabase.LoadAssetAtPath<Config>(configPath);
                if (config?.FeedbackSystemStyleSheet != null)
                {
                    return config.FeedbackSystemStyleSheet;
                }
            }

            // Fallback: find the style sheet by name, wherever the package is installed.
            StyleSheet styleSheet = null;
            foreach (var guid in AssetDatabase.FindAssets("FeedbackDrawers t:StyleSheet"))
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid));
                if (styleSheet != null) break;
            }

            if (styleSheet == null)
            {
                Debug.LogWarning("Feedback style sheet (FeedbackDrawers.uss) not found. Please assign it in the Shababeek Config asset.");
            }
            return styleSheet;
        }
    }
} 