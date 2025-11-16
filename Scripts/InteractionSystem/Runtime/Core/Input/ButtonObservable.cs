using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Observable for button state changes, allowing subscription to button events in the interaction system.
    /// Provides a clean UniRx-based implementation for button state management.
    /// </summary>
    public class ButtonObservable
    {
        private readonly Subject<VRButtonState> _onStateChanged = new();
        private bool _isDown;
        
        /// <summary>
        /// Observable that fires when the button state changes.
        /// </summary>
        /// <value>An observable that emits ButtonState changes.</value>
        public IObservable<VRButtonState> OnStateChanged => _onStateChanged;

        /// <summary>
        /// Sets the button state and triggers the observable if the state has changed.
        /// </summary>
        /// <value>The button state (true for down, false for up).</value>
        public bool ButtonState
        {
            set
            {
                if (_isDown == value) return;
                _isDown = value;
                _onStateChanged.OnNext(_isDown ? VRButtonState.Down : VRButtonState.Up);
            }
        }

        /// <summary>
        /// Gets the current button state.
        /// </summary>
        /// <value>The current button state (true for down, false for up).</value>
        public bool IsDown => _isDown;
    }
}