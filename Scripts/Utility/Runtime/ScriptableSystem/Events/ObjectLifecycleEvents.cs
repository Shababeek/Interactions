using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Triggers events when the object is enabled or disabled.
    /// Useful for responding to object activation/deactivation in the scene.
    /// </summary>
    /// <remarks>
    /// This component automatically raises both GameEvents and UnityEvents when the GameObject
    /// is enabled or disabled, providing a simple way to trigger events on object lifecycle changes.
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Events/Object Lifecycle Events")]
    public class ObjectLifecycleEvents : MonoBehaviour
    {
        [Header("Game Events")]
        [Tooltip("Array of game events to raise when this object is enabled.")]
        [SerializeField] private GameEvent[] gameEventsOnEnable;
        
        [Tooltip("Array of game events to raise when this object is disabled.")]
        [SerializeField] private GameEvent[] gameEventsOnDisable;
        
        [Header("Unity Events")]
        [Tooltip("Unity event raised when this object is enabled.")]
        [SerializeField] private UnityEvent onObjectEnabled;
        
        [Tooltip("Unity event raised when this object is disabled.")]
        [SerializeField] private UnityEvent onObjectDisabled;

        private void OnEnable()
        {
            // Raise GameEvents
            foreach (var gameEvent in gameEventsOnEnable)
            {
                if (gameEvent != null)
                    gameEvent.Raise();
            }
            
            // Raise UnityEvent
            onObjectEnabled?.Invoke();
        }
        
        private void OnDisable()
        {
            // Raise GameEvents
            foreach (var gameEvent in gameEventsOnDisable)
            {
                if (gameEvent != null)
                    gameEvent.Raise();
            }
            
            // Raise UnityEvent
            onObjectDisabled?.Invoke();
        }
    }
}