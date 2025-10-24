using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Triggers an animation and completes the step when the animation ends.
    /// </summary>
    [AddComponentMenu(menuName: "Shababeek/Sequencing/Actions/AnimationAction")]
    public class AnimationAction : AbstractSequenceAction
    {
        [Tooltip("Name of the animation trigger parameter in the animator.")]
        [SerializeField] private string animationTriggerName;
        
        [Tooltip("The animator to control.")]
        [SerializeField] private Animator animator;
        void Awake()
        {
        }
        
        /// <summary>
        /// Completes the step. Must be called from the animation event.
        /// </summary>
        public void AnimationEnded()
        {
            Step.CompleteStep();
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            animator.SetTrigger(animationTriggerName);
        }
    }
}