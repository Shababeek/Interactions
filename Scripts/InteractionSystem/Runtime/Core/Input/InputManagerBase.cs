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

        public interface IHandInputManager
        {
            public IObservable<VRButtonState> TriggerObservable { get; }
            public IObservable<VRButtonState> GripObservable { get; }
            public IObservable<VRButtonState> AButtonObserver { get; }
            public IObservable<VRButtonState> BButtonObserver { get; }
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