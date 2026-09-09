using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Shababeek.Interactions.Highlight;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Shared configuration hub for <see cref="InteractableOutlineFeedback"/>.
    /// One asset holds the colors, widths, modes, and enable toggles for every outline
    /// state (hover, selected, grab hint) so the whole project shares one source of truth.
    /// Assign the same asset to every feedback component (or set a project-wide default).
    ///
    /// The asset in <c>Resources/OutlineFeedbackConfig</c> is the root config (PC / Editor).
    /// It can list <see cref="PipelineOverride"/> rules that swap in a different config asset
    /// while a given Render Pipeline Asset is active (e.g. a leaner Quest config), so the right
    /// settings are picked automatically per platform and per quality level with no wiring.
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
            public InteractionOutline.Mode mode = InteractionOutline.Mode.OutlineVisible;
        }

        /// <summary>
        /// "While this render pipeline is active, use that config instead" rule.
        /// Matching is by name substring (portable between projects) or by direct
        /// Render Pipeline Asset reference (exact, but hard-links the asset).
        /// </summary>
        [Serializable]
        public class PipelineOverride
        {
            [Tooltip("Label for readability in the inspector. Not used at runtime.")]
            public string label = "Quest";

            [Tooltip("Matches when the active Render Pipeline Asset name contains any of these (case-insensitive). " +
                     "Keeps this config portable - no direct reference to a project's URP assets.")]
            public string[] pipelineNameContains = { "Quest" };

            [Tooltip("Optional exact matches against specific Render Pipeline Assets. " +
                     "Leave empty when matching by name.")]
            public RenderPipelineAsset[] pipelineAssets;

            [Tooltip("Config used while one of the pipelines above is active.")]
            public OutlineFeedbackConfig config;

            /// <summary>True when <paramref name="active"/> is one of the pipelines this rule targets.</summary>
            public bool Matches(RenderPipelineAsset active)
            {
                if (active == null) return false;

                if (pipelineAssets != null)
                {
                    foreach (var asset in pipelineAssets)
                        if (asset != null && asset == active) return true;
                }

                if (pipelineNameContains != null)
                {
                    var pipelineName = active.name;
                    foreach (var token in pipelineNameContains)
                        if (!string.IsNullOrEmpty(token) &&
                            pipelineName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }

                return false;
            }
        }

        [Tooltip("Global kill switch. When off no outline feedback is shown by any component using this config.")]
        [SerializeField] private bool feedbackEnabled = true;

        [Header("Hover")]
        [SerializeField] private OutlineState hover = new()
        {
            color = new Color(1f, 0.85f, 0.2f, 0.2f),
            width = 0.1f,
            mode = InteractionOutline.Mode.OutlineVisible
        };

        [Header("Selected")]
        [Tooltip("Keep outline visible while held.")]
        [SerializeField] private OutlineState selected = new()
        {
            color = new Color(0.2f, 0.9f, 1f, 0.2f),
            width = 0.1f,
            mode = InteractionOutline.Mode.OutlineVisible
        };

        [Header("Grab Hint (Idle)")]
        [Tooltip("Pulsing outline shown when idle (not hovered/selected) to hint the object is grabbable. 'enabled' here acts as the global default for showing the hint.")]
        [SerializeField] private OutlineState hint = new()
        {
            enabled = false,
            color = new Color(1f, 1f, 1f, 0.1f),
            width = 0.02f,
            mode = InteractionOutline.Mode.OutlineVisible
        };
        [Tooltip("Minimum outline width during the pulse cycle.")]
        [SerializeField, Range(0f, 5f)] private float hintMinWidth = 0.02f;
        [Tooltip("Maximum outline width during the pulse cycle.")]
        [SerializeField, Range(0f, 5f)] private float hintMaxWidth = 0.08f;
        [Tooltip("Pulses per second.")]
        [SerializeField, Range(0.1f, 5f)] private float hintPulseSpeed = 1f;

        [Header("Per-Render-Pipeline Overrides")]
        [Tooltip("Only read on the root asset (Resources/OutlineFeedbackConfig). First matching rule wins; " +
                 "when nothing matches, this asset's own settings are used. Re-evaluated automatically when the " +
                 "active Render Pipeline Asset changes (build target, quality level, runtime quality switch).")]
        [SerializeField] private List<PipelineOverride> pipelineOverrides = new();

        /// <summary>Global kill switch for all outline feedback driven by this config.</summary>
        public bool FeedbackEnabled => feedbackEnabled;

        public OutlineState Hover => hover;
        public OutlineState Selected => selected;
        public OutlineState Hint => hint;

        public float HintMinWidth => hintMinWidth;
        public float HintMaxWidth => hintMaxWidth;
        public float HintPulseSpeed => hintPulseSpeed;

        /// <summary>Resources path (without extension) of the project-wide root config asset.</summary>
        public const string DefaultResourcePath = "OutlineFeedbackConfig";

        private static OutlineFeedbackConfig _root;
        private static bool _rootLoaded;
        private static OutlineFeedbackConfig _active;
        private static bool _activeResolved;
        private static bool _hooked;

        /// <summary>
        /// Raised when <see cref="Active"/> resolves to a different config, i.e. the active
        /// Render Pipeline Asset changed. Components re-apply their outline state on this.
        /// </summary>
        public static event Action ActiveChanged;

        /// <summary>
        /// Root config asset, lazily loaded from <c>Resources/OutlineFeedbackConfig</c>.
        /// This is the PC / Editor config and the owner of the per-pipeline override table.
        /// Returns null (and components fall back to their local fields) if the asset is missing.
        /// </summary>
        public static OutlineFeedbackConfig Root
        {
            get
            {
                if (!_rootLoaded)
                {
                    _root = Resources.Load<OutlineFeedbackConfig>(DefaultResourcePath);
                    _rootLoaded = true;
                }
                return _root;
            }
        }

        /// <summary>
        /// Config that applies right now: the override matching the active Render Pipeline Asset,
        /// or <see cref="Root"/> when no rule matches. Cached; refreshed automatically on pipeline swap.
        /// </summary>
        public static OutlineFeedbackConfig Active
        {
            get
            {
                if (!_activeResolved)
                {
                    Hook();
                    _active = Resolve(GraphicsSettings.currentRenderPipeline);
                    _activeResolved = true;
                }
                return _active;
            }
        }

        /// <summary>Alias for <see cref="Active"/>, kept for existing call sites.</summary>
        public static OutlineFeedbackConfig Default => Active;

        /// <summary>Drop the cached root/active configs so the next read re-resolves from disk.</summary>
        public static void Invalidate()
        {
            var previous = _active;
            _root = null;
            _rootLoaded = false;
            _active = null;
            _activeResolved = false;
            if (Active != previous) ActiveChanged?.Invoke();
        }

        private static OutlineFeedbackConfig Resolve(RenderPipelineAsset pipeline)
        {
            var root = Root;
            if (root == null || root.pipelineOverrides == null) return root;

            foreach (var rule in root.pipelineOverrides)
            {
                // Skip empty rules and self-references - an override pointing back at the
                // root would make the resolved config depend on itself.
                if (rule?.config == null || rule.config == root) continue;
                if (rule.Matches(pipeline)) return rule.config;
            }

            return root;
        }

        private static void Hook()
        {
            if (_hooked) return;
            RenderPipelineManager.activeRenderPipelineAssetChanged += OnPipelineAssetChanged;
            _hooked = true;
        }

        private static void OnPipelineAssetChanged(RenderPipelineAsset previous, RenderPipelineAsset current)
        {
            var resolved = Resolve(current);
            _activeResolved = true;
            if (resolved == _active) return;
            _active = resolved;
            ActiveChanged?.Invoke();
        }

        // Statics survive "Enter Play Mode without domain reload", so clear them explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            if (_hooked)
            {
                RenderPipelineManager.activeRenderPipelineAssetChanged -= OnPipelineAssetChanged;
                _hooked = false;
            }
            _root = null;
            _rootLoaded = false;
            _active = null;
            _activeResolved = false;
            ActiveChanged = null;
        }

        private void OnValidate()
        {
            if (hintMaxWidth < hintMinWidth) hintMaxWidth = hintMinWidth;
#if UNITY_EDITOR
            // Edited in the inspector: drop the caches so the next read picks the change up.
            // No event is raised here - Resources.Load during OnValidate can run mid-import.
            _root = null;
            _rootLoaded = false;
            _active = null;
            _activeResolved = false;
#endif
        }
    }
}
