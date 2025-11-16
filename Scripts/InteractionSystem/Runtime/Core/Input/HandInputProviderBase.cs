using System;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Base implementation of IHandInputProvider with common functionality for button observables and finger tracking.
    /// Provides events for when the provider becomes active or inactive, enabling event-based switching.
    /// </summary>
    public abstract class HandInputProviderBase : MonoBehaviour, IHandInputProvider
    {
        [Header("Configuration")]
        [Tooltip("Which hand this provider is reading input for.")]
        [SerializeField] protected HandIdentifier handedness = HandIdentifier.Left;
        
        [Tooltip("Priority of this provider (higher = preferred when multiple providers available).")]
        [SerializeField] protected int priority = 0;
        
        [Header("Button Thresholds")]
        [Tooltip("Finger curl value threshold for trigger button press detection.")]
        [SerializeField] protected float triggerThreshold = 0.2f;
        
        [Tooltip("Finger curl value threshold for grip button press detection.")]
        [SerializeField] protected float gripThreshold = 0.2f;

        private readonly ButtonObservable _triggerObserver = new();
        private readonly ButtonObservable _gripObserver = new();
        private readonly ButtonObservable _aButtonObserver = new();
        private readonly ButtonObservable _bButtonObserver = new();
        private readonly float[] _fingers = new float[5];
        
        private bool _wasActive = false;
        
        /// <summary>
        /// Event raised when this provider becomes active.
        /// </summary>
        public event Action OnProviderActivated;
        
        /// <summary>
        /// Event raised when this provider becomes inactive.
        /// </summary>
        public event Action OnProviderDeactivated;
        
        /// <summary>
        /// Observable for trigger button state changes.
        /// </summary>
        public IObservable<VRButtonState> TriggerObservable => _triggerObserver.OnStateChanged;
        
        /// <summary>
        /// Observable for grip button state changes.
        /// </summary>
        public IObservable<VRButtonState> GripObservable => _gripObserver.OnStateChanged;
        
        /// <summary>
        /// Observable for A button state changes.
        /// </summary>
        public IObservable<VRButtonState> AButtonObservable => _aButtonObserver.OnStateChanged;
        
        /// <summary>
        /// Observable for B button state changes.
        /// </summary>
        public IObservable<VRButtonState> BButtonObservable => _bButtonObserver.OnStateChanged;
        
        /// <summary>
        /// Gets finger curl value by index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).
        /// </summary>
        public float this[int fingerIndex]
        {
            get => fingerIndex >= 0 && fingerIndex < 5 ? _fingers[fingerIndex] : 0f;
            private set
            {
                if (fingerIndex >= 0 && fingerIndex < 5)
                    _fingers[fingerIndex] = Mathf.Clamp01(value);
            }
        }
        
        /// <summary>
        /// Gets finger curl value by finger name.
        /// </summary>
        public float this[FingerName finger]
        {
            get => this[(int)finger];
            protected set => this[(int)finger] = value;
        }

        /// <summary>
        /// Checks if this provider is currently providing valid input data.
        /// </summary>
        public abstract bool IsActive { get; }
        
        /// <summary>
        /// Priority of this provider (higher values take precedence).
        /// </summary>
        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        /// <summary>
        /// Which hand this provider is reading.
        /// </summary>
        public HandIdentifier Handedness
        {
            get => handedness;
            set => handedness = value;
        }
        
        protected virtual void OnEnable()
        {
            // Override in derived classes for initialization
        }
        
        protected virtual void OnDisable()
        {
            // Override in derived classes for cleanup
            
            // Deactivate if we were active
            if (_wasActive)
            {
                _wasActive = false;
                OnProviderDeactivated?.Invoke();
            }
        }
        
        protected virtual void Update()
        {
            // Check for activation state changes
            bool isActive = IsActive;
            
            if (isActive != _wasActive)
            {
                _wasActive = isActive;
                
                if (isActive)
                    OnProviderActivated?.Invoke();
                else
                    OnProviderDeactivated?.Invoke();
            }
            
            // Only update if active
            if (!isActive)
                return;
            
            // Update finger values from the specific input source
            UpdateFingerValues();
            
            // Update button states based on finger values
            UpdateButtonStates();
        }
        
        /// <summary>
        /// Override this to read finger values from your specific input source.
        /// Set finger values using: this[fingerIndex] = value;
        /// </summary>
        protected abstract void UpdateFingerValues();
        
        /// <summary>
        /// Updates button observers based on current finger curl values.
        /// Can be overridden for custom button detection logic.
        /// </summary>
        protected virtual void UpdateButtonStates()
        {
            // Trigger = index finger curl
            _triggerObserver.ButtonState = this[FingerName.Index] > triggerThreshold;
            
            // Grip = average of middle, ring, pinky
            float gripValue = (this[FingerName.Middle] + this[FingerName.Ring] + this[FingerName.Pinky]) / 3f;
            _gripObserver.ButtonState = gripValue > gripThreshold;
        }
        
        /// <summary>
        /// Helper method to set all finger values at once.
        /// </summary>
        protected void SetFingerValues(float thumb, float index, float middle, float ring, float pinky)
        {
            this[0] = thumb;
            this[1] = index;
            this[2] = middle;
            this[3] = ring;
            this[4] = pinky;
        }
        
        /// <summary>
        /// Manually trigger activation event (useful for event-based providers).
        /// </summary>
        protected void TriggerActivation()
        {
            if (!_wasActive)
            {
                _wasActive = true;
                OnProviderActivated?.Invoke();
            }
        }
        
        /// <summary>
        /// Manually trigger deactivation event (useful for event-based providers).
        /// </summary>
        protected void TriggerDeactivation()
        {
            if (_wasActive)
            {
                _wasActive = false;
                OnProviderDeactivated?.Invoke();
            }
        }
    }
}
