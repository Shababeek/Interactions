using Shababeek.Interactions.Core;
using Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Detects hand gestures and updates a GestureVariable.
    /// </summary>
    [RequireComponent(typeof(Hand))]
    [AddComponentMenu("Shababeek/Interactions/Interactors/Gesture Setter")]
    public class GestureSetter : MonoBehaviour
    {
        [Header("Gesture Configuration")]
        [Tooltip("The GestureVariable ScriptableObject that will be updated with the detected gesture.")]
        [SerializeField] private GestureVariable gestureVariable;
        
        private Hand _hand;
        private bool _thumb;
        private bool _index;
        private bool _grip;

        private void Awake()
        {
            _hand = GetComponent<Hand>();
        }

        private void Update()
        {
            ReadFingerStatus();
            SetGesture();
        }

        private void SetGesture()
        {
            if (_thumb)
            {
                if (_grip)
                {
                    gestureVariable.value = _index ? Gesture.Fist : Gesture.Pointing;
                }
                else if (!_index)
                {
                    gestureVariable.value = Gesture.Three;
                }
            }
            else if (_grip)
            {
                gestureVariable.value = _index ? Gesture.ThumbsUp : Gesture.Pointing;
            }
            else
            {
                gestureVariable.value = _index ? Gesture.None : Gesture.Relaxed;
            }
        }

        private void ReadFingerStatus()
        {
            _thumb = _hand[FingerName.Thumb] > .5f;
            _index = _hand[FingerName.Index] > .5f;
            _grip = _hand[FingerName.Middle] > .5f;
        }
    }
}