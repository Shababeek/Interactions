using Shababeek.Interactions.Core;
using Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Sets a Gesture scriptable object based on the pose the player is making.
    /// Automatically detects hand gestures by monitoring finger positions and updates
    /// the assigned GestureVariable in real-time.
    /// </summary>
    /// <remarks>
    /// This component reads finger positions from the Hand component and maps them
    /// to predefined gestures. It's useful for gesture-based interactions and
    /// can be used to trigger different behaviors based on hand poses.
    /// </remarks>
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

        /// <summary>
        /// Determines and sets the current gesture based on finger positions.
        /// </summary>
        /// <remarks>
        /// Gesture detection logic:
        /// - Thumb + Grip + Index = Fist
        /// - Thumb + Grip + No Index = Pointing
        /// - Thumb + No Grip + No Index = Three
        /// - No Thumb + Grip + Index = ThumbsUp
        /// - No Thumb + Grip + No Index = Pointing
        /// - No Thumb + No Grip + Index = None
        /// - No Thumb + No Grip + No Index = Relaxed
        /// </remarks>
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

        /// <summary>
        /// Reads the current status of the thumb, index, and middle fingers from the Hand component.
        /// </summary>
        /// <remarks>
        /// Fingers are considered "active" when their value is greater than 0.5.
        /// This threshold can be adjusted by modifying the comparison value.
        /// </remarks>
        private void ReadFingerStatus()
        {
            _thumb = _hand[FingerName.Thumb] > .5f;
            _index = _hand[FingerName.Index] > .5f;
            _grip = _hand[FingerName.Middle] > .5f;
        }
    }
}