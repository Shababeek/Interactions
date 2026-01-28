using System;
using System.Collections.Generic;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// Unified feedback system that manages multiple feedback types for interactions
    /// </summary>
    [RequireComponent(typeof(InteractableBase))]
    public class FeedbackSystem : MonoBehaviour
    {
        [Header("Feedback Configuration")]
        [Tooltip("List of feedback components that respond to interaction events. Add feedbacks via code/editor only.")]
        [SerializeReference]
        private List<FeedbackData> feedbacks = new List<FeedbackData>();

        private InteractableBase _interactable;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Awake()
        {
            _interactable = GetComponent<InteractableBase>();
            InitializeFeedbacks();
            SubscribeToEvents();
        }

        private void InitializeFeedbacks()
        {
            foreach (var feedback in feedbacks)
            {
                if (feedback != null && feedback.IsValid())
                {
                    feedback.Initialize(this);
                }
            }
        }

        private void SubscribeToEvents()
        {
            _interactable.OnHoverStarted
                .Do(OnHoverStarted)
                .Subscribe().AddTo(_disposables);

            _interactable.OnHoverEnded
                .Do(OnHoverEnded)
                .Subscribe().AddTo(_disposables);

            // Selection events
            _interactable.OnSelected
                .Do(OnSelected)
                .Subscribe().AddTo(_disposables);

            _interactable.OnDeselected
                .Do(OnDeselected)
                .Subscribe().AddTo(_disposables);

            // Activation events
            _interactable.OnUseStarted
                .Do(OnActivated)
                .Subscribe().AddTo(_disposables);
        }

        private void OnHoverStarted(InteractorBase interactor)
        {
            foreach (var feedback in feedbacks)
            {
                if (feedback != null && feedback.IsValid())
                {
                    feedback.OnHoverStarted(interactor);
                }
            }
        }

        private void OnHoverEnded(InteractorBase interactor)
        {
            foreach (var feedback in feedbacks)
            {
                if (feedback != null && feedback.IsValid())
                {
                    feedback.OnHoverEnded(interactor);
                }
            }
        }

        private void OnSelected(InteractorBase interactor)
        {
            foreach (var feedback in feedbacks)
            {
                if (feedback != null && feedback.IsValid())
                {
                    feedback.OnSelected(interactor);
                }
            }
        }

        private void OnDeselected(InteractorBase interactor)
        {
            foreach (var feedback in feedbacks)
            {
                if (feedback != null && feedback.IsValid())
                {
                    feedback.OnDeselected(interactor);
                }
            }
        }

        private void OnActivated(InteractorBase interactor)
        {
            foreach (var feedback in feedbacks)
            {
                if (feedback != null && feedback.IsValid())
                {
                    feedback.OnActivated(interactor);
                }
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        /// <summary>
        /// Adds a new feedback component to the feedback system.
        /// The feedback will be initialized and start responding to interaction events.
        /// </summary>
        /// <param name="feedback">The feedback component to add to the system.</param>
        /// <remarks>
        /// The feedback will be automatically initialized if it's valid.
        /// Duplicate feedbacks will be ignored.
        /// </remarks>
        public void AddFeedback(FeedbackData feedback)
        {
            if (feedback != null && !feedbacks.Contains(feedback))
            {
                feedbacks.Add(feedback);
                if (feedback.IsValid())
                {
                    feedback.Initialize(this);
                }
            }
        }

        /// <summary>
        /// Adds a new material feedback component to the feedback system.
        /// Creates a default MaterialFeedback instance and adds it to the system.
        /// </summary>
        /// <remarks>
        /// This method creates a new MaterialFeedback with default settings.
        /// The feedback will be automatically initialized if it's valid.
        /// </remarks>
        public void AddFeedback()
        {
            var newFeedback = new MaterialFeedback();
            feedbacks.Add(newFeedback);
            if (newFeedback.IsValid())
            {
                newFeedback.Initialize(this);
            }
        }

        /// <summary>
        /// Removes a feedback component from the feedback system.
        /// </summary>
        /// <param name="feedback">The feedback component to remove from the system.</param>
        /// <remarks>
        /// If the feedback is not found in the system, no action is taken.
        /// </remarks>
        public void RemoveFeedback(FeedbackData feedback)
        {
            if (feedbacks.Contains(feedback))
            {
                feedbacks.Remove(feedback);
            }
        }

        /// <summary>
        /// Removes all feedback components from the feedback system.
        /// </summary>
        /// <remarks>
        /// This will clear the entire feedback list, removing all active feedbacks.
        /// </remarks>
        public void ClearFeedbacks()
        {
            feedbacks.Clear();
        }

        /// <summary>
        /// Gets all feedback components currently in the feedback system.
        /// </summary>
        /// <returns>A list containing all active feedback components.</returns>
        /// <remarks>
        /// This returns a reference to the internal feedback list.
        /// Modifying the returned list will affect the feedback system.
        /// </remarks>
        public List<FeedbackData> GetFeedbacks()
        {
            return feedbacks;
        }
    }

    /// <summary>
    /// Base class for all feedback types.
    /// </summary>
    [Serializable]
    public abstract class FeedbackData
    {
        [Tooltip("Display name for this feedback component.")]
        [SerializeField] protected string feedbackName = "Feedback";
        
        [Tooltip("Whether this feedback is active.")]
        [SerializeField] protected bool enabled = true;
        
        protected FeedbackSystem feedbackSystem;

        /// <summary>
        /// Gets or sets the display name for this feedback.
        /// </summary>
        public string FeedbackName
        {
            get => feedbackName;
            set => feedbackName = value;
        }

        /// <summary>
        /// Gets or sets whether this feedback is enabled.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        /// <summary>
        /// Initializes this feedback with the parent feedback system.
        /// </summary>
        public virtual void Initialize(FeedbackSystem system)
        {
            feedbackSystem = system;
        }

        /// <summary>
        /// Validates that this feedback is properly configured.
        /// </summary>
        public virtual bool IsValid() => enabled;

        /// <summary>
        /// Called when an interactor starts hovering.
        /// </summary>
        public virtual void OnHoverStarted(InteractorBase interactor)
        {
        }

        /// <summary>
        /// Called when an interactor stops hovering.
        /// </summary>
        public virtual void OnHoverEnded(InteractorBase interactor)
        {
        }

        /// <summary>
        /// Called when the interactable is selected.
        /// </summary>
        public virtual void OnSelected(InteractorBase interactor)
        {
        }

        /// <summary>
        /// Called when the interactable is deselected.
        /// </summary>
        public virtual void OnDeselected(InteractorBase interactor)
        {
        }

        /// <summary>
        /// Called when the interactable is activated (use button pressed).
        /// </summary>
        public virtual void OnActivated(InteractorBase interactor)
        {
        }
    }

    /// <summary>
    /// Material feedback for changing colors/properties.
    /// </summary>
    [Serializable]
    public class MaterialFeedback : FeedbackData
    {
        [Header("Material Settings")]
        [Tooltip("Renderers whose materials will be modified.")]
        [SerializeField] public Renderer[] renderers;

        [Tooltip("Name of the shader property to modify (e.g., '_Color', '_BaseColor').")]
        [SerializeField] public string colorPropertyName = "_Color";
        
        [Tooltip("Color to apply on hover.")]
        [SerializeField] public Color hoverColor = Color.yellow;
        
        [Tooltip("Color to apply on selection.")]
        [SerializeField] public Color selectColor = Color.green;
        
        [Tooltip("Color to apply on activation.")]
        [SerializeField] public Color activateColor = Color.red;
        
        [Tooltip("Multiplier for the hover color effect.")]
        [SerializeField] public float colorMultiplier = 0.3f;

        private Color[] _originalColors;
        private bool _isInitialized = false;

        public MaterialFeedback()
        {
            feedbackName = "Material Feedback";
        }

        public override void Initialize(FeedbackSystem system)
        {
            base.Initialize(system);

            if (renderers == null || renderers.Length == 0)
            {
                renderers = feedbackSystem.GetComponentsInChildren<Renderer>();
            }

            if (renderers != null && renderers.Length > 0)
            {
                _originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].material != null)
                    {
                        _originalColors[i] = renderers[i].material.GetColor(colorPropertyName);
                    }
                }

                _isInitialized = true;
            }
        }

        public override bool IsValid()
        {
            return base.IsValid() && renderers != null && renderers.Length > 0 && _isInitialized;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] && renderers[i].material)
                {
                    renderers[i].material.SetColor(colorPropertyName, _originalColors[i] * colorMultiplier);
                }
            }
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] && renderers[i].material)
                {
                    renderers[i].material.SetColor(colorPropertyName, _originalColors[i]);
                }
            }
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;

            foreach (var renderer in renderers)
            {
                if (renderer && renderer.material)
                {
                    renderer.material.SetColor(colorPropertyName, selectColor);
                }
            }
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    renderers[i].material.SetColor(colorPropertyName, _originalColors[i]);
                }
            }
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;

            foreach (var renderer in renderers)
            {
                if (renderer && renderer.material)
                {
                    renderer.material.SetColor(colorPropertyName, activateColor);
                }
            }
        }
    }

    /// <summary>
    /// Animation feedback for triggering animator parameters.
    /// </summary>
    [Serializable]
    public class AnimationFeedback : FeedbackData
    {
        [Tooltip("The animator component to control.")]
        [SerializeField] public Animator animator;
        
        [Header("Animation Parameter Names")] 
        [Tooltip("Bool parameter name for hover state.")]
        [SerializeField] public string hoverBoolName = "Hovered";

        [Tooltip("Trigger parameter name for selection.")]
        [SerializeField] public string selectTriggerName = "Selected";
        
        [Tooltip("Trigger parameter name for deselection.")]
        [SerializeField] public string deselectTriggerName = "Deselected";
        
        [Tooltip("Trigger parameter name for activation.")]
        [SerializeField] public string activatedTriggerName = "Activated";

        public AnimationFeedback()
        {
            feedbackName = "Animation Feedback";
        }

        public override void Initialize(FeedbackSystem system)
        {
            base.Initialize(system);
            if (animator == null)
            {
                animator = feedbackSystem.GetComponent<Animator>();
            }
        }

        public override bool IsValid()
        {
            return base.IsValid() && animator != null;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            animator.SetBool(hoverBoolName, true);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            animator.SetBool(hoverBoolName, false);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            animator.SetTrigger(selectTriggerName);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            animator.SetTrigger(deselectTriggerName);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            animator.SetTrigger(activatedTriggerName);
        }
    }

    /// <summary>
    /// Haptic feedback for controller vibration.
    /// </summary>
    [Serializable]
    public class HapticFeedback : FeedbackData
    {
        [Header("Haptic Settings")]
        [Tooltip("Vibration amplitude for hover (0-1).")]
        [SerializeField] public float hoverAmplitude = 0.3f;

        [Tooltip("Vibration duration for hover in seconds.")]
        [SerializeField] public float hoverDuration = 0.1f;
        
        [Tooltip("Vibration amplitude for selection (0-1).")]
        [SerializeField] public float selectAmplitude = 0.5f;
        
        [Tooltip("Vibration duration for selection in seconds.")]
        [SerializeField] public float selectDuration = 0.2f;
        
        [Tooltip("Vibration amplitude for activation (0-1).")]
        [SerializeField] public float activateAmplitude = 1f;
        
        [Tooltip("Vibration duration for activation in seconds.")]
        [SerializeField] public float activateDuration = 0.3f;

        public HapticFeedback()
        {
            feedbackName = "Haptic Feedback";
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!enabled) return;
            ExecuteHaptic(interactor.HandIdentifier, hoverAmplitude, hoverDuration);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!enabled) return;
            ExecuteHaptic(interactor.HandIdentifier, selectAmplitude, selectDuration);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!enabled) return;
            ExecuteHaptic(interactor.HandIdentifier, activateAmplitude, activateDuration);
        }

        private void ExecuteHaptic(HandIdentifier handIdentifier, float amplitude, float duration)
        {
            var hand = handIdentifier == HandIdentifier.Left
                ? UnityEngine.XR.XRNode.LeftHand
                : UnityEngine.XR.XRNode.RightHand;
            var inputDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(hand);
            inputDevice.SendHapticImpulse(0, amplitude, duration);
        }
    }

    /// <summary>
    /// Audio feedback for sound effects.
    /// </summary>
    [Serializable]
    public class AudioFeedback : FeedbackData
    {
        [Header("Audio Settings")]
        [Tooltip("Audio source component for playing sounds.")]
        [SerializeField] public AudioSource audioSource;

        [Tooltip("Audio clip to play on hover start.")]
        [SerializeField] public AudioClip hoverClip;
        
        [Tooltip("Audio clip to play on hover end.")]
        [SerializeField] public AudioClip hoverExitClip;
        
        [Tooltip("Audio clip to play on selection.")]
        [SerializeField] public AudioClip selectClip;
        
        [Tooltip("Audio clip to play on deselection.")]
        [SerializeField] public AudioClip deselectClip;
        
        [Tooltip("Audio clip to play on activation.")]
        [SerializeField] public AudioClip activateClip;
        
        [Tooltip("Volume for hover sounds (0-1).")]
        [SerializeField] public float hoverVolume = 0.5f;
        
        [Tooltip("Volume for selection sounds (0-1).")]
        [SerializeField] public float selectVolume = 0.7f;
        
        [Tooltip("Volume for activation sounds (0-1).")]
        [SerializeField] public float activateVolume = 1f;
        
        [Tooltip("Whether to use 3D spatial audio.")]
        [SerializeField] public bool useSpatialAudio = true;
        
        [Tooltip("Whether to randomize pitch for variety.")]
        [SerializeField] public bool randomizePitch = false;
        
        [Tooltip("Amount of pitch randomization (±this value).")]
        [SerializeField] public float pitchRandomization = 0.1f;

        public AudioFeedback()
        {
            feedbackName = "Audio Feedback";
        }

        public override void Initialize(FeedbackSystem system)
        {
            base.Initialize(system);

            if (audioSource == null)
            {
                audioSource = feedbackSystem.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = feedbackSystem.gameObject.AddComponent<AudioSource>();
                }
            }

            if (audioSource != null)
            {
                audioSource.spatialBlend = useSpatialAudio ? 1f : 0f;
                audioSource.playOnAwake = false;
            }
        }
        public override bool IsValid()
        {
            return base.IsValid() && audioSource != null;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid() || !enabled || hoverClip == null) return;
            PlaySound(hoverClip, hoverVolume);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!IsValid() || !enabled || hoverExitClip == null) return;
            PlaySound(hoverExitClip, hoverVolume);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled || selectClip == null) return;
            PlaySound(selectClip, selectVolume);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled || deselectClip == null) return;
            PlaySound(deselectClip, selectVolume);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid() || !enabled || activateClip == null) return;
            PlaySound(activateClip, activateVolume);
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (clip == null || audioSource == null) return;

            audioSource.clip = clip;
            audioSource.volume = volume;

            if (randomizePitch)
            {
                float pitchVariationAmount = UnityEngine.Random.Range(-pitchRandomization, pitchRandomization);
                audioSource.pitch = 1f + pitchVariationAmount;
            }
            else
            {
                audioSource.pitch = 1f;
            }

            audioSource.Play();
        }
    }

    /// <summary>
    /// Object toggle feedback for enabling/disabling GameObjects based on interaction state.
    /// </summary>
    [Serializable]
    public class ObjectToggleFeedback : FeedbackData
    {
        [Header("Hover Objects")]
        [Tooltip("Objects to enable when hovered (disabled when not hovered).")]
        [SerializeField] public GameObject[] enableOnHover;

        [Tooltip("Objects to disable when hovered (enabled when not hovered).")]
        [SerializeField] public GameObject[] disableOnHover;

        [Header("Selection Objects")]
        [Tooltip("Objects to enable when selected (disabled when deselected).")]
        [SerializeField] public GameObject[] enableOnSelect;

        [Tooltip("Objects to disable when selected (enabled when deselected).")]
        [SerializeField] public GameObject[] disableOnSelect;

        [Header("Activation Objects")]
        [Tooltip("Objects to enable when activated.")]
        [SerializeField] public GameObject[] enableOnActivate;

        [Tooltip("Objects to disable when activated.")]
        [SerializeField] public GameObject[] disableOnActivate;

        [Header("Options")]
        [Tooltip("Reset objects to original state on deselect/hover end.")]
        [SerializeField] public bool resetOnEnd = true;

        public ObjectToggleFeedback()
        {
            feedbackName = "Object Toggle Feedback";
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!enabled) return;
            SetObjectsActive(enableOnHover, true);
            SetObjectsActive(disableOnHover, false);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!enabled || !resetOnEnd) return;
            SetObjectsActive(enableOnHover, false);
            SetObjectsActive(disableOnHover, true);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!enabled) return;
            SetObjectsActive(enableOnSelect, true);
            SetObjectsActive(disableOnSelect, false);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!enabled || !resetOnEnd) return;
            SetObjectsActive(enableOnSelect, false);
            SetObjectsActive(disableOnSelect, true);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!enabled) return;
            SetObjectsActive(enableOnActivate, true);
            SetObjectsActive(disableOnActivate, false);
        }

        private void SetObjectsActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;
            foreach (var obj in objects)
            {
                if (obj != null) obj.SetActive(active);
            }
        }
    }

    /// <summary>
    /// Scale feedback for animating object scale based on interaction state.
    /// </summary>
    [Serializable]
    public class ScaleFeedback : FeedbackData
    {
        [Header("Target")]
        [Tooltip("Transform to scale. Uses feedback system's transform if not set.")]
        [SerializeField] public Transform target;

        [Header("Scale Settings")]
        [Tooltip("Scale multiplier when hovered.")]
        [SerializeField] public float hoverScale = 1.1f;

        [Tooltip("Scale multiplier when selected.")]
        [SerializeField] public float selectScale = 1.2f;

        [Tooltip("Scale multiplier when activated.")]
        [SerializeField] public float activateScale = 0.9f;

        [Header("Animation")]
        [Tooltip("Whether to animate scale changes.")]
        [SerializeField] public bool animate = true;

        [Tooltip("Animation speed.")]
        [SerializeField] public float animationSpeed = 10f;

        private Vector3 _originalScale;
        private Vector3 _targetScale;
        private bool _isAnimating;

        public ScaleFeedback()
        {
            feedbackName = "Scale Feedback";
        }

        public override void Initialize(FeedbackSystem system)
        {
            base.Initialize(system);
            if (target == null) target = feedbackSystem.transform;
            _originalScale = target.localScale;
            _targetScale = _originalScale;
        }

        public override bool IsValid()
        {
            return base.IsValid() && target != null;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            SetTargetScale(hoverScale);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            SetTargetScale(1f);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            SetTargetScale(selectScale);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            SetTargetScale(1f);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid() || !enabled) return;
            SetTargetScale(activateScale);
        }

        private void SetTargetScale(float multiplier)
        {
            _targetScale = _originalScale * multiplier;
            if (!animate)
            {
                target.localScale = _targetScale;
            }
            else if (!_isAnimating)
            {
                _isAnimating = true;
                feedbackSystem.StartCoroutine(AnimateScale());
            }
        }

        private System.Collections.IEnumerator AnimateScale()
        {
            while (Vector3.Distance(target.localScale, _targetScale) > 0.001f)
            {
                target.localScale = Vector3.Lerp(target.localScale, _targetScale, animationSpeed * UnityEngine.Time.deltaTime);
                yield return null;
            }
            target.localScale = _targetScale;
            _isAnimating = false;
        }
    }

    /// <summary>
    /// Particle feedback for playing particle effects on interaction events.
    /// </summary>
    [Serializable]
    public class ParticleFeedback : FeedbackData
    {
        [Header("Particle Systems")]
        [Tooltip("Particle system to play on hover start.")]
        [SerializeField] public ParticleSystem hoverParticles;

        [Tooltip("Particle system to play on selection.")]
        [SerializeField] public ParticleSystem selectParticles;

        [Tooltip("Particle system to play on deselection.")]
        [SerializeField] public ParticleSystem deselectParticles;

        [Tooltip("Particle system to play on activation.")]
        [SerializeField] public ParticleSystem activateParticles;

        [Header("Options")]
        [Tooltip("Stop hover particles when hover ends.")]
        [SerializeField] public bool stopHoverOnEnd = true;

        public ParticleFeedback()
        {
            feedbackName = "Particle Feedback";
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!enabled) return;
            PlayParticles(hoverParticles);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!enabled) return;
            if (stopHoverOnEnd && hoverParticles != null)
                hoverParticles.Stop();
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!enabled) return;
            PlayParticles(selectParticles);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!enabled) return;
            PlayParticles(deselectParticles);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!enabled) return;
            PlayParticles(activateParticles);
        }

        private void PlayParticles(ParticleSystem particles)
        {
            if (particles != null) particles.Play();
        }
    }

    /// <summary>
    /// UnityEvent feedback for triggering custom events on interaction states.
    /// </summary>
    [Serializable]
    public class UnityEventFeedback : FeedbackData
    {
        [Header("Events")]
        [Tooltip("Event fired when hover starts.")]
        [SerializeField] public UnityEngine.Events.UnityEvent onHoverStart;

        [Tooltip("Event fired when hover ends.")]
        [SerializeField] public UnityEngine.Events.UnityEvent onHoverEnd;

        [Tooltip("Event fired when selected.")]
        [SerializeField] public UnityEngine.Events.UnityEvent onSelect;

        [Tooltip("Event fired when deselected.")]
        [SerializeField] public UnityEngine.Events.UnityEvent onDeselect;

        [Tooltip("Event fired when activated.")]
        [SerializeField] public UnityEngine.Events.UnityEvent onActivate;

        public UnityEventFeedback()
        {
            feedbackName = "UnityEvent Feedback";
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!enabled) return;
            onHoverStart?.Invoke();
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!enabled) return;
            onHoverEnd?.Invoke();
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!enabled) return;
            onSelect?.Invoke();
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!enabled) return;
            onDeselect?.Invoke();
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!enabled) return;
            onActivate?.Invoke();
        }
    }
}