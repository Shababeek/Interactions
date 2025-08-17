using System;
using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Tweens the rotation and position of a Transform between two pivot points over time.
    /// Provides both event-based and async/await interfaces for tween completion.
    /// </summary>
    /// <remarks>
    /// This class smoothly interpolates a transform's position and rotation from its current state
    /// to a target transform's state. It's useful for smooth camera movements, object transitions,
    /// or any transform-based animations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transformTween = new TransformTweenable();
    /// transformTween.Initialize(transform, targetTransform);
    /// 
    /// // Listen for completion
    /// transformTween.OnTweenComplete += () => Debug.Log("Transform tween complete!");
    /// 
    /// // Add to tweener for automatic updates
    /// tweener.AddTweenable(transformTween);
    /// </code>
    /// </example>
    public class TransformTweenable : ITweenable
    {
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Transform _target;
        private Transform _transform;
        private float _time = 0;
        
        /// <summary>
        /// Event fired when the transform tween animation completes.
        /// </summary>
        /// <remarks>
        /// Subscribe to this event to perform actions when the transform reaches its target
        /// position and rotation. This event is fired once when the tween finishes.
        /// </remarks>
        public event Action OnTweenComplete;
        
        /// <summary>
        /// Performs one step of the transform tween animation.
        /// </summary>
        /// <param name="scaledDeltaTime">The scaled delta time for this frame</param>
        /// <returns>True if the tween has completed, false if it's still running</returns>
        /// <remarks>
        /// This method is called by VariableTweener each frame to update the transform tween.
        /// It interpolates both position and rotation using linear interpolation.
        /// </remarks>
        public bool Tween(float scaledDeltaTime)
        {
            if (_transform == null || _target == null)
            {
                Debug.LogWarning("TransformTweenable: Transform or target is null, cannot tween.");
                return true; // Return true to remove from tweener
            }
            
            _time += scaledDeltaTime;
            _transform.position = Vector3.Lerp(_startPosition, _target.position, _time);
            _transform.rotation = Quaternion.Lerp(_startRotation, _target.rotation, _time);
            
            if (_time < 1) return false;
            
            try
            {
                OnTweenComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in TransformTweenable OnTweenComplete event: {e}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Initializes the transform tweenable with source and target transforms.
        /// </summary>
        /// <param name="transform">The source transform to animate</param>
        /// <param name="target">The target transform to animate towards</param>
        /// <remarks>
        /// This method must be called before the tween can start. It captures the current
        /// position and rotation of the source transform as the starting point.
        /// </remarks>
        /// <example>
        /// <code>
        /// var tween = new TransformTweenable();
        /// tween.Initialize(myTransform, targetTransform);
        /// </code>
        /// </example>
        public void Initialize(Transform transform, Transform target)
        {
            if (transform == null)
            {
                Debug.LogError("TransformTweenable: Source transform cannot be null.");
                return;
            }
            
            if (target == null)
            {
                Debug.LogError("TransformTweenable: Target transform cannot be null.");
                return;
            }
            
            _transform = transform;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _target = target;
            _time = 0;
        }
    }
}