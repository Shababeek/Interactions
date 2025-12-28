using System;
using Shababeek.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Component that allows objects to be socketed into specific locations using trigger-based detection.
    /// Handles socket detection, positioning, and return-to-original-position functionality.
    /// </summary>
    [RequireComponent(typeof(Grabable))]
    [RequireComponent(typeof(VariableTweener))]
    public class Socketable : MonoBehaviour
    {
        [Tooltip("Whether the object should return to its original parent when unsocketed.")] [SerializeField]
        private bool shouldReturnToParent = true;

        [Tooltip("Whether to use smooth animation when returning to original position.")] [SerializeField]
        private bool useSmoothReturn = true;

        [Tooltip("Duration of the smooth return animation in seconds.")] [SerializeField]
        private float returnDuration = 0.5f;

        [Tooltip("Keyboard key for debug socket/unsocket operations.")] [SerializeField]
        private KeyCode debugKey = KeyCode.Space;

        [Tooltip("Transform used as a visual indicator for socket position.")] [SerializeField]
        private Transform indicator;

        [Tooltip("Event raised when the object is successfully socketed.")] [SerializeField]
        private SocketEvent onSocketed;

        [Header("Socket Detection")]
        [Tooltip("Radius of the detection sphere for finding nearby sockets.")] [SerializeField]
        private float detectionRadius = 0.5f;

        [Tooltip("Local space offset for the detection sphere center.")] [SerializeField]
        private Vector3 detectionOffset = Vector3.zero;

        [Tooltip("Layer mask for socket detection. Only objects on these layers will be detected.")] [SerializeField]
        private LayerMask socketLayerMask = -1;

        [Tooltip("The currently detected socket.")] [ReadOnly] [SerializeField]
        private AbstractSocket socket;

        [Tooltip("Indicates whether the object is currently socketed.")] [ReadOnly] [SerializeField]
        private bool isSocketed = false;

        private Transform _initialParent;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private Transform _lastSocket;
        private InteractableBase _interactable;
        private VariableTweener _tweener;
        private TransformTweenable _returnTweenable;
        private bool _isReturning = false;
        private Collider[] _overlapResults = new Collider[3];
        public Transform LastSocket=>_lastSocket;
        /// <summary>
        /// Gets whether the object is currently socketed.
        /// </summary>
        public bool IsSocketed
        {
            get => isSocketed;
            private set
            {
                isSocketed = value;
                if (isSocketed) return;
                if (shouldReturnToParent)
                {
                    Return();
                }
            }
        }

        /// <summary>
        /// Gets the currently detected socket.
        /// </summary>
        public AbstractSocket CurrentSocket => socket;

        /// <summary>
        /// Gets whether the object is currently returning to its original position.
        /// </summary>
        public bool IsReturning => _isReturning;
        public IObservable<AbstractSocket> OnSocketedAsObservable => onSocketed.AsObservable();

        private void Awake()
        {
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

            _interactable = GetComponent<InteractableBase>();
            _interactable.OnDeselected
                .Where(_ => !IsSocketed && socket != null && socket.CanSocket())
                .Do(_ => IsSocketed = true)
                .Do(_ => onSocketed.Invoke(socket))
                .Select(_ => socket.Insert(this))
                .Do(t => _lastSocket=t)
                .Do(LerpToPosition)
                .Subscribe().AddTo(this);
            _interactable.OnDeselected
                .Where(_ => shouldReturnToParent && !IsSocketed && (socket == null || !socket.CanSocket()))
                .Do(_ => Return()).Subscribe().AddTo(this);

            _interactable.OnSelected
                .Where(_ => IsSocketed)
                .Do(_ => IsSocketed = false)
                .Do(_ => socket.Remove(this))
                .Subscribe().AddTo(this);
        }

        public bool Insert(AbstractSocket socket)
        {
            if (!socket.CanSocket())
            {
                return false;
            }
            this.socket = socket;
            var t = socket.Insert(this);
            IsSocketed = true;
            onSocketed.Invoke(socket);
            _lastSocket = t;
            LerpToPosition(t);
            return true;
        }
        private void Update()
        {
            DetectSockets();
            HandeIndicator();
            DebugKeyHandling();
        }

        private void HandeIndicator()
        {
            if (!isSocketed&& socket != null && socket.CanSocket())
            {
                if (indicator)
                {
                    indicator.gameObject.SetActive(true);
                    var pivotInfo = socket.GetPivotForSocketable(this);
                    indicator.position = pivotInfo.position;
                    indicator.rotation = pivotInfo.rotation;
                }
            }
            else
            {
                indicator?.gameObject.SetActive(false);
            }
        }

        private void DebugKeyHandling()
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

        private void LerpToPosition(Transform pivot)
        {
            _interactable.OnStateChanged(InteractionState.None, _interactable.CurrentInteractor);
            transform.parent = pivot.transform;
            transform.position = pivot.transform.position;
            transform.rotation = pivot.transform.rotation;
        }

        private void Return()
        {
            if (_isReturning) return;

            isSocketed = false;
            transform.parent = null;

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
            transform.SetParent(_initialParent);
            transform.localPosition = _initialLocalPosition;
            transform.localRotation = _initialLocalRotation;
        }

        private void ReturnWithTween()
        {
            _isReturning = true;

            var targetTransform = new GameObject($"{gameObject.name}_ReturnTarget").transform;
            if (_initialParent != null)
            {
                targetTransform.SetParent(_initialParent);
                targetTransform.localPosition = _initialLocalPosition;
                targetTransform.localRotation = _initialLocalRotation;
            }
            else
            {
                targetTransform.position = transform.position;
                targetTransform.rotation = transform.rotation;
            }

            // Initialize and start the tween
            _returnTweenable.Initialize(transform, targetTransform);
            _tweener.AddTweenable(_returnTweenable);

            // Set up completion callback
            _returnTweenable.OnTweenComplete += () =>
            {
                transform.SetParent(_initialParent);
                transform.localPosition = _initialLocalPosition;
                transform.localRotation = _initialLocalRotation;


                if (targetTransform != null)
                {
                    DestroyImmediate(targetTransform.gameObject);
                }

                _isReturning = false;
            };
        }


        private void DetectSockets()
        {
            if (isSocketed) return;

            // Calculate world position of detection sphere center with local offset
            Vector3 detectionCenter = transform.TransformPoint(detectionOffset);

            // Use OverlapSphereNonAlloc to detect nearby sockets (avoids GC allocation)
            int hitCount = Physics.OverlapSphereNonAlloc(detectionCenter, detectionRadius, _overlapResults, socketLayerMask);
            
            AbstractSocket closestSocket = null;
            float closestDistance = float.MaxValue;

            // Find the closest socket
            for (int i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];
                var detectedSocket = col.GetComponent<AbstractSocket>();
                if (detectedSocket == null || !detectedSocket.CanSocket()) continue;

                float distance = Vector3.Distance(detectionCenter, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSocket = detectedSocket;
                }
            }

            // Handle socket enter
            if (closestSocket != null && closestSocket != socket)
            {
                // End hovering on previous socket if it exists
                if (socket != null)
                {
                    socket.EndHovering(this);
                }

                socket = closestSocket;
                socket.StartHovering(this);
            }

            // Handle socket exit
            if (closestSocket == null && socket != null)
            {
                socket.EndHovering(this);
                socket = null;
            }
        }

        /// <summary>
        /// Shows the indicator at the specified position and rotation.
        /// </summary>
        public void Indicate(Vector3 position, Quaternion rotation)
        {
            indicator.gameObject.SetActive(!isSocketed);
            indicator.transform.position = position;
            indicator.transform.rotation = rotation;
        }

        /// <summary>
        /// Hides the socket indicator.
        /// </summary>
        public void StopIndication()
        {
            indicator.gameObject.SetActive(false);
        }

        /// <summary>
        /// Forces the object to return to its original position immediately without animation.
        /// </summary>
        public void ForceReturn()
        {
            if (_isReturning)
            {
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
        public void ReturToOriginalState()
        {
            if (isSocketed && socket != null)
            {
                socket.Remove(this);
            }

            if (socket != null)
            {
                socket.EndHovering(this);
            }

            isSocketed = false;
            socket = null;

            if (indicator != null)
            {
                indicator.gameObject.SetActive(false);
            }

            ForceReturn();
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionRadius <= 0) return;

            // Calculate world position of detection sphere center with local offset
            Vector3 detectionCenter = transform.TransformPoint(detectionOffset);

            // Draw wire sphere to visualize detection range
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Green with transparency
            Gizmos.DrawWireSphere(detectionCenter, detectionRadius);

            // Draw a line from transform to detection center
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(transform.position, detectionCenter);

            // Draw a small sphere at the detection center
            Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
            Gizmos.DrawSphere(detectionCenter, 0.02f);

            // Reset color
            Gizmos.color = Color.white;
        }
    }
    [System.Serializable]public class SocketEvent : UnityEvent<AbstractSocket>
{}
}