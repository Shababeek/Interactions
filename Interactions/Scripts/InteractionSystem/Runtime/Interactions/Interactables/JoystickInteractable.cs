using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    public class JoystickInteractable : ConstrainedInteractableBase
    {
        public IObservable<Vector2> OnLeverChanged => onLeverChanged.AsObservable();
        [SerializeField] private bool snapToCenter;
        [SerializeField] private Vector2 limits;
        [SerializeField] private Vector2Event onLeverChanged;

        private Vector2 _currentNormalizedAngle = new(0, 0);
        private Vector2 _oldNormalizedAngle = new(0, 0);


        private void Start()
        {
            OnDeselected
                .Do(_ => Rotate(0,0))
                .Do(_ => InvokeEvents())
                .Subscribe().AddTo(this);
        }

        protected override void Activate()
        {
        }

        protected override void StartHover()
        {
        }

        protected override void EndHover()
        {
        }

        protected override void HandleObjectMovement()
        {
            if (!IsSelected) return;
            
            Rotate(CalculateAngle(transform.right), CalculateAngle(transform.forward));
            InvokeEvents();
        }

        protected override void HandleObjectDeselection()
        {
            Rotate(0, 0);
            InvokeEvents();
        }

        private void Update()
        {
            if (IsSelected)
            {
                HandleObjectMovement();
            }
        }
        private void Rotate(float x, float z)
        {
            var angleX = LimitAngle(x, limits.x);
            var angleZ = LimitAngle(z, limits.y);
            interactableObject.transform.localRotation = Quaternion.Euler(angleX, 0, angleZ);
            _currentNormalizedAngle.x = angleX / (limits.x / 2);
            _currentNormalizedAngle.y = angleZ / (limits.y / 2);
        }
        private void InvokeEvents()
        {
            var differenceX = _currentNormalizedAngle.x - _oldNormalizedAngle.x;
            var differenceZ = _currentNormalizedAngle.y - _oldNormalizedAngle.y;
            var absDifference = Mathf.Max(Mathf.Abs(differenceX), Mathf.Abs(differenceZ));
            if (!(Math.Abs(absDifference) > .1f)) return;
            onLeverChanged.Invoke(_currentNormalizedAngle);
            _oldNormalizedAngle = _currentNormalizedAngle;
        }

        private float CalculateAngle(Vector3 plane)
        {
            //-transform.right
            var direction = CurrentInteractor.transform.position - transform.position;
            direction = Vector3.ProjectOnPlane(direction, -plane).normalized;
            var angle = -Vector3.SignedAngle(direction, transform.up, plane);
            return angle;
        }

        private float LimitAngle(float angle, float limit)
        {
            if (angle > limit / 2)
            {
                angle = limit / 2;
            }

            if (angle < -limit / 2)
            {
                angle = -limit / 2;
            }

            return angle;
        }

        #region private classes
        

        [System.Serializable]
        private class Vector2Event : UnityEvent<Vector2>
        {
        }

        #endregion
    }
}