using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// A backpack that is both grabbable AND a grid inventory (<see cref="InventoryGridSocket"/>).
    /// When the hand grips while hovering an OCCUPIED cell, the item in that cell is retrieved into
    /// the hand instead of grabbing the backpack. Gripping over an empty area (or with an empty grid)
    /// grabs the backpack itself as normal.
    ///
    /// Retrieval uses the same interactor re-target pattern as <see cref="SpawningInteractable"/>:
    /// release this interactable, point the interactor at the stored item's Grabable, and re-select.
    /// The item's own <see cref="Grabable.Select"/> then runs <see cref="Socketable.DetachForGrab"/>,
    /// which restores the item's scale, parent and physics before the hand pose is computed.
    /// </summary>
    [RequireComponent(typeof(InventoryGridSocket))]
    public class InventoryBackpackGrabable : Grabable
    {
        private InventoryGridSocket _inventory;

        protected override void InitializeInteractable()
        {
            base.InitializeInteractable();
            _inventory = GetComponent<InventoryGridSocket>();
        }

        protected override bool Select()
        {
            if (_inventory != null)
            {
                var interactor = CurrentInteractor;
                var item = _inventory.GetItemAtWorldPosition(interactor.GetInteractionPoint());
                if (item != null)
                {
                    var grab = item.GetComponent<Grabable>();
                    if (grab != null && grab.CanInteract(interactor.Hand))
                    {
                        // Hand the stored item to the interactor. Aborts the backpack grab (return true).
                        interactor.DeSelect();
                        interactor.CurrentInteractable = grab;
                        interactor.Select();
                        return true;
                    }
                }
            }

            // Nothing retrievable under the hand — grab the backpack itself.
            return base.Select();
        }
    }
}
