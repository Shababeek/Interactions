using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Raises a game event when the object is enabled.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Object Enable Raiser")]
    public class ObjectEnableRaiser : MonoBehaviour
    {
        [SerializeField] private GameEvent[] gameEvents;

        private void OnEnable()
        {
            foreach (var gameEvent in gameEvents)
            {
                gameEvent.Raise();
            }
        }
    }
}