using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    public abstract class AbstractSocket : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Socketable> onSocketConnected;
        [SerializeField] private UnityEvent<Socketable> onSocketDisconnected;
        [SerializeField] private UnityEvent<Socketable> onHoverStart;
        [SerializeField] private UnityEvent<Socketable> onHoverEnd;

        protected virtual Transform Pivot => transform;

        public IObservable<Socketable> OnSocketConnected => onSocketConnected.AsObservable();
        public IObservable<Socketable> OnSocketDisconnected => onSocketDisconnected.AsObservable();
        public IObservable<Socketable> OnHoverStart => onHoverStart.AsObservable();
        public IObservable<Socketable> OnHoverEnd => onHoverEnd.AsObservable();

        /// <summary>
        /// called when an object gets near the socket
        /// </summary>
        public virtual void StartHovering(Socketable socketable)
        {
            onHoverStart.Invoke(socketable);
        }

        /// <summary>
        /// Called when the object is no longer near the object
        /// </summary>
        public virtual void EndHovering(Socketable socketable)
        {
            onHoverEnd.Invoke(socketable);
        }

        /// <summary>
        /// is called when the socketable is Deselected when in Range of the Socketable
        /// </summary>
        /// <param name="socketable"></param>
        /// <returns></returns>
        public virtual Transform Insert(Socketable socketable)
        {
            onSocketConnected.Invoke(socketable);
            return Pivot;
        }

        /// is called when the socketable is Selected after being socketed
        public virtual void Remove(Socketable socketable)
        {
            onSocketDisconnected.Invoke(socketable);

        }

        public abstract bool CanSocket();
    }

}