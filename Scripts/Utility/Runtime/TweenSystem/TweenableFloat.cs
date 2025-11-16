using System;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Tweens a float value between two values over time using linear interpolation.
    /// Provides both event-based and async/await interfaces for tween completion.
    /// </summary>
    /// <remarks>
    /// This class automatically handles tweening between start and target values at a specified rate.
    /// It integrates with VariableTweener for automatic frame updates and provides events for
    /// real-time value changes and completion notifications.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a tweenable float
    /// var tweenable = new TweenableFloat(tweener, value => Debug.Log($"Value: {value}"));
    /// 
    /// // Start tweening to a new value
    /// tweenable.Value = 10f;
    /// 
    /// // Listen for completion
    /// tweenable.OnFinished += () => Debug.Log("Tween complete!");
    /// </code>
    /// </example>
    public class TweenableFloat : ITweenable
    {
        /// <summary>
        /// Event fired when the float value changes during tweening.
        /// </summary>
        /// <remarks>
        /// Subscribe to this event to get real-time updates of the tweened value.
        /// The parameter is the current interpolated value.
        /// </remarks>
        public event Action<float> OnChange;
        
        /// <summary>
        /// Event fired when the tween animation completes.
        /// </summary>
        /// <remarks>
        /// Subscribe to this event to perform actions when the tween finishes.
        /// This event is fired once when the target value is reached.
        /// </remarks>
        public event Action OnFinished;
        
        private float _start;
        private float _target;
        private float _value;
        private float _rate;
        private float _t;
        private readonly VariableTweener _tweener;
        
        /// <summary>
        /// Gets or sets the current float value. Setting this property starts a tween to the new value.
        /// </summary>
        /// <remarks>
        /// When setting a new value, the current value becomes the start point and the new value
        /// becomes the target. The tween automatically starts and will interpolate between these
        /// values over time at the specified rate.
        /// </remarks>
        /// <example>
        /// <code>
        /// tweenableFloat.Value = 5f; // Starts tweening to 5
        /// </code>
        /// </example>
        public float Value
        {
            get => _value;
            set
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
#endif
                {
                    _t = 0;
                    _start = _value;
                    _target = value;
                    _tweener.AddTweenable(this);
                }
#if UNITY_EDITOR
                else
                {
                    _value = value;
                    OnChange?.Invoke(value);
                }
#endif
            }
        }

        /// <summary>
        /// Initializes a new instance of the TweenableFloat class.
        /// </summary>
        /// <param name="tweener">The VariableTweener component that will manage this tween</param>
        /// <param name="onChange">Optional callback for value changes during tweening</param>
        /// <param name="rate">The tweening rate (speed). Default is 2f</param>
        /// <param name="value">The initial value. Default is 0f</param>
        /// <remarks>
        /// The rate parameter controls how fast the tween progresses. Higher values result in faster
        /// tweening, while lower values create slower, more gradual animations.
        /// </remarks>
        public TweenableFloat(VariableTweener tweener, Action<float> onChange = null, float rate = 2f, float value = 0)
        {
            _start = _target = _value = value;
            _rate = rate;
            _t = 0;
            OnChange = onChange;
            _tweener = tweener;
        }
        
        /// <summary>
        /// Performs one step of the tween animation.
        /// </summary>
        /// <param name="scaledDeltaTime">The scaled delta time for this frame</param>
        /// <returns>True if the tween has completed, false if it's still running</returns>
        /// <remarks>
        /// This method is called by VariableTweener each frame to update the tween.
        /// It interpolates between start and target values using linear interpolation.
        /// </remarks>
        public bool Tween(float scaledDeltaTime)
        {
            _t += _rate * scaledDeltaTime;
            _value = Mathf.Lerp(_start, _target, _t);
            OnChange?.Invoke(_value);

            if (_t >= 1)
            {
                OnFinished?.Invoke();
                return true;
            }
            return false;
        }
    }
}