using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Utility class providing shared functionality for editor scripts.
    /// </summary>
    public static class EditorUtilities
    {
        private static string[] _layerNames;
        
        /// <summary>
        /// Gets an array of all defined layer names in the project.
        /// Only includes layers that have actual names (not empty/unused layers).
        /// </summary>
        /// <returns>Array of layer names</returns>
        public static string[] GetDefinedLayerNames()
        {
            if (_layerNames == null)
            {
                InitializeLayerNames();
            }
            return _layerNames;
        }
        
        /// <summary>
        /// Gets the layer name for a given layer index.
        /// </summary>
        /// <param name="layerIndex">The layer index</param>
        /// <returns>The layer name, or "Layer {index}" if not found</returns>
        public static string GetLayerName(int layerIndex)
        {
            string[] names = GetDefinedLayerNames();
            if (layerIndex >= 0 && layerIndex < names.Length)
            {
                return names[layerIndex];
            }
            return $"Layer {layerIndex}";
        }
        
        /// <summary>
        /// Gets the layer index for a given layer name.
        /// </summary>
        /// <param name="layerName">The layer name to find</param>
        /// <returns>The layer index, or -1 if not found</returns>
        public static int GetLayerIndex(string layerName)
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == layerName)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Creates a layer dropdown with only defined layers.
        /// </summary>
        /// <param name="position">The position to draw the dropdown</param>
        /// <param name="label">The label for the dropdown</param>
        /// <param name="selectedIndex">The currently selected layer index</param>
        /// <returns>The new selected layer index</returns>
        public static int LayerDropdown(Rect position, GUIContent label, int selectedIndex)
        {
            string[] layerNames = GetDefinedLayerNames();
            
            // Find the display index for the selected layer
            int displayIndex = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                if (GetLayerIndex(layerNames[i]) == selectedIndex)
                {
                    displayIndex = i;
                    break;
                }
            }
            
            // Draw the dropdown
            int newDisplayIndex = EditorGUI.Popup(position, displayIndex, layerNames);
            
            // Convert back to layer index
            if (newDisplayIndex != displayIndex && newDisplayIndex >= 0 && newDisplayIndex < layerNames.Length)
            {
                return GetLayerIndex(layerNames[newDisplayIndex]);
            }
            
            return selectedIndex;
        }
        
        /// <summary>
        /// Creates a layer dropdown with only defined layers.
        /// </summary>
        /// <param name="label">The label for the dropdown</param>
        /// <param name="selectedIndex">The currently selected layer index</param>
        /// <returns>The new selected layer index</returns>
        public static int LayerDropdown(GUIContent label, int selectedIndex)
        {
            string[] layerNames = GetDefinedLayerNames();
            
            // Find the display index for the selected layer
            int displayIndex = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                if (GetLayerIndex(layerNames[i]) == selectedIndex)
                {
                    displayIndex = i;
                    break;
                }
            }
            
            // Draw the dropdown
            int newDisplayIndex = EditorGUILayout.Popup(label, displayIndex, layerNames);
            
            // Convert back to layer index
            if (newDisplayIndex != displayIndex && newDisplayIndex >= 0 && newDisplayIndex < layerNames.Length)
            {
                return GetLayerIndex(layerNames[newDisplayIndex]);
            }
            
            return selectedIndex;
        }
        
        private static void InitializeLayerNames()
        {
            var layerNamesList = new List<string>();
            
            // Add built-in layers (0-7 are always available)
            for (int i = 0; i < 8; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layerNamesList.Add(layerName);
                }
            }
            
            // Add user-defined layers (8-31)
            for (int i = 8; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layerNamesList.Add(layerName);
                }
            }
            
            // Convert to array
            _layerNames = layerNamesList.ToArray();
        }
    }
} 