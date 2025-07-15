using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    public class DebugInteractable : InteractableBase
    {
        protected override void Activate()
        {
            Debug.Log("Activated");
        }

        protected override void StartHover()
        {
            Debug.Log("HoverStart");
        }

        protected override void EndHover()
        {
            Debug.Log("HoverEnd");
        }

        protected override void Select()
        {
            Debug.Log("Selected");
        }

        protected override void DeSelected()
        {
            Debug.Log("Deselected");
        }
    }
}