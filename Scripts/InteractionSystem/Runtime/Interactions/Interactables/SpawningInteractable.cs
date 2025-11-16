using System.Threading.Tasks;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Interactable that spawns a new grabable object when selected.
    /// Automatically transfers the grab to the newly spawned object.
    /// </summary>
    public class SpawningInteractable : InteractableBase
    {
        [Tooltip("The grabable prefab to spawn when this interactable is selected.")]
        [SerializeField] private Grabable prefab;
        
        protected override void UseStarted(){}
        protected override void StartHover(){}
        protected override void EndHover(){}

        protected override bool Select()
        {
            var grabable = Instantiate(prefab);
            grabable.transform.position = this.transform.position;
            var interactor = CurrentInteractor;
            interactor.DeSelect();
            interactor.CurrentInteractable = grabable;
            interactor.Select();
            return true;
        }

        protected override void DeSelected()
        {
        }
    }
}