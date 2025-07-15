using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UniRx;
using UnityEngine.Events;

namespace Shababeek.Interactions
{


    [RequireComponent(typeof(Grabable))]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Shababeek/Interactions/Interactables/Throwable")]
    public class Throwable : MonoBehaviour
    {
        [Header("Throw Settings")]
        [SerializeField] private int velocitySampleCount = 10;
        [SerializeField] private float throwMultiplier = 1f;
        [SerializeField] private bool enableAngularVelocity = true;
        [SerializeField] private float angularVelocityMultiplier = 1f;
        
        [Header("Events")]
        [SerializeField] private Vector3UnityEvent onThrowEnd;

        [Header("Debug")]
        [ReadOnly] [SerializeField] private bool isBeingThrown = false;
        [ReadOnly] [SerializeField] private Vector3 currentVelocity = Vector3.zero;
        [ReadOnly] [SerializeField] private Vector3 lastThrowVelocity = Vector3.zero;

        private Grabable _grabable;
        private Rigidbody _body;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3[] _velocitySamples;
        private Vector3[] _angularVelocitySamples;
        private int _currentSampleIndex = 0;
        private int _sampleCount = 0;
        private float _fixedDeltaTimeInverse;

        public IObservable<Vector3> OnThrowEnd => onThrowEnd.AsObservable();

        private void Awake()
        {
            _grabable = GetComponent<Grabable>();
            _body = GetComponent<Rigidbody>();
            
            // Initialize velocity tracking arrays
            _velocitySamples = new Vector3[velocitySampleCount];
            _angularVelocitySamples = new Vector3[velocitySampleCount];
            _fixedDeltaTimeInverse = 1f / Time.fixedDeltaTime;

            // Subscribe to grab events
            _grabable.OnSelected
                .Do(_ => StartThrowing())
                .Subscribe().AddTo(this);

            _grabable.OnDeselected
                .Do(_ => EndThrowing())
                .Subscribe().AddTo(this);
        }

        private void StartThrowing()
        {
            isBeingThrown = true;
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
            
            // Reset velocity tracking
            Array.Clear(_velocitySamples, 0, _velocitySamples.Length);
            Array.Clear(_angularVelocitySamples, 0, _angularVelocitySamples.Length);
            _currentSampleIndex = 0;
            _sampleCount = 0;
            
            // Make rigidbody kinematic while being held
            _body.isKinematic = true;
        }

        private void EndThrowing()
        {
            if (!isBeingThrown) return;
            
            isBeingThrown = false;
            
            // Calculate average velocity
            Vector3 averageVelocity = Vector3.zero;
            Vector3 averageAngularVelocity = Vector3.zero;
            
            int samplesToUse = Mathf.Min(_sampleCount, velocitySampleCount);
            
            for (int i = 0; i < samplesToUse; i++)
            {
                averageVelocity += _velocitySamples[i];
                averageAngularVelocity += _angularVelocitySamples[i];
            }
            
            if (samplesToUse > 0)
            {
                averageVelocity /= samplesToUse;
                averageAngularVelocity /= samplesToUse;
                
                // Apply multipliers
                lastThrowVelocity = averageVelocity * _fixedDeltaTimeInverse * throwMultiplier;
                Vector3 finalAngularVelocity = averageAngularVelocity * _fixedDeltaTimeInverse * angularVelocityMultiplier;
                
                // Apply velocities to rigidbody
                _body.isKinematic = false;
                _body.linearVelocity = lastThrowVelocity;
                
                if (enableAngularVelocity)
                {
                    _body.angularVelocity = finalAngularVelocity;
                }
                
                currentVelocity = lastThrowVelocity;
                onThrowEnd?.Invoke(lastThrowVelocity);
            }
        }

        private void FixedUpdate()
        {
            if (!isBeingThrown) return;
            
            // Sample linear velocity
            Vector3 currentPosition = transform.position;
            _velocitySamples[_currentSampleIndex] = currentPosition - _lastPosition;
            _lastPosition = currentPosition;
            
            // Sample angular velocity
            Quaternion currentRotation = transform.rotation;
            Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation);
            _angularVelocitySamples[_currentSampleIndex] = GetAngularVelocityFromDeltaRotation(deltaRotation, Time.fixedDeltaTime);
            _lastRotation = currentRotation;
            
            // Update sample index
            _currentSampleIndex = (_currentSampleIndex + 1) % velocitySampleCount;
            _sampleCount++;
        }

        private Vector3 GetAngularVelocityFromDeltaRotation(Quaternion deltaRotation, float deltaTime)
        {
            float angle;
            Vector3 axis;
            deltaRotation.ToAngleAxis(out angle, out axis);
            
            if (angle > 180f)
                angle -= 360f;
            
            return axis * (angle * Mathf.Deg2Rad / deltaTime);
        }

        private void OnValidate()
        {
            velocitySampleCount = Mathf.Max(1, velocitySampleCount);
            throwMultiplier = Mathf.Max(0.1f, throwMultiplier);
            angularVelocityMultiplier = Mathf.Max(0f, angularVelocityMultiplier);
        }
    }
}