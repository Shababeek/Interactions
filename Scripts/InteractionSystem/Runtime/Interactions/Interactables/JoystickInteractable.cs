using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Utilities;
using UniRx;

namespace Shababeek.Interactions
{
    public enum JoystickProjectionMethod
    {
        DirectionProjection,
        PlaneIntersection
    }
    
    /// <summary>
    /// Joystick-style interactable that allows constrained rotation around X (pitch) and Z (yaw) axes.
    /// Uses plane projection for natural, intuitive joystick control with configurable angle limits.
    /// </summary>
    public class JoystickInteractable : ConstrainedInteractableBase
    {
        [Header("Joystick Settings")]
        [Tooltip("should be where the height at which the hand will hold the object")]
        [SerializeField] private float projectionPlaneHeight = 0.5f;
        
        [Tooltip("DirectionProjection: Hand height doesn't affect angle (arcade stick feel)\nPlaneIntersection: Hand height affects angle (realistic joystick feel)")]
        [SerializeField] private JoystickProjectionMethod projectionMethod = JoystickProjectionMethod.DirectionProjection;
        
        [Header("Rotation Limits")]
        [SerializeField] private Vector2 xRotationRange = new Vector2(-45f, 45f);
        [SerializeField] private Vector2 zRotationRange = new Vector2(-45f, 45f);
        [SerializeField] private Vector2UnityEvent onRotationChanged = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private Vector2 currentRotation = Vector2.zero;
        [ReadOnly, SerializeField] private Vector2 normalizedRotation = Vector2.zero;

        private Quaternion _originalRotation;
        private float _returnTimer;
        
        private const float MaxAngleLimit = 85f;


        /// <summary>
        /// Observable that fires when the joystick's rotation changes.
        /// </summary>
        public IObservable<Vector2> OnRotationChanged => onRotationChanged.AsObservable();
        
        public Vector2 CurrentRotation => currentRotation;
        
        public Vector2 NormalizedRotation => normalizedRotation;
        
        /// <summary>
        /// X-axis rotation range (pitch). x=min, y=max. Clamped to ±85°.
        /// </summary>
        public Vector2 XRotationRange
        {
            get => xRotationRange;
            set
            {
                xRotationRange = new Vector2(
                    Mathf.Clamp(value.x, -MaxAngleLimit, value.y - 1f),
                    Mathf.Clamp(value.y, value.x + 1f, MaxAngleLimit)
                );
            }
        }
        
        /// <summary>
        /// Z-axis rotation range (yaw). x=min, y=max. Clamped to ±85°.
        /// </summary>
        public Vector2 ZRotationRange
        {
            get => zRotationRange;
            set
            {
                zRotationRange = new Vector2(
                    Mathf.Clamp(value.x, -MaxAngleLimit, value.y - 1f),
                    Mathf.Clamp(value.y, value.x + 1f, MaxAngleLimit)
                );
            }
        }
        
        public float ProjectionPlaneHeight
        {
            get => projectionPlaneHeight;
            set => projectionPlaneHeight = Mathf.Max(0.01f, value);
        }
        
        public JoystickProjectionMethod ProjectionMethod
        {
            get => projectionMethod;
            set => projectionMethod = value;
        }
        
        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            UpdateCurrentRotationFromTransform();
        }
        
        protected override void HandleObjectMovement(Vector3 target)
        {
            if (!IsSelected || IsReturning) return;
            
            CalculateAndApplyRotation(target);
            UpdateDebugValues();
            InvokeEvents();
        }

        protected override void HandleObjectDeselection()
        {
            _returnTimer = 0f;
            //TODO: Snap to nearest step if stepped movement is implemented
        }

        private void CalculateAndApplyRotation(Vector3 handWorldPosition)
        {
            if (CurrentInteractor == null) return;
            var pivot = interactableObject.transform;

            var plane= GetProjectiuonPlane(pivot);
            var angle = CalculateAngle(handWorldPosition, pivot, plane.center,plane.normal);
            angle = ClampAngles(angle);
            currentRotation = angle;
            ApplyRotationToTransform();
        }

        private Vector2 ClampAngles(Vector2 angle)
        {
            angle.x = Mathf.Clamp(angle.x, xRotationRange.x, xRotationRange.y);
            angle.y = Mathf.Clamp(angle.y, zRotationRange.x, zRotationRange.y);
            return angle;
        }

        private Vector2 CalculateAngle(Vector3 handWorldPosition, Transform pivot, Vector3 planeCenter, Vector3 planeNormal)
        {
            Vector3 localOffset;
            
            if (projectionMethod == JoystickProjectionMethod.DirectionProjection)
            {
                localOffset = CalculateAngleDirectionProjection(handWorldPosition, pivot, planeCenter, planeNormal);
            }
            else
            {
                localOffset = CalculateAnglePlaneIntersection(handWorldPosition, pivot, planeCenter, planeNormal);
            }

            Vector2 angle;
            angle.x = CalculateAngleFromOffset(localOffset.z, projectionPlaneHeight);
            angle.y = CalculateAngleFromOffset(-localOffset.x, projectionPlaneHeight);
            return angle;
        }

        private Vector3 CalculateAngleDirectionProjection(Vector3 handWorldPosition, Transform pivot, Vector3 planeCenter, Vector3 planeNormal)
        {
            var direction = handWorldPosition - pivot.position;
            direction=transform.InverseTransformDirection(direction);
            return  Vector3.ProjectOnPlane(direction, planeNormal);
        }

        private Vector3 CalculateAnglePlaneIntersection(Vector3 handWorldPosition, Transform pivot, Vector3 planeCenter, Vector3 planeNormal)
        {
            // Find where hand would intersect the plane if moved perpendicular to it
            Vector3 handToPlaneCenter = handWorldPosition - planeCenter;
            float distanceToPlane = Vector3.Dot(handToPlaneCenter, planeNormal);
            Vector3 projectedHandPosition = handWorldPosition - planeNormal * distanceToPlane;
            
            // Get offset from plane center in local space
            Vector3 offsetFromCenter = projectedHandPosition - planeCenter;
            return pivot.InverseTransformDirection(offsetFromCenter);
        }

        private (Vector3 center, Vector3 normal) GetProjectiuonPlane(Transform pivot)
        {
            var planeCenter = pivot.position + transform.up * projectionPlaneHeight;
            var planeNormal = Vector3.up;
            return (planeCenter, planeNormal);
        }

        private float CalculateAngleFromOffset(float offset, float height)
        {
            return Mathf.Atan2(offset, height) * Mathf.Rad2Deg;
        }

        private void ApplyRotationToTransform()
        { 
            var xRot = Quaternion.AngleAxis(currentRotation.x, Vector3.right);
            var zRot = Quaternion.AngleAxis(currentRotation.y, Vector3.forward);
            interactableObject.transform.localRotation = _originalRotation * xRot * zRot;
        }

        private void UpdateCurrentRotationFromTransform()
        {
            Vector3 eulerAngles = interactableObject.transform.localRotation.eulerAngles;
            
            float x = NormalizeAngle(eulerAngles.x);
            float z = NormalizeAngle(eulerAngles.z);
            
            currentRotation = new Vector2(x, z);
        }

        private void UpdateDebugValues()
        {
            // Calculate normalized values (0-1)
            normalizedRotation = new Vector2(
                Mathf.InverseLerp(xRotationRange.x, xRotationRange.y, currentRotation.x),
                Mathf.InverseLerp(zRotationRange.x, zRotationRange.y, currentRotation.y)
            );
        }
        
        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += Time.deltaTime * returnSpeed;
            var localRotation = interactableObject.transform.localRotation;
            localRotation = Quaternion.Lerp(localRotation, _originalRotation, _returnTimer);
            interactableObject.transform.localRotation = localRotation;
            
            UpdateCurrentRotationFromTransform();
            UpdateDebugValues();
            InvokeEvents();
            
            if (Quaternion.Angle(interactableObject.transform.localRotation, _originalRotation) < 1f)
            {
                IsReturning = false;
                interactableObject.transform.localRotation = _originalRotation;
                UpdateCurrentRotationFromTransform();
            }
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
        
        private void InvokeEvents()
        {
            onRotationChanged?.Invoke(currentRotation);
        }


        private void SetRotation(float xAngle, float zAngle)
        {
            Vector2 clampedAngles = new Vector2(
                Mathf.Clamp(xAngle, xRotationRange.x, xRotationRange.y),
                Mathf.Clamp(zAngle, zRotationRange.x, zRotationRange.y)
            );
            
            currentRotation = clampedAngles;
            ApplyRotationToTransform();
            UpdateDebugValues();
            InvokeEvents();
        }


        public void SetNormalizedRotation(float normalizedX, float normalizedZ)
        {
            float xAngle = Mathf.Lerp(xRotationRange.x, xRotationRange.y, normalizedX);
            float zAngle = Mathf.Lerp(zRotationRange.x, zRotationRange.y, normalizedZ);
            
            SetRotation(xAngle, zAngle);
        }
        public void ResetToOriginal()
        {
            interactableObject.transform.localRotation = _originalRotation;
            UpdateCurrentRotationFromTransform();
            IsReturning = false;
            UpdateDebugValues();
            InvokeEvents();
        }

        private void OnValidate()
        {
            // Ensure min < max
            if (xRotationRange.x >= xRotationRange.y)
            {
                xRotationRange.y = xRotationRange.x + 1f;
            }
            
            if (zRotationRange.x >= zRotationRange.y)
            {
                zRotationRange.y = zRotationRange.x + 1f;
            }
            
            // Clamp to ±85°
            xRotationRange.x = Mathf.Clamp(xRotationRange.x, -MaxAngleLimit, MaxAngleLimit);
            xRotationRange.y = Mathf.Clamp(xRotationRange.y, -MaxAngleLimit, MaxAngleLimit);
            zRotationRange.x = Mathf.Clamp(zRotationRange.x, -MaxAngleLimit, MaxAngleLimit);
            zRotationRange.y = Mathf.Clamp(zRotationRange.y, -MaxAngleLimit, MaxAngleLimit);
            
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
            projectionPlaneHeight = Mathf.Max(0.01f, projectionPlaneHeight);
        }

 
        private void OnDrawGizmos()
        {
            if (interactableObject == null) return;
            
            DrawJoystickVisualization();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;
            
            DrawJoystickVisualization(true);
            DrawProjectionPlane();
            DrawRotationLimits();
        }
        

        private void DrawJoystickVisualization(bool selected = false)
        {
            var position = interactableObject.transform.position;
            
            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(position, 0.02f);
            
            if (Application.isPlaying)
            {
                var forward = interactableObject.transform.forward;
                Gizmos.color = selected ? Color.green : new Color(0f, 1f, 0f, 0.7f);
                Gizmos.DrawRay(position, forward * 0.3f);
            }
        }
        
        private void DrawProjectionPlane()
        {
            Transform pivot = interactableObject.transform;
            Vector3 planeCenter = pivot.position + pivot.up * projectionPlaneHeight;
            float planeSize = 0.3f;
            
            // Draw projection plane
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            DrawPlane(planeCenter, pivot.up, pivot.forward, pivot.right, planeSize);
            
            // Draw support line
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pivot.position, planeCenter);
        }
        
        private void DrawPlane(Vector3 center, Vector3 normal, Vector3 forward, Vector3 right, float size)
        {
            Vector3 topLeft = center + forward * size - right * size;
            Vector3 topRight = center + forward * size + right * size;
            Vector3 bottomLeft = center - forward * size - right * size;
            Vector3 bottomRight = center - forward * size + right * size;
            
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, bottomRight);
            Gizmos.DrawLine(topRight, bottomLeft);
        }
        
        private void DrawRotationLimits()
        {
            var position = interactableObject.transform.position;
            float radius = 0.5f;
            
            // Draw X rotation limits (pitch) - Red
            Gizmos.color = Color.red;
            var minRotX = Quaternion.AngleAxis(xRotationRange.x, transform.right);
            var maxRotX = Quaternion.AngleAxis(xRotationRange.y, transform.right);
            var minDirX = minRotX * transform.forward;
            var maxDirX = maxRotX * transform.forward;
            Gizmos.DrawRay(position, minDirX * radius);
            Gizmos.DrawRay(position, maxDirX * radius);
            
            // Draw Z rotation limits (yaw) - Blue
            Gizmos.color = Color.blue;
            var minRotZ = Quaternion.AngleAxis(zRotationRange.x, transform.forward);
            var maxRotZ = Quaternion.AngleAxis(zRotationRange.y, transform.forward);
            var minDirZ = minRotZ * transform.up;
            var maxDirZ = maxRotZ * transform.up;
            Gizmos.DrawRay(position, minDirZ * radius);
            Gizmos.DrawRay(position, maxDirZ * radius);
        }
    }
}