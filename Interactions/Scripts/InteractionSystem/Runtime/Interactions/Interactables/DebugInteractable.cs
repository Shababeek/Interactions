using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Debug interactable that logs interaction events to the console.
    /// Useful for testing and debugging interaction systems.
    /// </summary>
    public class DebugInteractable : InteractableBase
    {
        protected override void UseStarted()
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

        protected override bool Select()
        {
            Debug.Log("Selected");
            return false;
        }

        protected override void DeSelected()
        {
            Debug.Log("Deselected");
        }
    }
}