using System;
using Kandooz.InteractionSystem.Core;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    [RequireComponent(typeof(Hand))]
    public class GestureSetter : MonoBehaviour
    {
        [SerializeField] private GestureVariable gestureVariable;
        private Hand hand;
        private bool thumb;
        private bool index;
        private bool grip;

        private void Awake()
        {
            hand = GetComponent<Hand>();
        }

        private void Update()
        {
            ReadFingerStatus();
            SetGesture();
        }

        private void SetGesture()
        {
            if (thumb)
            {
                if (grip)
                {
                    gestureVariable.value = index ? Gesture.Fist : Gesture.Pointing;
                }
                else if (!index)
                {
                    gestureVariable.value = Gesture.Three;
                }
            }
            else if (grip)
            {
                gestureVariable.value = index ? Gesture.ThumbsUp : Gesture.Pointing;
            }
            else
            {
                gestureVariable.value = index ? Gesture.None : Gesture.relaxed;
            }
        }

        private void ReadFingerStatus()
        {
            thumb = hand[FingerName.Thumb] > .5f;
            index = hand[FingerName.Index] > .5f;
            grip = hand[FingerName.Middle] > .5f;
        }
    }
}