using System;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Core.Editors
{
    /// <summary>Parts of the interaction zone drawing that can be shown or hidden in the Scene view.</summary>
    [Flags]
    public enum InteractionZoneLayers
    {
        None = 0,
        PlayArea = 1 << 0,
        Optimal = 1 << 1,
        Extended = 1 << 2,
        Maximum = 1 << 3,
        DeadZone = 1 << 4,
        Player = 1 << 5,
        Heights = 1 << 6,
        Labels = 1 << 7,
        All = ~0
    }

    /// <summary>Per-user Scene view options for the interaction zones. Stored in EditorPrefs, never in a scene or prefab.</summary>
    public static class InteractionZoneViewSettings
    {
        private const string LayersKey = "Shababeek.InteractionZones.Layers";
        private const string UnselectedKey = "Shababeek.InteractionZones.ShowWhenUnselected";

        private static bool _loaded;
        private static InteractionZoneLayers _layers;
        private static bool _showWhenUnselected;

        /// <summary>Raised after any option changes.</summary>
        public static event Action Changed;

        /// <summary>Layers currently drawn.</summary>
        public static InteractionZoneLayers Layers
        {
            get
            {
                Load();
                return _layers;
            }
            set
            {
                Load();
                if (_layers == value) return;
                _layers = value;
                EditorPrefs.SetInt(LayersKey, (int)value);
                NotifyChanged();
            }
        }

        /// <summary>Whether a faint footprint is drawn while the rig is not selected.</summary>
        public static bool ShowWhenUnselected
        {
            get
            {
                Load();
                return _showWhenUnselected;
            }
            set
            {
                Load();
                if (_showWhenUnselected == value) return;
                _showWhenUnselected = value;
                EditorPrefs.SetBool(UnselectedKey, value);
                NotifyChanged();
            }
        }

        /// <summary>Returns true when the layer is drawn.</summary>
        public static bool IsVisible(InteractionZoneLayers layer) => (Layers & layer) != 0;

        /// <summary>Shows or hides one layer.</summary>
        public static void SetVisible(InteractionZoneLayers layer, bool visible)
        {
            Layers = visible ? Layers | layer : Layers & ~layer;
        }

        /// <summary>Display name of a layer.</summary>
        public static string GetLabel(InteractionZoneLayers layer)
        {
            switch (layer)
            {
                case InteractionZoneLayers.PlayArea: return "Play Area";
                case InteractionZoneLayers.Optimal: return "Optimal";
                case InteractionZoneLayers.Extended: return "Extended Reach";
                case InteractionZoneLayers.Maximum: return "Maximum Reach";
                case InteractionZoneLayers.DeadZone: return "Dead Zone";
                case InteractionZoneLayers.Player: return "Player";
                case InteractionZoneLayers.Heights: return "Height Ranges";
                case InteractionZoneLayers.Labels: return "Labels";
                default: return layer.ToString();
            }
        }

        /// <summary>Colour a layer is drawn with.</summary>
        public static Color GetColor(InteractionZoneLayers layer)
        {
            switch (layer)
            {
                case InteractionZoneLayers.PlayArea: return new Color(0.25f, 0.75f, 1f);
                case InteractionZoneLayers.Optimal: return new Color(0.3f, 0.85f, 0.4f);
                case InteractionZoneLayers.Extended: return new Color(1f, 0.8f, 0.2f);
                case InteractionZoneLayers.Maximum: return new Color(1f, 0.5f, 0.15f);
                case InteractionZoneLayers.DeadZone: return new Color(0.95f, 0.25f, 0.25f);
                default: return new Color(0.75f, 0.8f, 0.9f);
            }
        }

        private static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            _layers = (InteractionZoneLayers)EditorPrefs.GetInt(LayersKey, (int)InteractionZoneLayers.All);
            _showWhenUnselected = EditorPrefs.GetBool(UnselectedKey, true);
        }

        private static void NotifyChanged()
        {
            SceneView.RepaintAll();
            Changed?.Invoke();
        }
    }
}
