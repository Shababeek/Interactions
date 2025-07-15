
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{

    public class PhysicsHandFollwer : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float maxVelocity = 50;
        [SerializeField] private float maxDistance = 4;
        [SerializeField] private float minDistance = .01f;
        private float maxVelocitySqrt;
        private Rigidbody body;
        private float timeInverse;
        
        internal Transform Target
        {
            set => target = value;
        }

        private void Start()
        {
            maxVelocitySqrt = maxVelocity * maxVelocity;
            body = GetComponent<Rigidbody>();
            timeInverse = 1 / Time.fixedDeltaTime;
            transform.parent = null;
            Teleport();
        }

        void FixedUpdate()
        {
            SetVelocity();
            SetAngularVelocity();
        }

        private void SetVelocity()
        {
            var velocityVector = target.transform.position - transform.position;
            if (velocityVector.sqrMagnitude > maxDistance || velocityVector.sqrMagnitude < minDistance)
            {
                Teleport();
                return;
            }

            var weightedSpeed = timeInverse * Mathf.Lerp(0, 1, velocityVector.sqrMagnitude*100 );
            body.linearVelocity = velocityVector*weightedSpeed ;
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, maxVelocity);
        }

        private void SetAngularVelocity()
        {
            var relativeRotation = FindRelativeRotation(target.rotation, transform.rotation);
            relativeRotation.ToAngleAxis(out var angle, out var axis);
            var weightedSpeed = timeInverse * Mathf.Lerp(0, 1, angle / 45);
            body.angularVelocity = axis * ( Mathf.Deg2Rad * weightedSpeed);
        }

        private Quaternion FindRelativeRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            }

            return a * Quaternion.Inverse(b);
        }

        public void Teleport()
        {
            body.position = target.position;
            body.rotation = target.rotation;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
        }
    }
}