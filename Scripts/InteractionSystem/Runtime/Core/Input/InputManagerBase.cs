using System;
using UnityEngine;
using UniRx;
namespace Shababeek.Interactions.Core
{
    /// <summary>Base class for managing hand input from various sources.</summary>
    public abstract class InputManagerBase : MonoBehaviour
    {
        /// <summary>Left hand input implementation.</summary>
        protected readonly HandInputManagerImpl LeftHand = new();
        /// <summary>Right hand input implementation.</summary>
        protected readonly HandInputManagerImpl RightHand = new();

        /// <summary>Gets hand input manager by hand identifier.</summary>
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

        /// <summary>Interface for managing input for a single hand.</summary>
        public interface IHandInputManager
        {
            /// <summary>Observable for trigger button state changes.</summary>
            public IObservable<VRButtonState> TriggerObservable { get; }
            /// <summary>Observable for grip button state changes.</summary>
            public IObservable<VRButtonState> GripObservable { get; }
            /// <summary>Observable for A button state changes.</summary>
            public IObservable<VRButtonState> AButtonObserver { get; }
            /// <summary>Observable for B button state changes.</summary>
            public IObservable<VRButtonState> BButtonObserver { get; }
            /// <summary>Gets finger curl value by index.</summary>
            public float this[int index] { get; }
        }

        protected class HandInputManagerImpl : IHandInputManager
        {
            internal readonly ButtonObservable TriggerObserver = new();
            internal readonly ButtonObservable GripObserver = new();
            internal readonly ButtonObservable ButtonAObserver = new();
            internal readonly ButtonObservable ButtonBObserver = new();
            private readonly float[] _fingers = new float[5];

            public IObservable<VRButtonState> TriggerObservable => TriggerObserver.OnStateChanged;
            public IObservable<VRButtonState> GripObservable => GripObserver.OnStateChanged;
            public IObservable<VRButtonState> AButtonObserver => ButtonAObserver.OnStateChanged;
            public IObservable<VRButtonState> BButtonObserver => ButtonBObserver.OnStateChanged;

            public float this[int index]
            {
                get => _fingers[index];
                set => _fingers[index] = value;
            }
        }
    }

}