using UniRx;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    [RequireComponent(typeof(Grabable))]
    [RequireComponent(typeof(Rigidbody))]
    public class Throwable : MonoBehaviour
    {

        [SerializeField] private int iterations=10;
        private Grabable grabable;
        private Rigidbody body;
        private Vector3 lastPosition;
        private Vector3[] velocityList;
        private bool active;
        private int currentIndex = 0;
        private int elapsed;
        private float dtInverse;
        private void Awake()
        {
            grabable = GetComponent<Grabable>();
            velocityList = new Vector3[iterations];
            dtInverse = 1 / Time.fixedDeltaTime;
            body = GetComponent<Rigidbody>();
            grabable.OnSelected.Do(OnSelected).Subscribe().AddTo(this);
            grabable.OnDeselected.Do(OnSelected).Subscribe().AddTo(this); 

        }
        
        void OnSelected (InteractorBase interactor)
        {
            active = true;
            lastPosition = this.transform.position;
            velocityList.Initialize();
            currentIndex = 0;
            elapsed = 0;
        }
        void OnDeselected(InteractableBase interactor)
        {
            active = false;
            var velocity = Vector3.zero;
            for (int i = 0; i < iterations && i<elapsed; i++)
            {
                velocity += velocityList[i];
            }
            velocity *= dtInverse/iterations;
            body.linearVelocity = velocity;
            body.isKinematic = false;
        }
        private void FixedUpdate()
        {
            if (!active) return;
            var currentPosition = transform.position;
            velocityList[currentIndex] = currentPosition - lastPosition;
            lastPosition = currentPosition;
            currentIndex = (currentIndex + 1) % iterations;
            elapsed++;
        }
    }
}