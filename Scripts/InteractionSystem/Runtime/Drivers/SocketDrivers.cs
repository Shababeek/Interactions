using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Writes socket states to BoolVariables.</summary>
    /// <remarks>
    /// Tracks whether a socket is occupied, hovering, etc. through the scriptable variable system.
    ///
    /// Common use cases include:
    /// - UI indicators showing socket availability
    /// - Enabling features when socket is filled
    /// - Puzzle mechanics requiring specific socket states
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Socket To Bool Driver")]
    [RequireComponent(typeof(AbstractSocket))]
    public class SocketToBoolDriver : MonoBehaviour
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

            BindPair(_socket.OnSocketConnected, _socket.OnSocketDisconnected, isOccupiedVariable);
            BindPair(_socket.OnHoverStart,      _socket.OnHoverEnd,           isHoveringVariable);
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

        private void BindPair<TOn, TOff>(IObservable<TOn> onEvent, IObservable<TOff> offEvent, BoolVariable variable)
        {
            if (variable == null) return;
            onEvent.Subscribe(_ => variable.Value = true).AddTo(_disposable);
            offEvent.Subscribe(_ => variable.Value = false).AddTo(_disposable);
        }

        /// <summary>Gets whether the socket is currently occupied.</summary>
        public bool IsOccupied => isOccupiedVariable != null && isOccupiedVariable.Value;

        /// <summary>Gets whether a socketable is hovering near the socket.</summary>
        public bool IsHovering => isHoveringVariable != null && isHoveringVariable.Value;
    }

    /// <summary>Raises GameEvents in response to socket events.</summary>
    /// <remarks>
    /// Fires scriptable GameEvents when socket state changes.
    ///
    /// Common use cases include:
    /// - Triggering effects when items are socketed
    /// - Sound/visual feedback on socket operations
    /// - Game logic triggered by socket completion
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Socket To Event Driver")]
    [RequireComponent(typeof(AbstractSocket))]
    public class SocketToEventDriver : MonoBehaviour
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

            Raise(_socket.OnSocketConnected,    onSocketedEvent);
            Raise(_socket.OnSocketDisconnected, onUnsocketedEvent);
            Raise(_socket.OnHoverStart,         onHoverStartEvent);
            Raise(_socket.OnHoverEnd,           onHoverEndEvent);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void Raise<T>(IObservable<T> source, GameEvent evt)
        {
            if (evt == null) return;
            source.Subscribe(_ => evt.Raise()).AddTo(_disposable);
        }
    }

    /// <summary>Writes socketable states to BoolVariables.</summary>
    /// <remarks>
    /// Tracks whether a socketable object is currently socketed.
    ///
    /// Common use cases include:
    /// - UI showing whether item is in socket
    /// - Preventing certain actions while socketed
    /// - Visual feedback on socketable state
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Socketable To Bool Driver")]
    [RequireComponent(typeof(Socketable))]
    public class SocketableToBoolDriver : MonoBehaviour
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
            if (isNearSocketVariable == null) return;

            bool nearSocket = _socketable.CurrentSocket != null && _socketable.CurrentSocket.CanSocket();
            if (isNearSocketVariable.Value != nearSocket)
            {
                isNearSocketVariable.Value = nearSocket;
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

        /// <summary>Gets whether the socketable is currently socketed.</summary>
        public bool IsSocketed => isSocketedVariable != null && isSocketedVariable.Value;

        /// <summary>Gets whether the socketable is near a valid socket.</summary>
        public bool IsNearSocket => isNearSocketVariable != null && isNearSocketVariable.Value;
    }

    /// <summary>Raises GameEvents in response to socketable events.</summary>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Socketable To Event Driver")]
    [RequireComponent(typeof(Socketable))]
    public class SocketableToEventDriver : MonoBehaviour
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

        private void OnDisable() => _disposable?.Dispose();
    }
}
