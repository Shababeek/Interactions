using System;
using UnityEngine;
using UniRx;
namespace Shababeek.Interactions.Core
{
    public abstract class InputManagerBase : MonoBehaviour
    {
        protected readonly HandInputManagerImpl LeftHand = new();
        protected readonly HandInputManagerImpl RightHand = new();

        public IHandInputManager this[HandIdentifier index]
        {
            get
            {
                return index switch
                {
                    HandIdentifier.Left => LeftHand,
                    HandIdentifier.Right => RightHand,
                    _ => null
                };
            }
        }

        /// <summary>
        /// Interface for hand input managers, providing access to hand input data and events.
        /// </summary>
        public interface IHandInputManager
        {
            public IObservable<ButtonState> TriggerObservable { get; }
            public IObservable<ButtonState> GripObservable { get; }
            public IObservable<ButtonState> AButtonObserver { get; }
            public IObservable<ButtonState> BButtonObserver { get; }
            public float this[int index] { get; }
        }

        protected class HandInputManagerImpl : IHandInputManager
        {
            internal readonly ButtonObservable TriggerObserver = new();
            internal readonly ButtonObservable GripObserver = new();
            internal readonly ButtonObservable ButtonAObserver = new();
            internal readonly ButtonObservable ButtonBObserver = new();
            private readonly float[] _fingers = new float[5];

            public IObservable<ButtonState> TriggerObservable => TriggerObserver.OnStateChanged;
            public IObservable<ButtonState> GripObservable => GripObserver.OnStateChanged;
            public IObservable<ButtonState> AButtonObserver => ButtonAObserver.OnStateChanged;
            public IObservable<ButtonState> BButtonObserver => ButtonBObserver.OnStateChanged;

            public float this[int index]
            {
                get => _fingers[index];
                set => _fingers[index] = value;
            }
        }
    }

}