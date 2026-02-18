using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds socket states to BoolVariables.
    /// </summary>
    /// <remarks>
    /// Tracks whether a socket is occupied, hovering, etc. through the scriptable variable system.
    ///
    /// Common use cases include:
    /// - UI indicators showing socket availability
    /// - Enabling features when socket is filled
    /// - Puzzle mechanics requiring specific socket states
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Socket To Bool Binder")]
    [RequireComponent(typeof(AbstractSocket))]
    public class SocketToBoolBinder : MonoBehaviour
    {
        [Header("Socket State Variables")]
        [Tooltip("BoolVariable set to true when socket has an object, false when empty.")]
        [SerializeField] private BoolVariable isOccupiedVariable;

        [Tooltip("BoolVariable set to true when a socketable is hovering near.")]
        [SerializeField] private BoolVariable isHoveringVariable;

        [Header("Options")]
        [Tooltip("Reset variables to false on disable.")]
        [SerializeField] private bool resetOnDisable = true;

        private AbstractSocket _socket;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _socket = GetComponent<AbstractSocket>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            // Socket connected/disconnected
            if (isOccupiedVariable != null)
            {
                _socket.OnSocketConnected
                    .Subscribe(_ => isOccupiedVariable.Value = true)
                    .AddTo(_disposable);

                _socket.OnSocketDisconnected
                    .Subscribe(_ => isOccupiedVariable.Value = false)
                    .AddTo(_disposable);
            }

            // Hover events
            if (isHoveringVariable != null)
            {
                _socket.OnHoverStart
                    .Subscribe(_ => isHoveringVariable.Value = true)
                    .AddTo(_disposable);

                _socket.OnHoverEnd
                    .Subscribe(_ => isHoveringVariable.Value = false)
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();

            if (resetOnDisable)
            {
                if (isOccupiedVariable != null) isOccupiedVariable.Value = false;
                if (isHoveringVariable != null) isHoveringVariable.Value = false;
            }
        }

        /// <summary>
        /// Gets whether the socket is currently occupied.
        /// </summary>
        public bool IsOccupied => isOccupiedVariable != null && isOccupiedVariable.Value;

        /// <summary>
        /// Gets whether a socketable is hovering near the socket.
        /// </summary>
        public bool IsHovering => isHoveringVariable != null && isHoveringVariable.Value;
    }

    /// <summary>
    /// Binds socket events to GameEvents.
    /// </summary>
    /// <remarks>
    /// Fires scriptable GameEvents when socket state changes.
    ///
    /// Common use cases include:
    /// - Triggering effects when items are socketed
    /// - Sound/visual feedback on socket operations
    /// - Game logic triggered by socket completion
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Socket To Event Binder")]
    [RequireComponent(typeof(AbstractSocket))]
    public class SocketToEventBinder : MonoBehaviour
    {
        [Header("Socket Events")]
        [Tooltip("GameEvent raised when an object is socketed.")]
        [SerializeField] private GameEvent onSocketedEvent;

        [Tooltip("GameEvent raised when an object is removed from socket.")]
        [SerializeField] private GameEvent onUnsocketedEvent;

        [Tooltip("GameEvent raised when a socketable starts hovering.")]
        [SerializeField] private GameEvent onHoverStartEvent;

        [Tooltip("GameEvent raised when a socketable stops hovering.")]
        [SerializeField] private GameEvent onHoverEndEvent;

        private AbstractSocket _socket;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _socket = GetComponent<AbstractSocket>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (onSocketedEvent != null)
            {
                _socket.OnSocketConnected
                    .Subscribe(_ => onSocketedEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onUnsocketedEvent != null)
            {
                _socket.OnSocketDisconnected
                    .Subscribe(_ => onUnsocketedEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onHoverStartEvent != null)
            {
                _socket.OnHoverStart
                    .Subscribe(_ => onHoverStartEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onHoverEndEvent != null)
            {
                _socket.OnHoverEnd
                    .Subscribe(_ => onHoverEndEvent.Raise())
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }
    }

    /// <summary>
    /// Binds socketable states to BoolVariables.
    /// </summary>
    /// <remarks>
    /// Tracks whether a socketable object is currently socketed.
    ///
    /// Common use cases include:
    /// - UI showing whether item is in socket
    /// - Preventing certain actions while socketed
    /// - Visual feedback on socketable state
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Socketable To Bool Binder")]
    [RequireComponent(typeof(Socketable))]
    public class SocketableToBoolBinder : MonoBehaviour
    {
        [Header("Socketable State Variables")]
        [Tooltip("BoolVariable set to true when this object is socketed.")]
        [SerializeField] private BoolVariable isSocketedVariable;

        [Tooltip("BoolVariable set to true when near a valid socket.")]
        [SerializeField] private BoolVariable isNearSocketVariable;

        [Header("Options")]
        [Tooltip("Reset variables to false on disable.")]
        [SerializeField] private bool resetOnDisable = true;

        private Socketable _socketable;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _socketable = GetComponent<Socketable>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (isSocketedVariable != null)
            {
                _socketable.OnSocketedAsObservable
                    .Subscribe(_ => isSocketedVariable.Value = true)
                    .AddTo(_disposable);

                // Also track when unsocketed via grabbing
                var interactable = GetComponent<InteractableBase>();
                if (interactable != null)
                {
                    interactable.OnSelected
                        .Where(_ => _socketable.IsSocketed)
                        .Subscribe(_ => isSocketedVariable.Value = false)
                        .AddTo(_disposable);
                }
            }
        }

        private void Update()
        {
            // Update near socket state each frame
            if (isNearSocketVariable != null)
            {
                bool nearSocket = _socketable.CurrentSocket != null && _socketable.CurrentSocket.CanSocket();
                if (isNearSocketVariable.Value != nearSocket)
                {
                    isNearSocketVariable.Value = nearSocket;
                }
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();

            if (resetOnDisable)
            {
                if (isSocketedVariable != null) isSocketedVariable.Value = false;
                if (isNearSocketVariable != null) isNearSocketVariable.Value = false;
            }
        }

        /// <summary>
        /// Gets whether the socketable is currently socketed.
        /// </summary>
        public bool IsSocketed => isSocketedVariable != null && isSocketedVariable.Value;

        /// <summary>
        /// Gets whether the socketable is near a valid socket.
        /// </summary>
        public bool IsNearSocket => isNearSocketVariable != null && isNearSocketVariable.Value;
    }

    /// <summary>
    /// Binds socketable events to GameEvents.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Socketable To Event Binder")]
    [RequireComponent(typeof(Socketable))]
    public class SocketableToEventBinder : MonoBehaviour
    {
        [Header("Socketable Events")]
        [Tooltip("GameEvent raised when this object is socketed.")]
        [SerializeField] private GameEvent onSocketedEvent;

        private Socketable _socketable;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _socketable = GetComponent<Socketable>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (onSocketedEvent != null)
            {
                _socketable.OnSocketedAsObservable
                    .Subscribe(_ => onSocketedEvent.Raise())
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }
    }
}
