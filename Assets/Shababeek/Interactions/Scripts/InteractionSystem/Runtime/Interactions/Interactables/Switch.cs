using System;
using Shababeek.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    enum Direction
    {
        Up=1,
        Down=-1,
        None=0
    }
    public class Switch : MonoBehaviour
    {
        [SerializeField] private UnityEvent onUp;
        [SerializeField] private UnityEvent onDown;
        [SerializeField] private UnityEvent onHold;
        
        [SerializeField] private Transform switchBody;
        [SerializeField] private float upRotation =20;
        [SerializeField] private float downRotation =-20;
        [SerializeField] private float rotateSpeed = 10;
        [ReadOnly][SerializeField]private Direction direction;

        private float t = 0;
        private float targetRotation=0;
        private Collider activeCollider ;

        private void Update()
        {
            ChooseDirection();
            Rotate();
        }

        private void ChooseDirection()
        {
            if(!activeCollider)return;
            var dir = activeCollider.transform.position - transform.position;
            var angle = Vector3.SignedAngle(switchBody.right, dir, switchBody.forward);
            switch (angle)
            {
                case > 5 when direction!=Direction.Down:
                    direction = Direction.Down;
                    Debug.Log("down");
                    t = 0;
                    targetRotation = downRotation;
                    onUp.Invoke();
                    break;
                case < -5 when direction!=Direction.Up:
                    direction = Direction.Up;
                    Debug.Log("down");
                    t = 0;
                    targetRotation = upRotation;
                    onDown.Invoke();
                    break;
            }
        }

        private void Rotate()
        {
            t += Time.deltaTime * rotateSpeed;
            t = Mathf.Clamp01(t);
            var angle = Mathf.Lerp(switchBody.rotation.eulerAngles.z, targetRotation, t);
            switchBody.transform.localRotation = Quaternion.Euler(0, 0, targetRotation);
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
            targetRotation = 0;
            t = 0;
        }
    }
}