using Shababeek.Core;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Component that allows objects to be socketed into specific locations.
    /// Handles socket detection, positioning, and return-to-original-position functionality.
    /// </summary>
    /// <remarks>
    /// This component requires both an InteractableBase and VariableTweener component.
    /// It automatically detects nearby sockets and handles the socketing process
    /// with optional smooth return animations.
    /// </remarks>
    [RequireComponent(typeof(InteractableBase))]
    [RequireComponent(typeof(VariableTweener))]
    public class Socketable : MonoBehaviour
    {
        [Tooltip("Whether the object should return to its original parent when unsocketed.")]
        [SerializeField] private bool shouldReturnToParent = true;
        
        [Tooltip("Whether to use smooth animation when returning to original position.")]
        [SerializeField] private bool useSmoothReturn = true;
        
        [Tooltip("Duration of the smooth return animation in seconds.")]
        [SerializeField] private float returnDuration = 0.5f;
        
        [Tooltip("Layer mask for socket detection.")]
        [SerializeField] private LayerMask mask;
        
        [Tooltip("Renderer used to calculate bounds for socket detection.")]
        [SerializeField] private Renderer boundsRenderer;
        
        [Tooltip("Keyboard key for debug socket/unsocket operations.")]
        [SerializeField] private KeyCode debugKey = KeyCode.S;
        
        [Tooltip("Whether to automatically find bounds from child renderers.")]
        [SerializeField] private bool findBoundsAutomatically = true;
        
        [Tooltip("Custom bounds for socket detection if not using automatic bounds.")]
        [SerializeField] private Bounds bounds;
        
        [Tooltip("Rotation to apply when the object is socketed.")]
        [SerializeField] private Vector3 rotationWhenSocketed = Vector3.zero;
        
        [Tooltip("Transform used as a visual indicator for socket position.")]
        [SerializeField] private Transform indicator;
        
        [Tooltip("Event raised when the object is successfully socketed.")]
        [SerializeField] private UnityEvent onSocketed;
        
        [Tooltip("The currently detected socket.")]
        [ReadOnly] [SerializeField] private AbstractSocket socket;
        
        [Tooltip("Indicates whether the object is currently socketed.")]
        [ReadOnly] [SerializeField] private bool isSocketed = false;

        private Transform _initialParent;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;

        private InteractableBase _interactable;

        private readonly Collider[] _colliders = new Collider [4];
        private Collider _currentCollider;
        private Rigidbody _rb;
        
        // Tweening system for smooth return
        private VariableTweener _tweener;
        private TransformTweenable _returnTweenable;
        private bool _isReturning = false;

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
        
        /// <summary>
        /// Gets the keyboard key used for debug socket/unsocket operations.
        /// </summary>
        public KeyCode DebugKey => debugKey;
        
        /// <summary>
        /// Gets the currently detected socket.
        /// </summary>
        public AbstractSocket CurrentSocket => socket;
        
        /// <summary>
        /// Gets whether the object is currently returning to its original position.
        /// </summary>
        public bool IsReturning => _isReturning;

        public Bounds Bounds
        {
            get => bounds;
            set => bounds = value;
        }

        public Renderer BondsRenderer
        {
            get => boundsRenderer;
            set => boundsRenderer = value;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _tweener = GetComponent<VariableTweener>();
            _returnTweenable = new TransformTweenable();

            if (indicator)
            {
                indicator?.gameObject.SetActive(false);                
            }

            if (shouldReturnToParent)
            {
                _initialParent = transform.parent;
                _initialLocalPosition = transform.localPosition;
                _initialLocalRotation = transform.localRotation;
            }

            
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
            _interactable.OnDeselected
                .Where(_=>shouldReturnToParent && !IsSocketed && socket == null)
                .Do(_=>Return()).Subscribe().AddTo(this);
        


            _interactable.OnSelected
                .Where(_ => IsSocketed)
                .Do(_ => IsSocketed = false)
                .Do(_ => socket.Remove(this))
                .Subscribe().AddTo(this);
        }

        private void GetBounds()
        {
            if (!BondsRenderer) BondsRenderer = GetComponentInChildren<MeshRenderer>();
            Bounds = BondsRenderer.bounds;
            bounds.size *= 1.5f;
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
            if (_isReturning) return; // Prevent multiple return operations
            
            isSocketed = false;
            this.transform.parent = null;
            
            if (shouldReturnToParent)
            {
                if (useSmoothReturn && _tweener != null)
                {
                    ReturnWithTween();
                }
                else
                {
                    ReturnImmediate();
                }
            }
        }
        
        private void ReturnImmediate()
        {
            this.transform.SetParent(_initialParent);
            this.transform.localPosition = _initialLocalPosition;
            this.transform.localRotation = _initialLocalRotation;
            
            // Re-enable physics if needed
            if (_rb && !_rb.isKinematic)
            {
                _rb.isKinematic = false;
            }
        }
        
        private void ReturnWithTween()
        {
            _isReturning = true;
            
            // Create a temporary target transform for the tween
            var targetTransform = new GameObject($"{gameObject.name}_ReturnTarget").transform;
            if (_initialParent != null)
            {
                targetTransform.SetParent(_initialParent);
                targetTransform.localPosition = _initialLocalPosition;
                targetTransform.localRotation = _initialLocalRotation;
            }
            else
            {
                targetTransform.position = transform.position; // Fallback to world position
                targetTransform.rotation = transform.rotation;
            }
            
            // Initialize and start the tween
            _returnTweenable.Initialize(transform, targetTransform);
            _tweener.AddTweenable(_returnTweenable);
            
            // Set up completion callback
            _returnTweenable.OnTweenComplete += () =>
            {
                // Clean up
                this.transform.SetParent(_initialParent);
                this.transform.localPosition = _initialLocalPosition;
                this.transform.localRotation = _initialLocalRotation;
                
                // Re-enable physics if needed
                if (_rb && !_rb.isKinematic)
                {
                    _rb.isKinematic = false;
                }
                
                // Clean up temporary target
                if (targetTransform != null)
                {
                    DestroyImmediate(targetTransform.gameObject);
                }
                
                _isReturning = false;
            };
        }

        private void Update()
        {
            if (!Input.GetKeyDown(debugKey)) return;
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
            bounds.center = transform.position;
            var count = Physics.OverlapBoxNonAlloc(Bounds.center, Bounds.extents, _colliders, Quaternion.identity,
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
        
        /// <summary>
        /// Forces the object to return to its original position immediately.
        /// </summary>
        public void ForceReturn()
        {
            if (_isReturning)
            {
                // Stop any ongoing tween
                if (_tweener != null)
                {
                    _tweener.RemoveTweenable(_returnTweenable);
                }
                _isReturning = false;
            }
            
            ReturnImmediate();
        }
        
        /// <summary>
        /// Forces the object to return to its original position with smooth animation.
        /// </summary>
        public void ForceReturnWithTween()
        {
            if (_isReturning)
            {
                // Stop any ongoing tween
                if (_tweener != null)
                {
                    _tweener.RemoveTweenable(_returnTweenable);
                }
                _isReturning = false;
            }
            
            ReturnWithTween();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Bounds.center+transform.position, Bounds.extents);
        }
    }
}