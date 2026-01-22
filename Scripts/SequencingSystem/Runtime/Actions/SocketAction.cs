using System;
using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{

    public enum SocketEventType
    {
        Connected = 0,
        Disconnected = 1,
        HoverStart = 2,
        HoverEnd = 3,
    }

    /// <summary>
    /// Completes a step when a specific socket event occurs.
    /// Monitors AbstractSocket events (Connected, Disconnected, HoverStart, HoverEnd).
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/SocketAction")]
    public class SocketAction : AbstractSequenceAction
    {
        [Tooltip("The socket to monitor for events.")]
        [SerializeField] private AbstractSocket socket;

        [SerializeField] private SocketEventType socketEventType;

        [Tooltip("Optional: Only complete the step if this specific socketable is involved. Leave empty to accept any socketable.")]
        [SerializeField] private Socketable requiredSocketable;

        [Tooltip("If true, completes the step immediately when the condition is already met at step start.")]
        [SerializeField] private bool completeIfAlreadySocketed = false;

        private void Subscribe()
        {
            if (socket == null) return;

            IObservable<Socketable> observable = socketEventType switch
            {
                SocketEventType.Connected => socket.OnSocketConnected,
                SocketEventType.Disconnected => socket.OnSocketDisconnected,
                SocketEventType.HoverStart => socket.OnHoverStart,
                SocketEventType.HoverEnd => socket.OnHoverEnd,
                _ => null
            };

            if (observable == null) return;

            if (requiredSocketable != null)
            {
                observable = observable.Where(s => s == requiredSocketable);
            }

            observable.Do(_ => CompleteStep()).Subscribe().AddTo(StepDisposable);
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                // Check if condition is already met
                if (completeIfAlreadySocketed && CheckCurrentState())
                {
                    CompleteStep();
                    return;
                }

                Subscribe();
            }
        }

        /// <summary>
        /// Checks if the socket's current state matches the required condition.
        /// Only applicable for Connected/Disconnected event types.
        /// </summary>
        private bool CheckCurrentState()
        {
            if (requiredSocketable == null) return false;

            return socketEventType switch
            {
                SocketEventType.Connected => requiredSocketable.IsSocketed &&
                                             requiredSocketable.CurrentSocket == socket,
                SocketEventType.Disconnected => !requiredSocketable.IsSocketed,
                _ => false
            };
        }
    }
}
