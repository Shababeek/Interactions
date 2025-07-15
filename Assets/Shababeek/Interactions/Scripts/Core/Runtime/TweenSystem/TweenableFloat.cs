using System;
using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Tweens a float value between two values over time using linear interpolation.
    /// Provides both event-based and async/await interfaces for tween completion.
    /// </summary>

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
        /// <example>
        /// <code>
        /// tweenableFloat.Value = 5f; // Starts tweening to 5
        /// </code>
        /// </example>
        public float Value
        {
            get
            {
                return _value;
            }

            set
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
#endif
                {
                    _t = 0;
                    _start = this._value;
                    _target = value;
                    _tweener.AddTweenable(this);
                }
#if UNITY_EDITOR
                else
                {
                    this._value = value;
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
        public TweenableFloat(VariableTweener tweener,Action<float> onChange=null, float rate = 2f, float value = 0)
        {
            _start = _target = this._value = value;
            this._rate = rate;
            this._t = 0;
            this.OnChange = onChange;
            this._tweener = tweener;
        }
        

        public bool Tween(float scaledDeltaTime)
        {
            _t += _rate * scaledDeltaTime;
            this._value = Mathf.Lerp(_start, _target, _t);
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