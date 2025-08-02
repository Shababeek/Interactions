using System;
using System.Collections.Generic;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
        // IMPORTANT: Only add feedbacks via code/editor, not via the inspector + button, to ensure correct managed reference type.
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
            // Hover events
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
            _interactable.OnActivated
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
    /// Base class for all feedback types
    /// </summary>
    [Serializable]
    public class FeedbackData
    {
        [SerializeField] protected string feedbackName = "Feedback";
        [SerializeField] private bool enabled = true;
        protected FeedbackSystem feedbackSystem;

        public virtual void Initialize(FeedbackSystem system)
        {
            feedbackSystem = system;
        }

        public virtual bool IsValid() => true;

        public virtual void OnHoverStarted(InteractorBase interactor)
        {
        }

        public virtual void OnHoverEnded(InteractorBase interactor)
        {
        }

        public virtual void OnSelected(InteractorBase interactor)
        {
        }

        public virtual void OnDeselected(InteractorBase interactor)
        {
        }

        public virtual void OnActivated(InteractorBase interactor)
        {
        }

        public virtual VisualElement CreateVisualElement(Action onValueChanged)
        {
            var root = new VisualElement();
            root.Add(new Label("Nothing Done"));
            return root;
        }
    }

    /// <summary>
    /// Material feedback for changing colors/properties
    /// </summary>
    [Serializable]
    public class MaterialFeedback : FeedbackData
    {
        [Header("Material Settings")] [SerializeField]
        public Renderer[] renderers;

        [SerializeField] public string colorPropertyName = "_Color";
        [SerializeField] public Color hoverColor = Color.yellow;
        [SerializeField] public Color selectColor = Color.green;
        [SerializeField] public Color activateColor = Color.red;
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
            return renderers != null && renderers.Length > 0 && _isInitialized;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid()) return;

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
            if (!IsValid()) return;

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
            if (!IsValid()) return;

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
            if (!IsValid()) return;

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
            if (!IsValid()) return;

            foreach (var renderer in renderers)
            {
                if (renderer && renderer.material)
                {
                    renderer.material.SetColor(colorPropertyName, activateColor);
                }
            }
        }

        public override VisualElement CreateVisualElement(Action onValueChanged)
        {
            var container = new VisualElement();
            var nameField = new TextField("Name") { value = feedbackName };
            nameField.RegisterValueChangedCallback(evt => { feedbackName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(nameField);
            var colorPropertyField = new TextField("Color Property") { value = colorPropertyName };
            colorPropertyField.RegisterValueChangedCallback(evt => { colorPropertyName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(colorPropertyField);
            var hoverColorField = new ColorField("Hover Color") { value = hoverColor };
            hoverColorField.RegisterValueChangedCallback(evt => { hoverColor = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(hoverColorField);
            var selectColorField = new ColorField("Select Color") { value = selectColor };
            selectColorField.RegisterValueChangedCallback(evt => { selectColor = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(selectColorField);
            var activateColorField = new ColorField("Activate Color") { value = activateColor };
            activateColorField.RegisterValueChangedCallback(evt => { activateColor = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(activateColorField);
            var multiplierField = new FloatField("Color Multiplier") { value = colorMultiplier };
            multiplierField.RegisterValueChangedCallback(evt => { colorMultiplier = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(multiplierField);
            return container;
        }
    }

    /// <summary>
    /// Animation feedback for triggering animator parameters
    /// </summary>
    [Serializable]
    public class AnimationFeedback : FeedbackData
    {
        [SerializeField] public Animator animator;
        [Header("Animation ParameterNames")] 
        [SerializeField] public string hoverBoolName = "Hovered";

        [SerializeField] public string selectTriggerName = "Selected";
        [SerializeField] public string deselectTriggerName = "Deselected";
        [SerializeField] public string activatedTriggerName = "Activated";


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
            return animator != null;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid()) return;
            animator.SetBool(hoverBoolName, true);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!IsValid()) return;
            animator.SetBool(hoverBoolName, false);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid()) return;
            animator.SetTrigger(selectTriggerName);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!IsValid()) return;
            animator.SetTrigger(deselectTriggerName);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid()) return;
            animator.SetTrigger(activatedTriggerName);
        }

        public override VisualElement CreateVisualElement(Action onValueChanged)
        {
            var container = new VisualElement();
            var nameField = new TextField("Name") { value = feedbackName };
            nameField.RegisterValueChangedCallback(evt => { feedbackName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(nameField);
            var animatorField = new UnityEditor.UIElements.ObjectField("Animator") { objectType = typeof(Animator), value = animator };
            animatorField.RegisterValueChangedCallback(evt => { animator = (Animator)evt.newValue; onValueChanged?.Invoke(); });
            container.Add(animatorField);
            var hoverBoolField = new TextField("Hover Bool") { value = hoverBoolName };
            hoverBoolField.RegisterValueChangedCallback(evt => { hoverBoolName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(hoverBoolField);
            var selectTriggerField = new TextField("Select Trigger") { value = selectTriggerName };
            selectTriggerField.RegisterValueChangedCallback(evt => { selectTriggerName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(selectTriggerField);
            var deselectTriggerField = new TextField("Deselect Trigger") { value = deselectTriggerName };
            deselectTriggerField.RegisterValueChangedCallback(evt => { deselectTriggerName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(deselectTriggerField);
            var activatedTriggerField = new TextField("Activated Trigger") { value = activatedTriggerName };
            activatedTriggerField.RegisterValueChangedCallback(evt => { activatedTriggerName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(activatedTriggerField);
            return container;
        }
    }

    /// <summary>
    /// Haptic feedback for controller vibration
    /// </summary>
    [Serializable]
    public class HapticFeedback : FeedbackData
    {
        [Header("Haptic Settings")] [SerializeField]
        public float hoverAmplitude = 0.3f;

        [SerializeField] public float hoverDuration = 0.1f;
        [SerializeField] public float selectAmplitude = 0.5f;
        [SerializeField] public float selectDuration = 0.2f;
        [SerializeField] public float activateAmplitude = 1f;
        [SerializeField] public float activateDuration = 0.3f;



        public override bool IsValid()
        {
            return true; // Always valid for haptics
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid()) return;
            ExecuteHaptic(interactor.HandIdentifier, hoverAmplitude, hoverDuration);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid()) return;
            ExecuteHaptic(interactor.HandIdentifier, selectAmplitude, selectDuration);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid()) return;
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

        public override VisualElement CreateVisualElement(Action onValueChanged)
        {
            var container = new VisualElement();
            var nameField = new TextField("Name") { value = feedbackName };
            nameField.RegisterValueChangedCallback(evt => { feedbackName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(nameField);
            var hoverIntensityField = new FloatField("Hover Intensity") { value = hoverAmplitude };
            hoverIntensityField.RegisterValueChangedCallback(evt => { hoverAmplitude = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(hoverIntensityField);
            var selectIntensityField = new FloatField("Select Intensity") { value = selectAmplitude };
            selectIntensityField.RegisterValueChangedCallback(evt => { selectAmplitude = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(selectIntensityField);
            var activateIntensityField = new FloatField("Activate Intensity") { value = activateAmplitude };
            activateIntensityField.RegisterValueChangedCallback(evt => { activateAmplitude = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(activateIntensityField);
            var durationField = new FloatField("Duration") { value = activateDuration };
            durationField.RegisterValueChangedCallback(evt => { activateDuration = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(durationField);
            return container;
        }
    }

    /// <summary>
    /// Audio feedback for sound effects
    /// </summary>
    [Serializable]
    public class AudioFeedback : FeedbackData
    {
        [Header("Audio Settings")] [SerializeField]
        public AudioSource audioSource;

        [SerializeField] public AudioClip hoverCLip;
        [SerializeField] public AudioClip hoverExitClip;
        [SerializeField] public AudioClip selectClip;
        [SerializeField] public AudioClip deselectClip;
        [SerializeField] public AudioClip activateClip;
        [SerializeField] public float hoverVolume = 0.5f;
        [SerializeField] public float selectVolume = 0.7f;
        [SerializeField] public float activateVolume = 1f;
        [SerializeField] public bool useSpatialAudio = true;
        [SerializeField] public bool randomizePitch = false; 
        [SerializeField] public float pitchRandomization = 0.1f;

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
            return audioSource != null;
        }

        public override void OnHoverStarted(InteractorBase interactor)
        {
            if (!IsValid() || hoverCLip == null) return;
            PlaySound(hoverCLip, hoverVolume);
        }

        public override void OnHoverEnded(InteractorBase interactor)
        {
            if (!IsValid() || hoverExitClip == null) return;
            PlaySound(hoverExitClip, hoverVolume);
        }

        public override void OnSelected(InteractorBase interactor)
        {
            if (!IsValid() || selectClip == null) return;
            PlaySound(selectClip, selectVolume);
        }

        public override void OnDeselected(InteractorBase interactor)
        {
            if (!IsValid() || deselectClip == null) return;
            PlaySound(deselectClip, selectVolume);
        }

        public override void OnActivated(InteractorBase interactor)
        {
            if (!IsValid() || activateClip == null) return;
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

        public override VisualElement CreateVisualElement(Action onValueChanged)
        {
            var container = new VisualElement();
            var nameField = new TextField("Name") { value = feedbackName };
            nameField.RegisterValueChangedCallback(evt => { feedbackName = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(nameField);
            var audioSourceField = new UnityEditor.UIElements.ObjectField("Audio Source") { objectType = typeof(AudioSource), value = audioSource };
            audioSourceField.RegisterValueChangedCallback(evt => { audioSource = (AudioSource)evt.newValue; onValueChanged?.Invoke(); });
            container.Add(audioSourceField);
            var hoverClipField = new UnityEditor.UIElements.ObjectField("Hover Clip") { objectType = typeof(AudioClip), value = hoverCLip };
            hoverClipField.RegisterValueChangedCallback(evt => { hoverCLip = (AudioClip)evt.newValue; onValueChanged?.Invoke(); });
            container.Add(hoverClipField);
            var selectClipField = new UnityEditor.UIElements.ObjectField("Select Clip") { objectType = typeof(AudioClip), value = selectClip };
            selectClipField.RegisterValueChangedCallback(evt => { selectClip = (AudioClip)evt.newValue; onValueChanged?.Invoke(); });
            container.Add(selectClipField);
            var activateClipField = new UnityEditor.UIElements.ObjectField("Activate Clip") { objectType = typeof(AudioClip), value = activateClip };
            activateClipField.RegisterValueChangedCallback(evt => { activateClip = (AudioClip)evt.newValue; onValueChanged?.Invoke(); });
            container.Add(activateClipField);
            var volumeField = new FloatField("Volume") { value = hoverVolume };
            volumeField.RegisterValueChangedCallback(evt => { hoverVolume = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(volumeField);
            var spatialAudioField = new Toggle("Spatial Audio") { value = useSpatialAudio };
            spatialAudioField.RegisterValueChangedCallback(evt => { useSpatialAudio = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(spatialAudioField);
            var pitchRandomizationField = new Toggle("Pitch Randomization") { value = randomizePitch };
            pitchRandomizationField.RegisterValueChangedCallback(evt => { randomizePitch = evt.newValue; onValueChanged?.Invoke(); });
            container.Add(pitchRandomizationField);
            return container;
        }
    }
}