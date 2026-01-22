using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Types of proximity conditions that can trigger step completion.
    /// </summary>
    public enum ProximityCondition
    {
        /// <summary>Triggered when the target enters the proximity range.</summary>
        Enter = 0,
        /// <summary>Triggered when the target exits the proximity range.</summary>
        Exit = 1,
        /// <summary>Triggered when the target stays within proximity for a duration.</summary>
        StayDuration = 2,
    }

    /// <summary>
    /// Completes a step based on distance between two transforms.
    /// Useful for detecting when the player or objects approach specific locations.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/ProximityAction")]
    public class ProximityAction : AbstractSequenceAction
    {
        [Tooltip("The transform to monitor (e.g., player, hand, object).")]
        [SerializeField] private Transform target;

        [Tooltip("The reference point to measure distance from. If not set, uses this transform.")]
        [SerializeField] private Transform referencePoint;

        [Tooltip("The distance threshold for proximity detection.")]
        [SerializeField] private float proximityDistance = 1f;

        [Tooltip("The condition that triggers step completion.")]
        [SerializeField] private ProximityCondition condition = ProximityCondition.Enter;

        [Tooltip("Duration the target must stay within proximity (only used with StayDuration condition).")]
        [SerializeField] private float requiredStayDuration = 1f;

        private bool _wasInRange = false;
        private float _timeInRange = 0f;

        private void Awake()
        {
            if (referencePoint == null)
                referencePoint = transform;
        }

        private void Update()
        {
            if (!Started || target == null || referencePoint == null) return;

            float distance = Vector3.Distance(target.position, referencePoint.position);
            bool isInRange = distance <= proximityDistance;

            switch (condition)
            {
                case ProximityCondition.Enter:
                    if (isInRange && !_wasInRange)
                    {
                        CompleteStep();
                    }
                    break;

                case ProximityCondition.Exit:
                    if (!isInRange && _wasInRange)
                    {
                        CompleteStep();
                    }
                    break;

                case ProximityCondition.StayDuration:
                    if (isInRange)
                    {
                        _timeInRange += Time.deltaTime;
                        if (_timeInRange >= requiredStayDuration)
                        {
                            CompleteStep();
                        }
                    }
                    else
                    {
                        _timeInRange = 0f;
                    }
                    break;
            }

            _wasInRange = isInRange;
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _timeInRange = 0f;
                // Initialize _wasInRange based on current state
                if (target != null && referencePoint != null)
                {
                    _wasInRange = Vector3.Distance(target.position, referencePoint.position) <= proximityDistance;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var center = referencePoint != null ? referencePoint.position : transform.position;
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(center, proximityDistance);
        }
    }
}
