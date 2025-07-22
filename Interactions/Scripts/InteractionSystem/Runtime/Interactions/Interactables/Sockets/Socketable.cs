using Shababeek.Core;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    [RequireComponent(typeof(InteractableBase))]
    public class Socketable : MonoBehaviour
    {
        [SerializeField] private bool shouldReturnToParent = true;
        [SerializeField] private LayerMask mask;
        [SerializeField] private MeshRenderer _bondsRenderer;
        [SerializeField] private KeyCode socketKey = KeyCode.S;
        [SerializeField] private bool findBoundsAutomatically = true;
        [SerializeField] private Bounds _bounds;
        [SerializeField] private Vector3 rotationWhenSocketed = Vector3.zero;
        [SerializeField] private Transform indicator;
        [SerializeField] private UnityEvent onSocketed;
        [ReadOnly] [SerializeField] private AbstractSocket socket;
        [ReadOnly] [SerializeField] private bool isSocketed = false;

        private Transform _parent;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        private InteractableBase _interactable;

        private readonly Collider[] _colliders = new Collider [4];
        private Collider _currentCollider;
        private Rigidbody _rb;

        public bool IsSocketed
        {
            get => isSocketed;
            set
            {
                isSocketed = value;
                if (isSocketed) return;
                if (shouldReturnToParent)
                {
                    Return();
                }
                else
                {
                    if (_rb) _rb.isKinematic = false;
                }
            }
        }

        public Vector3 RotationWhenSocketed => rotationWhenSocketed;

        public Bounds bounds
        {
            get => _bounds;
            set => _bounds = value;
        }

        public MeshRenderer bondsRenderer
        {
            get => _bondsRenderer;
            set => _bondsRenderer = value;
        }

        private void Awake()
        {
            indicator.gameObject.SetActive(false);
            if (shouldReturnToParent)
            {
                _parent = transform.parent;
                _initialPosition = transform.position;
                _initialRotation = transform.rotation;
            }

            _rb = GetComponent<Rigidbody>();
            if (findBoundsAutomatically)
            {
                GetBounds();
            }

            _interactable = GetComponent<InteractableBase>();
            _interactable.OnDeselected
                .Where(_ => !IsSocketed && socket != null && socket.CanSocket())
                .Do(_ => IsSocketed = true)
                .Do(_=>onSocketed.Invoke())
                .Select(_ => socket.Insert(this))
                .Do(LerpToPosition)
                .Subscribe().AddTo(this);


            _interactable.OnSelected
                .Where(_ => IsSocketed)
                .Do(_ => IsSocketed = false)
                .Do(_ => socket.Remove(this))
                .Subscribe().AddTo(this);
        }

        private void GetBounds()
        {
            if (!bondsRenderer) bondsRenderer = GetComponentInChildren<MeshRenderer>();
            bounds = bondsRenderer.bounds;
            _bounds.size *= 1.5f;
        }

        private void LerpToPosition(Transform pivot)
        {
            _interactable.OnStateChanged(InteractionState.None,_interactable.CurrentInteractor);
            this.transform.parent = pivot.transform;
            this.transform.position = pivot.transform.position;
            this.transform.rotation = pivot.transform.rotation * Quaternion.Euler(rotationWhenSocketed);

            _rb.isKinematic = true;
        }

        private void Return()
        {
            isSocketed = false;
            this.transform.parent = null;
            if (shouldReturnToParent)
            {
                this.transform.position = _initialPosition;
                this.transform.rotation = _initialRotation;
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(socketKey)) return;
            if (isSocketed)
            {
                socket.Remove(this);
                Return();
            }
            else
            {
                if (!socket || !socket.CanSocket()) return;
                IsSocketed = true;
                var parentSocket = socket.Insert(this);
                LerpToPosition(parentSocket);
            }
        }

        private void FixedUpdate()
        {
            bool found = false;
            _bounds.center = transform.position;
            var count = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _colliders, Quaternion.identity,
                mask);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_colliders[i] == _currentCollider)
                    {
                        found = true;
                        break;
                    }

                    var s = _colliders[i].GetComponent<AbstractSocket>();
                    if (s == null) continue;
                    found = true;
                    _currentCollider = _colliders[i];
                    socket = s;
                    break;
                }
            }

            if (found)
            {
                socket.StartHovering(this);
            }
            else
            {
                if (socket) socket.EndHovering(this);
                socket = null;
                _currentCollider = null;
            }
        }

        public void Indicate(Vector3 position, Quaternion rotation)
        {
            indicator.gameObject.SetActive(!isSocketed);
            indicator.transform.position = position;
            indicator.transform.rotation = rotation;
        }

        public void StopIndication()
        {
            indicator.gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.extents);
        }
    }
}