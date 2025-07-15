using UnityEngine;

namespace Shababeek.Core
{
    public class ObjectEnableRaiser : MonoBehaviour
    {
        [SerializeField] private GameEvent gameEvent;

        private void OnEnable()
        {
            gameEvent?.Raise();
        }
    }
}