using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Manages multiple tweenable objects and updates them each frame.
    /// Acts as a central coordinator for all ITweenable instances in a GameObject.
    /// </summary>
    /// <remarks>
    /// This MonoBehaviour component manages the lifecycle of multiple ITweenable objects.
    /// It automatically updates all registered tweenables each frame and removes them
    /// when they complete. This provides a centralized way to handle multiple animations
    /// without manually managing each one's update cycle.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Add to a GameObject
    /// var tweener = gameObject.AddComponent<VariableTweener>();
    /// 
    /// // Create and add tweenables
    /// var floatTween = new TweenableFloat(tweener);
    /// var transformTween = new TransformTweenable();
    /// 
    /// tweener.AddTweenable(floatTween);
    /// tweener.AddTweenable(transformTween);
    /// </code>
    /// </example>
    public class VariableTweener : MonoBehaviour
    {

        //TODO: add a way to manage  tweenablesfrom the inspector
        //TODO: add animation curves for the tweenables
        [Header("Tweening Settings")]
        [Tooltip("The scale factor applied to delta time for tweening speed control. Higher values result in faster tweening across all managed tweenables.")]
        [SerializeField] private float _tweenScale = 12f;
        
        private List<ITweenable> _values = new();

        /// <summary>
        /// Gets or sets the scale factor applied to delta time for tweening speed control.
        /// </summary>
        /// <remarks>
        /// Higher values result in faster tweening across all managed tweenables.
        /// This is multiplied by Time.deltaTime to control the overall speed
        /// of all animations managed by this tweener.
        /// </remarks>
        /// <example>
        /// <code>
        /// tweener.TweenScale = 2f; // Double speed
        /// tweener.TweenScale = 0.5f; // Half speed
        /// </code>
        /// </example>
        public float TweenScale
        {
            get => _tweenScale;
            set => _tweenScale = value;
        }

        /// <summary>
        /// Initializes the tweenable list when the component is enabled.
        /// </summary>
        /// <remarks>
        /// This ensures a clean state for the tweenable collection when the component
        /// is re-enabled, preventing any stale references from previous sessions.
        /// </remarks>
        private void OnEnable()
        {
            _values = new List<ITweenable>();
        }
        
        /// <summary>
        /// Adds a tweenable object to the management system.
        /// </summary>
        /// <param name="value">The ITweenable object to add for management</param>
        /// <remarks>
        /// Once added, the tweenable will be automatically updated each frame until
        /// it completes (returns true from its Tween method). The tweener will then
        /// automatically remove it from the management list.
        /// </remarks>
        /// <example>
        /// <code>
        /// var tweenable = new TweenableFloat(this);
        /// AddTweenable(tweenable);
        /// </code>
        /// </example>
        public void AddTweenable(ITweenable value)
        {
            if (value == null)
            {
                Debug.LogWarning("VariableTweener: Attempted to add null tweenable.");
                return;
            }
            
            _values.Add(value);
        }

        /// <summary>
        /// Removes a tweenable object from the management system.
        /// </summary>
        /// <param name="value">The ITweenable object to remove from management</param>
        /// <remarks>
        /// This method safely removes a tweenable from the management list. If the
        /// tweenable is not found in the list, the operation is silently ignored.
        /// This is useful for manually stopping animations before they complete.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Stop a tween before it completes
        /// RemoveTweenable(myTweenable);
        /// </code>
        /// </example>
        public void RemoveTweenable(ITweenable value)
        {
            if (value == null) return;
            
            try
            {
                _values.Remove(value);
            }
            catch
            {
                // Ignored - safe to ignore removal errors
            }
        }
        
        /// <summary>
        /// Updates all managed tweenable objects each frame.
        /// </summary>
        /// <remarks>
        /// This method is called automatically by Unity each frame. It iterates through
        /// all registered tweenables, calls their Tween method, and removes any that
        /// have completed. The iteration is done in reverse order to safely handle
        /// removal during iteration.
        /// </remarks>
        private void Update()
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (_values[i].Tween(Time.deltaTime * _tweenScale))
                {
                    _values.RemoveAt(i);
                }
            }
        }
    }
}