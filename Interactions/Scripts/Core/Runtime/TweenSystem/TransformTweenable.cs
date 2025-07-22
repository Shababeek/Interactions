using System;
using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Tweens the rotation and position of a Transform between two pivot points over time.
    /// Provides both event-based and async/await interfaces for tween completion.
    /// </summary>
    public class TransformTweenable :ITweenable
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
        
        public bool Tween(float scaledDeltaTime)
        {
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
                Debug.LogError(e);
                throw;
            }
            return true;
        }
        
        
        /// <summary>
        /// Initializes the transform tweenable with source and target transforms.
        /// </summary>
        /// <param name="transform">The source transform to animate</param>
        /// <param name="target">The target transform to animate towards</param>
        public void Initialize(Transform transform, Transform target)
        {
            this._transform = transform;
            _startPosition = transform.position;
            _startRotation = this._transform.rotation;
            this._target = target;
            _time = 0;
        }
    }
}