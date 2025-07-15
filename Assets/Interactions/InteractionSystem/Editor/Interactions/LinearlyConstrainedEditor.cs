using UnityEditor;

namespace Kandooz.InteractionSystem.Interactions.Editors
{
    [CustomEditor(typeof(ConstrainedInteractableBase))]
    public class LinearlyConstrainedEditor : Editor
    {
        private ConstrainedInteractableBase interactable;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
        }
    }
}