using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    [RequireComponent(typeof(InteractableBase))]
    [RequireComponent(typeof(Animator))]
    public class Animinteractable : MonoBehaviour
    {
        [SerializeField] private string hoverBoolName = "Hovered";
        [SerializeField] private string selectedTrigger = "Selected";
        [SerializeField] private string unselectedTrigger = "Deselected";
        [SerializeField] private AnimationFeedback AnimationFeedback;
        [SerializeField]private AudioFeedback animator;
        [SerializeField]private MaterialFeedback materialFeedback;
        private void Awake()
        {
            var animator = GetComponent<Animator>();
            var intractable = GetComponent<InteractableBase>();
            intractable.OnHoverStarted.Do(_ => animator.SetBool(hoverBoolName, true)).Subscribe().AddTo(this);
            intractable.OnHoverEnded.Do(_ => animator.SetBool(hoverBoolName, false)).Subscribe().AddTo(this);
            intractable.OnSelected.Do(_ => animator.SetTrigger(selectedTrigger)).Subscribe().AddTo(this);
            intractable.OnDeselected.Do(_ => animator.SetTrigger(unselectedTrigger)).Subscribe().AddTo(this);
        }
    }
}