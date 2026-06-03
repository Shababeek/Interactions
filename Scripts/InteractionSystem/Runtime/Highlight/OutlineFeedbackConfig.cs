using System;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Shared configuration hub for <see cref="InteractableOutlineFeedback"/>.
    /// One asset holds the colors, widths, modes, and enable toggles for every outline
    /// state (hover, selected, grab hint) so the whole project shares one source of truth.
    /// Assign the same asset to every feedback component (or set a project-wide default).
    /// </summary>
    [CreateAssetMenu(fileName = "OutlineFeedbackConfig", menuName = "Shababeek/Interactions/Outline Feedback Config", order = 1)]
    public class OutlineFeedbackConfig : ScriptableObject
    {
        /// <summary>Color/width/mode settings for a single outline state.</summary>
        [Serializable]
        public class OutlineState
        {
            [Tooltip("Master switch for this state. When off the outline stays hidden for this state.")]
            public bool enabled = true;
            public Color color = new Color(1f, 0.85f, 0.2f, 0.2f);
            [Range(0f, 5f)] public float width = 0.1f;
            public Outline.Mode mode = Outline.Mode.OutlineVisible;
        }

        [Tooltip("Global kill switch. When off no outline feedback is shown by any component using this config.")]
        [SerializeField] private bool feedbackEnabled = true;

        [Header("Hover")]
        [SerializeField] private OutlineState hover = new()
        {
            color = new Color(1f, 0.85f, 0.2f, 0.2f),
            width = 0.1f,
            mode = Outline.Mode.OutlineVisible
        };

        [Header("Selected")]
        [Tooltip("Keep outline visible while held.")]
        [SerializeField] private OutlineState selected = new()
        {
            color = new Color(0.2f, 0.9f, 1f, 0.2f),
            width = 0.1f,
            mode = Outline.Mode.OutlineVisible
        };

        [Header("Grab Hint (Idle)")]
        [Tooltip("Pulsing outline shown when idle (not hovered/selected) to hint the object is grabbable. 'enabled' here acts as the global default for showing the hint.")]
        [SerializeField] private OutlineState hint = new()
        {
            enabled = false,
            color = new Color(1f, 1f, 1f, 0.1f),
            width = 0.02f,
            mode = Outline.Mode.OutlineVisible
        };
        [Tooltip("Minimum outline width during the pulse cycle.")]
        [SerializeField, Range(0f, 5f)] private float hintMinWidth = 0.02f;
        [Tooltip("Maximum outline width during the pulse cycle.")]
        [SerializeField, Range(0f, 5f)] private float hintMaxWidth = 0.08f;
        [Tooltip("Pulses per second.")]
        [SerializeField, Range(0.1f, 5f)] private float hintPulseSpeed = 1f;

        /// <summary>Global kill switch for all outline feedback driven by this config.</summary>
        public bool FeedbackEnabled => feedbackEnabled;

        public OutlineState Hover => hover;
        public OutlineState Selected => selected;
        public OutlineState Hint => hint;

        public float HintMinWidth => hintMinWidth;
        public float HintMaxWidth => hintMaxWidth;
        public float HintPulseSpeed => hintPulseSpeed;

        /// <summary>Resources path (without extension) of the project-wide default config asset.</summary>
        public const string DefaultResourcePath = "OutlineFeedbackConfig";

        private static OutlineFeedbackConfig _default;
        private static bool _defaultLoaded;

        /// <summary>
        /// Project-wide default config, lazily loaded from
        /// <c>Resources/OutlineFeedbackConfig</c>. Components with no explicit config use this,
        /// so a single asset configures every outline in the project with zero wiring.
        /// Returns null (and components fall back to their local fields) if the asset is missing.
        /// </summary>
        public static OutlineFeedbackConfig Default
        {
            get
            {
                if (!_defaultLoaded)
                {
                    _default = Resources.Load<OutlineFeedbackConfig>(DefaultResourcePath);
                    _defaultLoaded = true;
                }
                return _default;
            }
        }

        private void OnValidate()
        {
            if (hintMaxWidth < hintMinWidth) hintMaxWidth = hintMinWidth;
        }
    }
}
