using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// [DEPRECATED] Controls animator parameters based on interaction events.
    /// Automatically sets bool and trigger parameters on the animator component
    /// when interaction events occur (hover, select, deselect).
    /// </summary>
    /// <remarks>
    /// <b>DEPRECATED:</b> This component is deprecated. Please use the FeedbackSystem with AnimationFeedback instead.
    /// The FeedbackSystem provides more comprehensive feedback options and better integration with the interaction system.
    /// 
    /// To migrate:
    /// 1. Remove this Animinteractable component
    /// 2. Add a FeedbackSystem component to your interactable
    /// 3. Add an AnimationFeedback to the FeedbackSystem
    /// 4. Configure the AnimationFeedback settings for your desired animation behavior
    /// </remarks>
    [RequireComponent(typeof(InteractableBase))]
    [RequireComponent(typeof(Animator))]
    [System.Obsolete("This component is deprecated. Use FeedbackSystem with AnimationFeedback instead for better integration and more features.")]
    public class Animinteractable : MonoBehaviour
    {
        [Tooltip("Name of the bool parameter to set for hover state in the animator.")]
        [SerializeField] private string hoverBoolName = "Hovered";
        
        [Tooltip("Name of the trigger parameter to set when the object is selected.")]
        [SerializeField] private string selectedTrigger = "Selected";
        
        [Tooltip("Name of the trigger parameter to set when the object is deselected.")]
        [SerializeField] private string unselectedTrigger = "Deselected";
        
        [Tooltip("Reference to an AnimationFeedback component for additional animation control.")]
        [SerializeField] private AnimationFeedback AnimationFeedback;
        private void Awake()
        {
            // Deprecation warning
            Debug.LogWarning($"[DEPRECATED] Animinteractable on {gameObject.name} is deprecated. " +
                           "Please migrate to FeedbackSystem with AnimationFeedback for better integration and more features. " +
                           "See the component documentation for migration steps.");
            
            var animator = GetComponent<Animator>();
            var intractable = GetComponent<InteractableBase>();
            intractable.OnHoverStarted.Do(_ => animator.SetBool(hoverBoolName, true)).Subscribe().AddTo(this);
            intractable.OnHoverEnded.Do(_ => animator.SetBool(hoverBoolName, false)).Subscribe().AddTo(this);
            intractable.OnSelected.Do(_ => animator.SetTrigger(selectedTrigger)).Subscribe().AddTo(this);
            intractable.OnDeselected.Do(_ => animator.SetTrigger(unselectedTrigger)).Subscribe().AddTo(this);
        }
    }
}