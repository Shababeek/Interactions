using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    [AddComponentMenu(menuName: "Shababeek/Sequencing/Actions/AnimationAction")]
    public class AnimationAction : AbstractSequenceAction
    {
        [SerializeField] private string animationTriggerName;
        [SerializeField] private Animator animator;
        void Awake()
        {
        }
        /// <summary>
        /// this function must be called from the animation 
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