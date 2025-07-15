using System;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public abstract class InputManagerBase : MonoBehaviour
    {
        protected readonly HandInputManagerImpl leftHand = new();
        protected readonly HandInputManagerImpl rightHand = new();

        public IHandInputManager this[HandIdentifier index]
        {
            get
            {
                return index switch
                {
                    HandIdentifier.Left => leftHand,
                    HandIdentifier.Right => rightHand,
                    _ => null
                };
            }
        }

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
            internal readonly ButtonObservable triggerObserver = new();
            internal readonly ButtonObservable gripObserver = new();
            internal readonly ButtonObservable aButtonObserver = new();
            internal readonly ButtonObservable bButtonObserver = new();
            internal readonly float[] fingers = new float[5];

            public IObservable<ButtonState> TriggerObservable => triggerObserver;
            public IObservable<ButtonState> GripObservable => gripObserver;
            public IObservable<ButtonState> AButtonObserver => aButtonObserver;
            public IObservable<ButtonState> BButtonObserver => bButtonObserver;

            public float this[int index]
            {
                get => fingers[index];
                set => fingers[index] = value;
            }
        }
    }

}