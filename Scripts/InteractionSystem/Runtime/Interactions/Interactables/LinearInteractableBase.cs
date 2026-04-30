using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for linear interactables (drawers, sliders) that move between two local-space points.
    /// Handles projection of hand input onto the rail and dispatches normalized position updates to
    /// subclass-specific clamp/snap/step logic.
    /// </summary>
    public abstract class LinearInteractableBase : ConstrainedInteractableBase
    {
        [Tooltip("Local-space position when the handle is at the start of the rail (normalized = 0).")]
        [SerializeField] protected Vector3 localStart = Vector3.zero;

        [Tooltip("Local-space position when the handle is at the end of the rail (normalized = 1).")]
        [SerializeField] protected Vector3 localEnd = Vector3.forward;

        [Header("Debug")]
        [ReadOnly, SerializeField] protected float currentNormalized;

        protected Vector3 _originalPosition;

        /// <summary>Local-space start of the rail (handle position when normalized = 0).</summary>
        public Vector3 LocalStart
        {
            get => localStart;
            set => localStart = value;
        }

        /// <summary>Local-space end of the rail (handle position when normalized = 1).</summary>
        public Vector3 LocalEnd
        {
            get => localEnd;
            set => localEnd = value;
        }

        /// <summary>Current handle position along the rail, normalized to [0, 1].</summary>
        public float CurrentNormalized => currentNormalized;

        protected virtual void Start()
        {
            _originalPosition = interactableObject.transform.localPosition;
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected) return;

            float requested = ProjectHandToNormalized(handWorldPosition);
            float applied = ProcessNormalizedPosition(currentNormalized, requested);

            if (!Mathf.Approximately(applied, currentNormalized) || interactableObject.transform.localPosition != PositionAtNormalized(applied))
            {
                currentNormalized = applied;
                ApplyNormalized(applied);
                OnNormalizedApplied(applied);
            }
        }

        /// <summary>
        /// Subclass implements clamping, snap-on-release, or stepping logic.
        /// Returns the normalized position to apply this frame.
        /// </summary>
        protected abstract float ProcessNormalizedPosition(float current, float requested);

        /// <summary>
        /// Hook fired after a normalized position is applied. Subclasses use this for events,
        /// step detection, or haptics.
        /// </summary>
        protected virtual void OnNormalizedApplied(float newNormalized) { }

        protected void ApplyNormalized(float t)
        {
            if (interactableObject == null) return;
            interactableObject.transform.localPosition = Vector3.Lerp(localStart, localEnd, t);
        }

        protected Vector3 PositionAtNormalized(float t)
        {
            return Vector3.Lerp(localStart, localEnd, t);
        }

        protected float ProjectHandToNormalized(Vector3 handWorldPosition)
        {
            Vector3 localHand = transform.InverseTransformPoint(handWorldPosition);
            Vector3 direction = localEnd - localStart;
            if (direction.sqrMagnitude < 1e-6f) return 0f;

            int axis = GetBiggestAxis(direction);
            float projected = Vector3.Project(localHand - localStart, direction)[axis] + localStart[axis];
            float t = (projected - localStart[axis]) / direction[axis];
            return Mathf.Clamp01(t);
        }

        private static int GetBiggestAxis(Vector3 direction)
        {
            float ax = Mathf.Abs(direction.x);
            float ay = Mathf.Abs(direction.y);
            float az = Mathf.Abs(direction.z);
            if (ax >= ay) return ax >= az ? 0 : 2;
            return ay >= az ? 1 : 2;
        }

        protected virtual void OnDrawGizmos()
        {
            var worldStart = transform.TransformPoint(localStart);
            var worldEnd = transform.TransformPoint(localEnd);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldStart, worldEnd);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldStart, 0.02f);
            Gizmos.DrawSphere(worldEnd, 0.02f);

            if (interactableObject != null)
            {
                var localObjPos = interactableObject.transform.localPosition;
                var direction = localEnd - localStart;
                if (direction.sqrMagnitude > 1e-6f)
                {
                    var projected = Vector3.Project(localObjPos - localStart, direction) + localStart;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(transform.TransformPoint(projected), 0.025f);
                }
            }
        }
    }
}
