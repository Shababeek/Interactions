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
    
    /// <summary>
    /// Physical switch component that responds to trigger interactions.
    /// Rotates the switch body based on interaction direction and raises events.
    /// </summary>
    /// <remarks>
    /// This component creates a physical switch that can be activated by trigger interactions.
    /// It automatically rotates the switch body and raises events based on the interaction direction.
    /// </remarks>
    public class Switch : MonoBehaviour
    {
        [Tooltip("Event raised when the switch is moved to the up position.")]
        [SerializeField] private UnityEvent onUp;
        
        [Tooltip("Event raised when the switch is moved to the down position.")]
        [SerializeField] private UnityEvent onDown;
        
        [Tooltip("Event raised when the switch is held in a position.")]
        [SerializeField] private UnityEvent onHold;
        
        [Tooltip("The transform of the switch body that rotates during interaction.")]
        [SerializeField] private Transform switchBody;
        
        [Tooltip("Rotation angle in degrees for the up position.")]
        [SerializeField] private float upRotation = 20;
        
        [Tooltip("Rotation angle in degrees for the down position.")]
        [SerializeField] private float downRotation = -20;
        
        [Tooltip("Speed of the rotation animation.")]
        [SerializeField] private float rotateSpeed = 10;
        
        [Tooltip("Current direction of the switch.")]
        [ReadOnly][SerializeField] private Direction direction;

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
                    t = 0;
                    targetRotation = downRotation;
                    onUp.Invoke();
                    break;
                case < -5 when direction!=Direction.Up:
                    direction = Direction.Up;
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