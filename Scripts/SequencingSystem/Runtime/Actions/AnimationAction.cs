using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Triggers an animation and completes the step when the animation ends.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/AnimationAction")]
    public class AnimationAction : AbstractSequenceAction
    {
        [Tooltip("Name of the animation trigger parameter in the animator.")]
        [SerializeField] private string animationTriggerName;

        [Tooltip("The animator to control.")]
        [SerializeField] private Animator animator;

        [Tooltip("If true, automatically completes the step after the animation state finishes.")]
        [SerializeField] private bool autoCompleteOnAnimationEnd = false;

        [Tooltip("Layer index to monitor for auto-complete (default: 0).")]
        [SerializeField] private int animationLayer = 0;

        private bool _waitingForAnimation = false;
        private int _targetStateHash;

        /// <summary>
        /// Completes the step. Can be called from an animation event.
        /// </summary>
        public void AnimationEnded()
        {
            if (Started)
            {
                CompleteStep();
            }
        }

        private void Update()
        {
            if (!_waitingForAnimation || animator == null) return;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
            if (stateInfo.normalizedTime >= 1f && !animator.IsInTransition(animationLayer))
            {
                _waitingForAnimation = false;
                CompleteStep();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                if (animator == null || string.IsNullOrEmpty(animationTriggerName)) return;
                animator.SetTrigger(animationTriggerName);

                if (autoCompleteOnAnimationEnd)
                {
                    _waitingForAnimation = true;
                }
            }
            else
            {
                _waitingForAnimation = false;
            }
        }
    }
}