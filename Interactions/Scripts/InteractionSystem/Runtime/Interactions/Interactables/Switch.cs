using Shababeek.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Enum representing an axis in 3D space.
    /// </summary>
    public enum Axis
    {
        X,
        Y,
        Z
    }
    
    enum Direction
    {
        Up=1,
        Down=-1,
        None=0
    }
    
    /// <summary>
    /// Enum representing the starting position of the switch.
    /// </summary>
    public enum StartingPosition
    {
        Off,
        Neutral,
        On
    }
    
    /// <summary>
    /// Physical switch component that responds to trigger interactions.
    /// Rotates the switch body based on interaction direction and raises events.
    /// Configurable to work with any rotation axis and detection direction.
    /// </summary>
    /// <remarks>
    /// This component creates a physical switch that can be activated by trigger interactions.
    /// It automatically rotates the switch body and raises events based on the interaction direction.
    /// The rotation axis and detection direction can be configured to work with switches oriented in any direction.
    /// </remarks>
    public class Switch : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Event raised when the switch is moved to the up position.")]
        [SerializeField] private UnityEvent onUp;
        
        [Tooltip("Event raised when the switch is moved to the down position.")]
        [SerializeField] private UnityEvent onDown;
        
        [Tooltip("Event raised when the switch is held in a position.")]
        [SerializeField] private UnityEvent onHold;
        
        [Header("Switch Configuration")]
        [Tooltip("The transform of the switch body that rotates during interaction.")]
        [SerializeField] private Transform switchBody;
        
        [Tooltip("The axis around which the switch rotates.")]
        [SerializeField] private Axis rotationAxis = Axis.Z;
        
        [Tooltip("The axis used to detect interaction direction.")]
        [SerializeField] private Axis detectionAxis = Axis.X;
        
        [Tooltip("Rotation angle in degrees for the up position.")]
        [SerializeField] private float upRotation = 20;
        
        [Tooltip("Rotation angle in degrees for the down position.")]
        [SerializeField] private float downRotation = -20;
        
        [Tooltip("Speed of the rotation animation.")]
        [SerializeField] private float rotateSpeed = 10;
        
        [Tooltip("Angle threshold for direction detection.")]
        [SerializeField] private float angleThreshold = 5f;
        
        [Tooltip("When enabled, the switch will stay in its current position instead of returning to neutral when the trigger exits.")]
        [SerializeField] private bool stayInPosition = false;
        
        [Tooltip("The starting position of the switch when the scene starts.")]
        [SerializeField] private StartingPosition startingPosition = StartingPosition.Neutral;
        
        [Header("Debug")]
        [Tooltip("Current direction of the switch.")]
        [ReadOnly][SerializeField] private Direction direction;

        private float t = 0;
        private float targetRotation=0;
        private Collider activeCollider ;

        public Transform SwitchBody
        {
            get => switchBody;
            set => switchBody = value;
        }

        /// <summary>
        /// Gets or sets whether the switch should stay in position instead of returning to neutral.
        /// </summary>
        public bool StayInPosition
        {
            get => stayInPosition;
            set => stayInPosition = value;
        }
        
        /// <summary>
        /// Gets or sets the starting position of the switch.
        /// </summary>
        public StartingPosition StartingPosition
        {
            get => startingPosition;
            set => startingPosition = value;
        }

        private void Update()
        {
            ChooseDirection();
            Rotate();
        }
        
        private void Start()
        {
            SetStartingPosition();
        }
        
        private void SetStartingPosition()
        {
            if (switchBody == null) return;
            
            switch (startingPosition)
            {
                case StartingPosition.On:
                    direction = Direction.Up;
                    targetRotation = upRotation;
                    break;
                case StartingPosition.Off:
                    direction = Direction.Down;
                    targetRotation = downRotation;
                    break;
                case StartingPosition.Neutral:
                default:
                    direction = Direction.None;
                    targetRotation = (upRotation + downRotation) / 2f; // Calculate middle position
                    break;
            }
            
            // Apply the starting rotation immediately
            t = 1f; // Set to 1 to skip animation
            Rotate();
        }

        private void ChooseDirection()
        {
            if(!activeCollider) return;
            
            var dir = activeCollider.transform.position - transform.position;
            var detectionVector = GetDetectionVector();
            var rotationAxisVector = GetRotationAxisVector();
            
            var angle = Vector3.SignedAngle(detectionVector, dir, rotationAxisVector);
            
            // Switch should rotate away from the collider, so we invert the logic
            switch (angle)
            {
                case > 0 when Mathf.Abs(angle) > angleThreshold && direction != Direction.Up:
                    direction = Direction.Up;
                    t = 0;
                    targetRotation = upRotation;
                    onUp.Invoke();
                    break;
                case < 0 when Mathf.Abs(angle) > angleThreshold && direction != Direction.Down:
                    direction = Direction.Down;
                    t = 0;
                    targetRotation = downRotation;
                    onDown.Invoke();
                    break;
            }
        }
        
        /// <summary>
        /// Gets the detection vector based on the configured detection axis.
        /// </summary>
        /// <returns>The world space vector for direction detection.</returns>
        private Vector3 GetDetectionVector()
        {
            return detectionAxis switch
            {
                Axis.X => transform.right,
                Axis.Y => transform.up,
                Axis.Z => transform.forward,
                _ => transform.right
            };
        }
        
        /// <summary>
        /// Gets the rotation axis vector based on the configured rotation axis.
        /// </summary>
        /// <returns>The world space vector for the rotation axis.</returns>
        private Vector3 GetRotationAxisVector()
        {
            return rotationAxis switch
            {
                Axis.X => transform.right,
                Axis.Y => transform.up,
                Axis.Z => transform.forward,
                _ => transform.forward
            };
        }

        private void Rotate()
        {
            t += Time.deltaTime * rotateSpeed;
            t = Mathf.Clamp01(t);
            
            // Get current rotation and apply target rotation based on the configured axis
            var currentRotation = switchBody.localRotation.eulerAngles;
            var newRotation = rotationAxis switch
            {
                Axis.X => new Vector3(targetRotation, currentRotation.y, currentRotation.z),
                Axis.Y => new Vector3(currentRotation.x, targetRotation, currentRotation.z),
                Axis.Z => new Vector3(currentRotation.x, currentRotation.y, targetRotation),
                _ => new Vector3(currentRotation.x, currentRotation.y, targetRotation)
            };
            
            switchBody.localRotation = Quaternion.Euler(newRotation);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.isTrigger) return;
            activeCollider = other;
            t = 0;
        }
        private void OnTriggerExit(Collider other)
        {
            if(other!=activeCollider)return;
            direction = Direction.None;
            activeCollider =null;
            
            // Only reset to neutral position if stayInPosition is false
            if (!stayInPosition)
            {
                targetRotation = 0;
                t = 0;
            }
        }
        
        /// <summary>
        /// Called when the object is selected in the editor to validate configuration.
        /// </summary>
        private void OnValidate()
        {
            if (switchBody == null)
            {
                switchBody = transform;
            }
            
            // Ensure angle threshold is positive
            if (angleThreshold < 0)
            {
                angleThreshold = Mathf.Abs(angleThreshold);
            }
        }
        
        /// <summary>
        /// Resets the switch to its neutral position.
        /// Note: This method will only reset to neutral if stayInPosition is false.
        /// </summary>
        public void ResetSwitch()
        {
            direction = Direction.None;
            activeCollider = null;
            
            // Only reset to neutral position if stayInPosition is false
            if (!stayInPosition)
            {
                targetRotation = (upRotation + downRotation) / 2f; // Calculate middle position
                t = 0;
                
                // Reset rotation to neutral position
                var currentRotation = switchBody.localRotation.eulerAngles;
                var neutralRotation = rotationAxis switch
                {
                    Axis.X => new Vector3(targetRotation, currentRotation.y, currentRotation.z),
                    Axis.Y => new Vector3(currentRotation.x, targetRotation, currentRotation.z),
                    Axis.Z => new Vector3(currentRotation.x, currentRotation.y, targetRotation),
                    _ => new Vector3(currentRotation.x, currentRotation.y, targetRotation)
                };
                
                switchBody.localRotation = Quaternion.Euler(neutralRotation);
            }
        }
        
        /// <summary>
        /// Forces the switch to reset to neutral position regardless of the stayInPosition setting.
        /// </summary>
        public void ForceResetSwitch()
        {
            direction = Direction.None;
            activeCollider = null;
            targetRotation = (upRotation + downRotation) / 2f; // Calculate middle position
            t = 0;
            
            // Reset rotation to neutral position
            var currentRotation = switchBody.localRotation.eulerAngles;
            var neutralRotation = rotationAxis switch
            {
                Axis.X => new Vector3(targetRotation, currentRotation.y, currentRotation.z),
                Axis.Y => new Vector3(currentRotation.x, targetRotation, currentRotation.z),
                Axis.Z => new Vector3(currentRotation.x, currentRotation.y, targetRotation),
                _ => new Vector3(currentRotation.x, currentRotation.y, targetRotation)
            };
            
            switchBody.localRotation = Quaternion.Euler(neutralRotation);
        }
        
        /// <summary>
        /// Gets the current switch state.
        /// </summary>
        /// <returns>True if switch is in up position, false if down, null if neutral.</returns>
        public bool? GetSwitchState()
        {
            return direction switch
            {
                Direction.Up => true,
                Direction.Down => false,
                Direction.None => null,
                _ => null
            };
        }
        
        /// <summary>
        /// Gets the current rotation of the switch body.
        /// </summary>
        /// <returns>The current local rotation as a Vector3.</returns>
        public Vector3 GetCurrentRotation()
        {
            return switchBody != null ? switchBody.localRotation.eulerAngles : Vector3.zero;
        }
        
        /// <summary>
        /// Sets the switch to a specific position.
        /// </summary>
        /// <param name="position">The position to set the switch to.</param>
        public void SetPosition(StartingPosition position)
        {
            if (switchBody == null) return;
            
            switch (position)
            {
                case StartingPosition.On:
                    direction = Direction.Up;
                    targetRotation = upRotation;
                    break;
                case StartingPosition.Off:
                    direction = Direction.Down;
                    targetRotation = downRotation;
                    break;
                case StartingPosition.Neutral:
                default:
                    direction = Direction.None;
                    targetRotation = (upRotation + downRotation) / 2f; // Calculate middle position
                    break;
            }
            
            // Apply the position immediately
            t = 1f;
            Rotate();
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Draws gizmos in the scene view to visualize switch configuration.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (switchBody == null) return;
            
            DrawSwitchVisualization();
        }
        
        /// <summary>
        /// Draws selected gizmos with more detail when the object is selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (switchBody == null) return;
            
            DrawSwitchVisualization(true);
            DrawDetectionArea();
        }
        
        /// <summary>
        /// Draws the switch visualization gizmos.
        /// </summary>
        /// <param name="selected">Whether the object is selected (for more detailed visualization).</param>
        private void DrawSwitchVisualization(bool selected = false)
        {
            var detectionVector = GetDetectionVector();
            var rotationAxisVector = GetRotationAxisVector();
            var position = switchBody.position;
            
            // Draw rotation axis
            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawRay(position, rotationAxisVector * 0.5f);
            Gizmos.DrawRay(position, -rotationAxisVector * 0.5f);
            
            // Draw detection direction
            Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawRay(position, detectionVector * 0.3f);
            
            // Draw rotation range
            if (selected)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                DrawRotationArc(position, rotationAxisVector, detectionVector, upRotation);
                
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                DrawRotationArc(position, rotationAxisVector, detectionVector, downRotation);
            }
        }
        
        /// <summary>
        /// Draws the detection area for the switch.
        /// </summary>
        private void DrawDetectionArea()
        {
            var collider = GetComponent<Collider>();
            if (collider == null) return;
            
            // Draw detection threshold
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            var bounds = collider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            // Draw angle threshold visualization
            var detectionVector = GetDetectionVector();
            var rotationAxisVector = GetRotationAxisVector();
            var position = switchBody.position;
            
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            DrawAngleThreshold(position, detectionVector, rotationAxisVector, angleThreshold);
        }
        
        /// <summary>
        /// Draws a rotation arc to visualize the switch movement range.
        /// </summary>
        private void DrawRotationArc(Vector3 center, Vector3 axis, Vector3 from, float angle)
        {
            var rotation = Quaternion.AngleAxis(angle, axis);
            var to = rotation * from;
            
            // Draw arc
            var steps = 10;
            var stepAngle = angle / steps;
            var currentVector = from;
            
            for (int i = 0; i < steps; i++)
            {
                var nextRotation = Quaternion.AngleAxis(stepAngle, axis);
                var nextVector = nextRotation * currentVector;
                
                Gizmos.DrawLine(center + currentVector * 0.2f, center + nextVector * 0.2f);
                currentVector = nextVector;
            }
            
            // Draw end position
            Gizmos.DrawRay(center, to * 0.2f);
        }
        
        /// <summary>
        /// Draws the angle threshold visualization.
        /// </summary>
        private void DrawAngleThreshold(Vector3 center, Vector3 detectionVector, Vector3 rotationAxis, float threshold)
        {
            var thresholdRotation1 = Quaternion.AngleAxis(threshold, rotationAxis);
            var thresholdRotation2 = Quaternion.AngleAxis(-threshold, rotationAxis);
            
            var threshold1 = thresholdRotation1 * detectionVector;
            var threshold2 = thresholdRotation2 * detectionVector;
            
            Gizmos.DrawRay(center, threshold1 * 0.4f);
            Gizmos.DrawRay(center, threshold2 * 0.4f);
        }
        #endif
    }
}