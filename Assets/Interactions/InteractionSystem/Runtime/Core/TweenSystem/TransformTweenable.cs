using System;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public class TransformTweenable :ITweenable
    {
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Transform target;
        private Transform transform;
        private float time = 0;
        public event Action OnTweenComplete;
        public bool Tween(float scaledDetaTime)
        {
            time += scaledDetaTime;
            transform.position = Vector3.Lerp(startPosition, target.position, time);
            transform.rotation = Quaternion.Lerp(startRotation, target.rotation, time);
            if (time < 1) return false;
            try
            {
                OnTweenComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            return true;
        }
        public void Initialize(Transform transform, Transform target)
        {
            this.transform = transform;
            startPosition = transform.position;
            startRotation = this.transform.rotation;
            this.target = target;
            time = 0;
        }
    }
}