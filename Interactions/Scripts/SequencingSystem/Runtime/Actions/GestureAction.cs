using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Sequencing
{
    [AddComponentMenu(menuName :"Shababeek/Sequencing/Actions/GestureAction")]
    public class GestureAction : AbstractSequenceAction
    {
        [System.Serializable]
        public class GestureDefinition
        {
            public enum GestureType
            {
                Fist,
                OpenHand,
                Pointing,
                ThumbsUp,
                ThumbsDown,
                Victory,
                Custom
            }

            public GestureType gestureType;
            public HandIdentifier targetHand = HandIdentifier.Right;
            public float holdDuration = 0.5f;
            public bool requireHold = false;
            public string customGestureName;
            public float tolerance = 0.1f;
        }

        [SerializeField] private List<GestureDefinition> gestures = new List<GestureDefinition>();
        [SerializeField] private bool requireAllGestures = false;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private bool continuousCheck = true;

        private StepEventListener listener;
        private Config config;
        private Dictionary<GestureDefinition, float> gestureHoldTimes = new Dictionary<GestureDefinition, float>();
        private bool gestureDetected = false;

        private void Awake()
        {
            listener = GetComponent<StepEventListener>();
            config = FindAnyObjectByType<CameraRig>()?.Config;
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                gestureDetected = false;
                gestureHoldTimes.Clear();
                
                if (continuousCheck)
                {
                    // Check gestures continuously
                    Observable.Interval(System.TimeSpan.FromSeconds(checkInterval))
                        .Where(_ => Started && !gestureDetected)
                        .Do(_ => CheckGestures())
                        .Subscribe()
                        .AddTo(Disposable);
                }
                else
                {
                    // Check once
                    CheckGestures();
                }
            }
        }

        private void CheckGestures()
        {
            if (!config ) return;

            bool allGesturesDetected = true;
            bool anyGestureDetected = false;

            foreach (var gesture in gestures)
            {
                bool isDetected = IsGestureDetected(gesture);
                
                if (isDetected)
                {
                    anyGestureDetected = true;
                    
                    if (gesture.requireHold)
                    {
                        if (!gestureHoldTimes.ContainsKey(gesture))
                        {
                            gestureHoldTimes[gesture] = 0f;
                        }
                        
                        gestureHoldTimes[gesture] += checkInterval;
                        
                        if (gestureHoldTimes[gesture] >= gesture.holdDuration)
                        {
                            // Gesture held long enough
                        }
                        else
                        {
                            allGesturesDetected = false;
                        }
                    }
                }
                else
                {
                    allGesturesDetected = false;
                    if (gestureHoldTimes.ContainsKey(gesture))
                    {
                        gestureHoldTimes.Remove(gesture);
                    }
                }
            }

            // Determine if sequence should complete
            bool shouldComplete = requireAllGestures ? allGesturesDetected : anyGestureDetected;
            
            if (shouldComplete && !gestureDetected)
            {
                gestureDetected = true;
                listener.OnActionCompleted();
            }
        }

        private bool IsGestureDetected(GestureDefinition gesture)
        {
            if (config == null) return false;

            var handManager = config.InputManager[gesture.targetHand];
            if (handManager == null) return false;

            switch (gesture.gestureType)
            {
                case GestureDefinition.GestureType.Fist:
                    return IsFistGesture(handManager, gesture.tolerance);
                
                case GestureDefinition.GestureType.OpenHand:
                    return IsOpenHandGesture(handManager, gesture.tolerance);
                
                case GestureDefinition.GestureType.Pointing:
                    return IsPointingGesture(handManager, gesture.tolerance);
                
                case GestureDefinition.GestureType.ThumbsUp:
                    return IsThumbsUpGesture(handManager, gesture.tolerance);
                
                case GestureDefinition.GestureType.ThumbsDown:
                    return IsThumbsDownGesture(handManager, gesture.tolerance);
                
                case GestureDefinition.GestureType.Victory:
                    return IsVictoryGesture(handManager, gesture.tolerance);
                
                case GestureDefinition.GestureType.Custom:
                    return IsCustomGesture(handManager, gesture.customGestureName, gesture.tolerance);
                
                default:
                    return false;
            }
        }

        private bool IsFistGesture(InputManagerBase.IHandInputManager handManager, float tolerance)
        {
            // Check if all fingers are closed (fist)
            var fingerValues = GetFingerValues(handManager);
            
            return fingerValues.thumb < tolerance &&
                   fingerValues.index < tolerance &&
                   fingerValues.middle < tolerance &&
                   fingerValues.ring < tolerance &&
                   fingerValues.pinky < tolerance;
        }

        private bool IsOpenHandGesture(InputManagerBase.IHandInputManager handManager, float tolerance)
        {
            // Check if all fingers are open
            var fingerValues = GetFingerValues(handManager);
            
            return fingerValues.thumb > (1f - tolerance) &&
                   fingerValues.index > (1f - tolerance) &&
                   fingerValues.middle > (1f - tolerance) &&
                   fingerValues.ring > (1f - tolerance) &&
                   fingerValues.pinky > (1f - tolerance);
        }

        private bool IsPointingGesture(InputManagerBase.IHandInputManager handManager, float tolerance)
        {
            // Check if index finger is extended, others closed
            var fingerValues = GetFingerValues(handManager);
            
            return fingerValues.thumb < tolerance &&
                   fingerValues.index > (1f - tolerance) &&
                   fingerValues.middle < tolerance &&
                   fingerValues.ring < tolerance &&
                   fingerValues.pinky < tolerance;
        }

        private bool IsThumbsUpGesture(InputManagerBase.IHandInputManager handManager, float tolerance)
        {
            // Check if thumb is extended, others closed
            var fingerValues = GetFingerValues(handManager);
            
            return fingerValues.thumb > (1f - tolerance) &&
                   fingerValues.index < tolerance &&
                   fingerValues.middle < tolerance &&
                   fingerValues.ring < tolerance &&
                   fingerValues.pinky < tolerance;
        }

        private bool IsThumbsDownGesture(InputManagerBase.IHandInputManager handManager, float tolerance)
        {
            // Similar to thumbs up but with different thumb orientation
            // This would need more sophisticated hand pose detection
            return IsThumbsUpGesture(handManager, tolerance); // Simplified for now
        }

        private bool IsVictoryGesture(InputManagerBase.IHandInputManager handManager, float tolerance)
        {
            // Check if index and middle fingers are extended, others closed
            var fingerValues = GetFingerValues(handManager);
            
            return fingerValues.thumb < tolerance &&
                   fingerValues.index > (1f - tolerance) &&
                   fingerValues.middle > (1f - tolerance) &&
                   fingerValues.ring < tolerance &&
                   fingerValues.pinky < tolerance;
        }

        private bool IsCustomGesture(InputManagerBase.IHandInputManager handManager, string gestureName, float tolerance)
        {
            // Custom gesture detection - can be extended with specific logic
            // For now, return false as placeholder
            return false;
        }

        private (float thumb, float index, float middle, float ring, float pinky) GetFingerValues(InputManagerBase.IHandInputManager handManager)
        {
            // This is a simplified implementation
            // In a real implementation, you'd get actual finger values from the hand tracking system
            return (
                thumb: Random.Range(0f, 1f),   // Placeholder
                index: Random.Range(0f, 1f),   // Placeholder
                middle: Random.Range(0f, 1f),  // Placeholder
                ring: Random.Range(0f, 1f),    // Placeholder
                pinky: Random.Range(0f, 1f)    // Placeholder
            );
        }

        // Public methods for external control
        public void AddGesture(GestureDefinition gesture)
        {
            gestures.Add(gesture);
        }

        public void RemoveGesture(int index)
        {
            if (index >= 0 && index < gestures.Count)
            {
                gestures.RemoveAt(index);
            }
        }

        public void ClearGestures()
        {
            gestures.Clear();
        }

        public void TriggerCustomGesture(string gestureName)
        {
            // External method to trigger custom gestures
            var gesture = gestures.Find(g => 
                g.gestureType == GestureDefinition.GestureType.Custom && 
                g.customGestureName == gestureName);

            if (gesture != null && !gestureDetected)
            {
                gestureDetected = true;
                listener.OnActionCompleted();
            }
        }

        // Helper methods to create gestures
        public static GestureDefinition CreateFistGesture(HandIdentifier hand = HandIdentifier.Right, float holdDuration = 0.5f)
        {
            return new GestureDefinition
            {
                gestureType = GestureDefinition.GestureType.Fist,
                targetHand = hand,
                holdDuration = holdDuration,
                requireHold = holdDuration > 0
            };
        }

        public static GestureDefinition CreatePointingGesture(HandIdentifier hand = HandIdentifier.Right, float holdDuration = 0.5f)
        {
            return new GestureDefinition
            {
                gestureType = GestureDefinition.GestureType.Pointing,
                targetHand = hand,
                holdDuration = holdDuration,
                requireHold = holdDuration > 0
            };
        }

        public static GestureDefinition CreateCustomGesture(string gestureName, HandIdentifier hand = HandIdentifier.Right)
        {
            return new GestureDefinition
            {
                gestureType = GestureDefinition.GestureType.Custom,
                targetHand = hand,
                customGestureName = gestureName
            };
        }
    }
} 