using UnityEngine;
using UniRx;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Highlight
{
    /// <summary>
    /// Drives a QuickOutline component from an InteractableBase's hover/select events.
    /// Enables the outline on hover, swaps color/width when selected, disables on release.
    /// Optionally shows a pulsing idle "grab hint" outline to advertise the object as grabbable.
    /// All colors/widths/modes/toggles come from the shared <see cref="OutlineFeedbackConfig"/>
    /// (Resources/OutlineFeedbackConfig) so the whole project shares one source of truth.
    /// The exact config is picked from the active Render Pipeline Asset, so PC/Editor and Quest
    /// get their own settings automatically; a pipeline swap re-applies the state live.
    /// </summary>
    [RequireComponent(typeof(InteractableBase))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Shababeek/Interactions/Interactable Outline Feedback")]
    public class InteractableOutlineFeedback : MonoBehaviour
    {
        [Tooltip("Outline to drive. Auto-found on this GameObject (or children) if not assigned. Created on this GameObject when missing.")]
        [SerializeField] private InteractionOutline outline;

        // Shared project-wide settings, resolved statically from Resources/OutlineFeedbackConfig
        // and narrowed to the config that matches the active render pipeline (PC/Editor, Quest, ...).
        private static OutlineFeedbackConfig Config => OutlineFeedbackConfig.Active;

        private InteractableBase _interactable;
        private readonly CompositeDisposable _disposables = new();
        private bool _isHovering;
        private bool _isSelected;
        // Per-instance runtime toggle for the idle grab hint. Style comes from the config;
        // this decides whether THIS object currently advertises itself (e.g. driven by sockets).
        private bool _showGrabHint;
        // True once SetGrabHint has been called on this instance. Until then the hint follows
        // whatever the active config defaults to, so a pipeline swap can turn it off/on.
        private bool _grabHintOverridden;

        private void Awake()
        {
            _interactable = GetComponent<InteractableBase>();
            var config = Config;
            _showGrabHint = config != null && config.Hint.enabled;
            EnsureOutline();
            ApplyState();
        }

        private void OnEnable()
        {
            _disposables.Clear();
            OutlineFeedbackConfig.ActiveChanged += OnActiveConfigChanged;
            if (_interactable == null) return;

            _interactable.OnHoverStarted
                .Subscribe(_ => { _isHovering = true;  ApplyState(); })
                .AddTo(_disposables);

            _interactable.OnHoverEnded
                .Subscribe(_ => { _isHovering = false; ApplyState(); })
                .AddTo(_disposables);

            _interactable.OnSelected
                .Subscribe(_ => { _isSelected = true;  ApplyState(); })
                .AddTo(_disposables);

            _interactable.OnDeselected
                .Subscribe(_ => { _isSelected = false; _isHovering = false; ApplyState(); })
                .AddTo(_disposables);

            ApplyState();
        }

        private void OnDisable()
        {
            OutlineFeedbackConfig.ActiveChanged -= OnActiveConfigChanged;
            _disposables.Clear();
            _isHovering = false;
            _isSelected = false;
            HideOutline();
        }

        private void Update()
        {
            var config = Config;
            if (config == null || !config.FeedbackEnabled || !_showGrabHint) return;
            if (_isHovering || _isSelected) return;
            if (outline == null) return;

            float t = (Mathf.Sin(Time.time * config.HintPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            Set(config.Hint.mode, config.Hint.color, Mathf.Lerp(config.HintMinWidth, config.HintMaxWidth, t));
        }

        private void EnsureOutline()
        {
            if (outline == null) outline = GetComponent<InteractionOutline>();
            if (outline == null) outline = GetComponentInChildren<InteractionOutline>(true);
            if (outline == null) outline = gameObject.AddComponent<InteractionOutline>();
            // Component stays enabled; visibility is driven through SetOutlineActive,
            // which attaches/detaches the fill material. A hidden outline therefore
            // costs no draw calls (the old width-0/alpha-0 trick still rendered a
            // transparent pass for every interactable, every frame).
            outline.enabled = true;
            HideOutline();
        }

        private void ApplyState()
        {
            if (outline == null) return;

            var config = Config;
            if (config == null || !config.FeedbackEnabled)
            {
                HideOutline();
                return;
            }

            if (_isSelected && config.Selected.enabled)
            {
                Set(config.Selected.mode, config.Selected.color, config.Selected.width);
                return;
            }

            if (_isHovering && !_isSelected && config.Hover.enabled)
            {
                Set(config.Hover.mode, config.Hover.color, config.Hover.width);
                return;
            }

            if (_showGrabHint)
            {
                Set(config.Hint.mode, config.Hint.color, config.HintMinWidth);
                return;
            }

            HideOutline();
        }

        /// <summary>Toggle this object's idle grab-hint outline at runtime (style comes from the config).</summary>
        public void SetGrabHint(bool enabled)
        {
            _showGrabHint = enabled;
            _grabHintOverridden = true;
            ApplyState();
        }

        /// <summary>
        /// The active config changed (render pipeline swap). Re-take the hint default unless this
        /// instance drives it explicitly, then re-apply so the new colors/widths show immediately.
        /// </summary>
        private void OnActiveConfigChanged()
        {
            if (!_grabHintOverridden)
            {
                var config = Config;
                _showGrabHint = config != null && config.Hint.enabled;
            }
            ApplyState();
        }

        private void HideOutline()
        {
            if (outline == null) return;
            outline.SetOutlineActive(false);
        }

        private void Set(InteractionOutline.Mode mode, Color color, float width)
        {
            if (outline == null) return;
            outline.SetOutlineActive(true);
            outline.OutlineMode = mode;
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
        }

        private void OnValidate()
        {
            if (outline == null) outline = GetComponent<InteractionOutline>();
        }
    }
}
