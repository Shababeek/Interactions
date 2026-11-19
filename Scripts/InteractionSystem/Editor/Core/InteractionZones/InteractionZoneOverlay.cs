using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Interactions.Core.Editors
{
    /// <summary>Scene view panel that shows or hides parts of the interaction zones while a rig is selected.</summary>
    [Overlay(typeof(SceneView), "shababeek-interaction-zones", "Interaction Zones", true)]
    public class InteractionZoneOverlay : Overlay, ITransientOverlay
    {
        private static readonly InteractionZoneLayers[] LayerOrder =
        {
            InteractionZoneLayers.Optimal,
            InteractionZoneLayers.Extended,
            InteractionZoneLayers.Maximum,
            InteractionZoneLayers.PlayArea,
            InteractionZoneLayers.DeadZone,
            InteractionZoneLayers.Player,
            InteractionZoneLayers.Heights,
            InteractionZoneLayers.Labels
        };

        private readonly Dictionary<InteractionZoneLayers, Toggle> _toggles = new();
        private Toggle _unselectedToggle;
        private bool _rigSelected;

        /// <summary>The panel only appears while a rig with zones is selected.</summary>
        public bool visible => _rigSelected;

        /// <inheritdoc/>
        public override void OnCreated()
        {
            Selection.selectionChanged += RefreshSelection;
            InteractionZoneViewSettings.Changed += RefreshToggles;
            RefreshSelection();
        }

        /// <inheritdoc/>
        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= RefreshSelection;
            InteractionZoneViewSettings.Changed -= RefreshToggles;
        }

        /// <inheritdoc/>
        public override VisualElement CreatePanelContent()
        {
            _toggles.Clear();
            var root = new VisualElement();
            root.style.minWidth = 170;
            root.style.paddingLeft = root.style.paddingRight = 4;

            foreach (var layer in LayerOrder)
            {
                var captured = layer;
                var toggle = CreateToggle(InteractionZoneViewSettings.GetLabel(layer),
                    InteractionZoneViewSettings.IsVisible(layer),
                    value => InteractionZoneViewSettings.SetVisible(captured, value));
                toggle.Insert(0, CreateSwatch(InteractionZoneViewSettings.GetColor(layer)));
                _toggles[layer] = toggle;
                root.Add(toggle);
            }

            _unselectedToggle = CreateToggle("Show When Not Selected",
                InteractionZoneViewSettings.ShowWhenUnselected,
                value => InteractionZoneViewSettings.ShowWhenUnselected = value);
            _unselectedToggle.style.marginTop = 6;
            root.Add(_unselectedToggle);
            return root;
        }

        private static Toggle CreateToggle(string label, bool value, System.Action<bool> onChanged)
        {
            var toggle = new Toggle { text = label, value = value };
            toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return toggle;
        }

        private static VisualElement CreateSwatch(Color color)
        {
            var swatch = new VisualElement();
            swatch.style.width = swatch.style.height = 10;
            swatch.style.marginRight = 4;
            swatch.style.alignSelf = Align.Center;
            swatch.style.backgroundColor = color;
            swatch.style.borderTopLeftRadius = swatch.style.borderTopRightRadius = 2;
            swatch.style.borderBottomLeftRadius = swatch.style.borderBottomRightRadius = 2;
            return swatch;
        }

        private void RefreshSelection()
        {
            var selected = Selection.activeGameObject;
            _rigSelected = selected && selected.GetComponentInParent<VRInteractionZoneVisualizer>();
        }

        private void RefreshToggles()
        {
            foreach (var pair in _toggles)
                pair.Value.SetValueWithoutNotify(InteractionZoneViewSettings.IsVisible(pair.Key));
            _unselectedToggle?.SetValueWithoutNotify(InteractionZoneViewSettings.ShowWhenUnselected);
        }
    }
}
