
using Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// ScriptableObject that stores a Gesture value for use in gesture-based interactions.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interaction System/Gesture")]
    public class GestureVariable : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The current gesture value stored in this variable.")]
        public Gesture value;
    }

}
