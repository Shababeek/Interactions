using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public class ButtonObservable : IObservable<ButtonState>
    {
        private readonly List<IObserver<ButtonState>> observers = new();
        private bool down;

        public bool ButtonState
        {
            set
            {
                if (down != value)
                {
                    down = value;
                    OnStateChanged(down ? Core.ButtonState.Down : Core.ButtonState.Up);
                }
            }
        }

        public IDisposable Subscribe(IObserver<ButtonState> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }

        public void OnStateChanged(ButtonState state)
        {
            for (var i = observers.Count - 1; i >= 0; i--)
            {
                try
                {
                    observers[i].OnNext(state);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<ButtonState>> observers;
            private IObserver<ButtonState> observer;

            public Unsubscriber(List<IObserver<ButtonState>> observers, IObserver<ButtonState> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose()
            {
                if (observer != null && observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
        }
    }
}