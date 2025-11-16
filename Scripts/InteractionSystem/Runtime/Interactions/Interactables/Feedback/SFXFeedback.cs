using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// Plays sound effects for different interaction states (hover, select, activate)
    /// </summary>
    [RequireComponent(typeof(InteractableBase))]
    [RequireComponent(typeof(AudioSource))]
    public class SFXFeedback : MonoBehaviour
    {
        [Header("Hover Sound Effects")]
        [Tooltip("Whether to play sound effects when hovering over the interactable.")]
        [SerializeField] private bool playHoverSFX = true;
        [Tooltip("Audio clip to play when hover starts.")]
        [SerializeField] private AudioClip hoverEnterClip;
        [Tooltip("Audio clip to play when hover ends.")]
        [SerializeField] private AudioClip hoverExitClip;
        [Tooltip("Volume for hover sound effects (0-1).")]
        [SerializeField] [Range(0f, 1f)] private float hoverVolume = 0.5f;
        
        [Header("Selection Sound Effects")]
        [Tooltip("Whether to play sound effects when selecting the interactable.")]
        [SerializeField] private bool playSelectionSFX = true;
        [Tooltip("Audio clip to play when the object is selected.")]
        [SerializeField] private AudioClip selectClip;
        [Tooltip("Audio clip to play when the object is deselected.")]
        [SerializeField] private AudioClip deselectClip;
        [Tooltip("Volume for selection sound effects (0-1).")]
        [SerializeField] [Range(0f, 1f)] private float selectionVolume = 0.7f;
        
        [Header("Activation Sound Effects")]
        [Tooltip("Whether to play sound effects when activating the interactable.")]
        [SerializeField] private bool playActivationSFX = true;
        [Tooltip("Audio clip to play when the object is activated (use button pressed).")]
        [SerializeField] private AudioClip activateClip;
        [Tooltip("Volume for activation sound effects (0-1).")]
        [SerializeField] [Range(0f, 1f)] private float activationVolume = 1f;
        
        [Header("Audio Settings")]
        [Tooltip("Whether to use 3D spatial audio (false for 2D audio).")]
        [SerializeField] private bool useSpatialAudio = true;
        [Tooltip("Minimum distance for spatial audio falloff.")]
        [SerializeField] [Range(0f, 5f)] private float minDistance = 1f;
        [Tooltip("Maximum distance for spatial audio falloff.")]
        [SerializeField] [Range(0f, 25f)] private float maxDistance = 10f;
        [Tooltip("Whether to randomize the pitch of sound effects.")]
        [SerializeField] private bool randomizePitch = false;
        [Tooltip("Amount of pitch randomization to apply (±this value).")]
        [SerializeField] [Range(0.8f, 1.2f)] private float pitchVariation = 0.1f;
        
        private AudioSource _audioSource;
        private InteractableBase _interactable;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _interactable = GetComponent<InteractableBase>();
            
            // Configure audio source settings
            ConfigureAudioSource();
            
            // Subscribe to interaction events
            SubscribeToEvents();
        }
        
        private void ConfigureAudioSource()
        {
            _audioSource.spatialBlend = useSpatialAudio ? 1f : 0f;
            _audioSource.minDistance = minDistance;
            _audioSource.maxDistance = maxDistance;
            _audioSource.playOnAwake = false;
        }
        
        private void SubscribeToEvents()
        {
            // Hover events
            if (playHoverSFX)
            {
                _interactable.OnHoverStarted
                    .Where(_ => hoverEnterClip != null)
                    .Do(_ => PlaySound(hoverEnterClip, hoverVolume))
                    .Subscribe().AddTo(this);
                    
                _interactable.OnHoverEnded
                    .Where(_ => hoverExitClip != null)
                    .Do(_ => PlaySound(hoverExitClip, hoverVolume))
                    .Subscribe().AddTo(this);
            }
            
            // Selection events
            if (playSelectionSFX)
            {
                _interactable.OnSelected
                    .Where(_ => selectClip != null)
                    .Do(_ => PlaySound(selectClip, selectionVolume))
                    .Subscribe().AddTo(this);
                    
                _interactable.OnDeselected
                    .Where(_ => deselectClip != null)
                    .Do(_ => PlaySound(deselectClip, selectionVolume))
                    .Subscribe().AddTo(this);
            }
            
            // Activation events
            if (playActivationSFX)
            {
                _interactable.OnUseStarted
                    .Where(_ => activateClip != null)
                    .Do(_ => PlaySound(activateClip, activationVolume))
                    .Subscribe().AddTo(this);
            }
        }
        
        private void PlaySound(AudioClip clip, float volume)
        {
            if (clip == null || _audioSource == null) return;
            
            // Set clip and volume
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            
            // Apply pitch variation if enabled
            if (randomizePitch)
            {
                float pitchVariationAmount = Random.Range(-pitchVariation, pitchVariation);
                _audioSource.pitch = 1f + pitchVariationAmount;
            }
            else
            {
                _audioSource.pitch = 1f;
            }
            
            // Play the sound
            _audioSource.Play();
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure volume values are within valid range
            hoverVolume = Mathf.Clamp01(hoverVolume);
            selectionVolume = Mathf.Clamp01(selectionVolume);
            activationVolume = Mathf.Clamp01(activationVolume);
            pitchVariation = Mathf.Clamp(pitchVariation, 0f, 0.4f);
            
            // Ensure distance values are logical
            if (minDistance > maxDistance)
            {
                maxDistance = minDistance + 1f;
            }
        }
        #endif
    }
} 