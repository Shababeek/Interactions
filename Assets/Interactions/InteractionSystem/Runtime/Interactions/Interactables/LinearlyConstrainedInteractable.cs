using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    public class LinearlyConstrainedInteractable : ConstrainedInteractableBase
    {
        [SerializeField] private Transform point1;
        [SerializeField] private Transform point2;
        
        protected override void Activate()
        {
        }

        protected override void StartHover()
        {
        }

        protected override void EndHover()
        {
        }

        private void Update()
        {
            if (!IsSelected) return;
            interactableObject.transform.position =
                GetPositionBetweenTwoPoints(
                    CurrentInteractor.transform.position,
                    point1.position,
                    point2.position
                );
        }

        private void OnDrawGizmos()
        {
            var position2 = point2.position;
            var position1 = point1.position;
            var direction = (position2 - position1);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(position2, .03f);
            Gizmos.DrawSphere(position1, .03f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(position2, position1);
            var projectedPoint = Vector3.Project(interactableObject.position - position1, direction) + position1;
            Gizmos.DrawSphere(projectedPoint, .03f);
        }

        private static Vector3 GetPositionBetweenTwoPoints(Vector3 point, Vector3 start, Vector3 end)
        {
            var direction = (end - start);
            var projectedPoint = Vector3.Project(point - start, direction) + start;
            var normalizedDistance = FindNormalizedDistanceAlongPath(direction, projectedPoint, start);
            return Vector3.Lerp(start, end, normalizedDistance);
        }

        private static float FindNormalizedDistanceAlongPath(Vector3 direction, Vector3 projectedPoint, Vector3 position1)
        {
            var axe = GetBiggestAxe(direction);
            var x = projectedPoint[axe];
            var m = 1 / direction[axe];
            var c = 0 - m * position1.x;
            var t = m * x + c;
            return t;
        }

        private static int GetBiggestAxe(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return Mathf.Abs(direction.x) > Mathf.Abs(direction.z) ? 0 : 2;
            }

            return Mathf.Abs(direction.y) > Mathf.Abs(direction.z) ? 1 : 2;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!point1)
            {
                point1 = new GameObject("Point1").transform;
                point1.transform.parent = transform;
                point1.localPosition = Vector3.zero;
            }
            if (!point2)
            {
                point2 = new GameObject("point2").transform;
                point2.transform.parent = transform;
                point2.localPosition = Vector3.forward;
            }
        }
    }
}