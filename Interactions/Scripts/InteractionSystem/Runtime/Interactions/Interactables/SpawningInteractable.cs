using System.Threading.Tasks;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    public class SpawningInteractable : InteractableBase
    {
        [SerializeField] private Grabable  prefab;
        protected override void UseStarted(){}
        protected override void StartHover(){}
        protected override void EndHover(){}

        protected override  bool Select()
        {
                var grabable = Instantiate(prefab);
                grabable.transform.position = this.transform.position;
                var interactor = CurrentInteractor;
                interactor.OnDeSelect();
                interactor.currentInteractable = grabable;
                interactor.OnSelect();
                return true;


        }

        protected override void DeSelected()
        {
            //this.OnStateChanged(InteractionState.None,CurrentInteractor);
        }
        
        private void Awake()
        {

        }
        
    }

}