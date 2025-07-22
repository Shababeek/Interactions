using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    [AddComponentMenu(menuName : "Shababeek/Interactions/Interactables/VRButton")]
    public class VRButton : MonoBehaviour
    {
        [SerializeField] private UnityEvent onClick;
        [SerializeField] private Transform button;
        [SerializeField] private Vector3 normalPosition = new Vector3(0, .5f, 0);
        [SerializeField] private Vector3 pressedPosition = new Vector3(0, .2f, 0);
        [SerializeField] private bool isClicked;
        [SerializeField] private float pressSpeed = 10;
        [SerializeField]private float coolDownTime = .2f;
        private float _coolDownTimer = 0;
        private float t = 0;
        public IObservable<Unit> OnClick => onClick.AsObservable();

        public Transform Button
        {
            get => button;
            set => button = value;
        }

        private void Update()
        {
            _coolDownTimer += Time.deltaTime;
            t += (isClicked ? Time.deltaTime : -Time.deltaTime) * pressSpeed;
            t = Mathf.Clamp01(t);
            button.transform.localPosition = Vector3.Lerp(normalPosition, pressedPosition, t);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_coolDownTimer < coolDownTime)return;
            if (other.isTrigger || isClicked) return;
            _coolDownTimer = 0;
            onClick.Invoke();
            isClicked = true;
        }
        private void OnTriggerExit(Collider other)
        {
            if (!isClicked ||other.isTrigger) return;
            isClicked = false;
        }
    }
}