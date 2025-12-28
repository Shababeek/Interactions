using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for socket components that can receive socketable objects.
    /// Handles socket connection, disconnection, and hover events.
    /// </summary>
    [Serializable]
    public abstract class AbstractSocket : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Socketable> onSocketConnected;
        [SerializeField] private UnityEvent<Socketable> onSocketDisconnected;
        [SerializeField] private UnityEvent<Socketable> onHoverStart;
        [SerializeField] private UnityEvent<Socketable> onHoverEnd;

        /// <summary>
        /// The pivot transform where socketable objects will be positioned.
        /// </summary>
        public virtual Transform Pivot => transform;

        /// <summary>
        /// Observable that fires when a socketable object is connected to this socket.
        /// </summary>
        public IObservable<Socketable> OnSocketConnected => onSocketConnected.AsObservable();

        /// <summary>
        /// Observable that fires when a socketable object is disconnected from this socket.
        /// </summary>
        public IObservable<Socketable> OnSocketDisconnected => onSocketDisconnected.AsObservable();

        /// <summary>
        /// Observable that fires when a socketable object starts hovering near this socket.
        /// </summary>
        public IObservable<Socketable> OnHoverStart => onHoverStart.AsObservable();

        /// <summary>
        /// Observable that fires when a socketable object stops hovering near this socket.
        /// </summary>
        public IObservable<Socketable> OnHoverEnd => onHoverEnd.AsObservable();

        /// <summary>
        /// Called when a socketable object gets near the socket.
        /// </summary>
        public virtual void StartHovering(Socketable socketable)
        {
            onHoverStart.Invoke(socketable);
        }

        /// <summary>
        /// Gets the pivot position and rotation for a specific socketable.
        /// </summary>
        public virtual (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
        {
            return (Pivot.position, Pivot.rotation);
        }

        /// <summary>
        /// Called when a socketable object is no longer near the socket.
        /// </summary>
        public virtual void EndHovering(Socketable socketable)
        {
            onHoverEnd.Invoke(socketable);
        }

        /// <summary>
        /// Called when a socketable object is inserted into the socket.
        /// </summary>
        public virtual Transform Insert(Socketable socketable)
        {
            onSocketConnected.Invoke(socketable);
            return Pivot;
        }

        /// <summary>
        /// Called when a socketable object is removed from the socket.
        /// </summary>
        public virtual void Remove(Socketable socketable)
        {
            onSocketDisconnected.Invoke(socketable);
        }

        /// <summary>
        /// Checks if the socket can accept a new socketable object.
        /// </summary>
        public abstract bool CanSocket();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socketable"> the socketable to insert</param>
        /// <returns>true if socketed false otherwise</returns>
        public virtual Transform Socket(Socketable socketable)
        {
            if (!CanSocket()) return null;
            var t =Insert(socketable);
            return t;
        }
    }
}