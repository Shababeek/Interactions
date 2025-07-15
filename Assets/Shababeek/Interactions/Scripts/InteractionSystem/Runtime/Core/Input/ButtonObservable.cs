using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Observable for button state changes, allowing subscription to button events in the interaction system.
    /// </summary>
    public class ButtonObservable
    {
        //TODO: rewrite to use UniRX instead
        private readonly List<IObserver<ButtonState>> _observers = new();
        private readonly Subject<ButtonState> _onStateChanged = new();
        private bool _isDown;
        
        public IObservable<ButtonState> OnStateChanged => _onStateChanged;

        public bool ButtonState
        {
            set
            {
                if (_isDown == value) return;
                _isDown = value;
                _onStateChanged.OnNext(_isDown ? Core.ButtonState.Down : Core.ButtonState.Up);
            }
        }
    }
}