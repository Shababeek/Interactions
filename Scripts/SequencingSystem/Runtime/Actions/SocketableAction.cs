using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum SocketableEventType
    {
        Socketed = 0,
        SocketedToSpecific = 1,
        Unsocketed = 2,
        Returned = 3,
    }

    /// <summary>
    /// Completes a step when a socketable object is socketed or unsocketed.
    /// Monitors the Socketable component for socket state changes.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/SocketableAction")]
    public class SocketableAction : AbstractSequenceAction
    {
        [Tooltip("The socketable object to monitor.")]
        [SerializeField] private Socketable socketable;
        [SerializeField] private SocketableEventType eventType;

        [Tooltip("Required socket for SocketedToSpecific event type. The step only completes if socketed to this specific socket.")]
        [SerializeField] private AbstractSocket targetSocket;
        [Tooltip("If true, completes the step immediately if the socketable is already in the required state at step start.")]
        [SerializeField] private bool completeIfAlreadyInState = false;

        private void Subscribe()
        {
            if (socketable == null) return;

            switch (eventType)
            {
                case SocketableEventType.Socketed:
                    socketable.OnSocketedAsObservable
                        .Do(_ => CompleteStep())
                        .Subscribe()
                        .AddTo(StepDisposable);
                    break;

                case SocketableEventType.SocketedToSpecific:
                    if (targetSocket == null)
                    {
                        Debug.LogWarning($"[SocketableAction] TargetSocket is required for SocketedToSpecific event type on {gameObject.name}");
                        return;
                    }
                    socketable.OnSocketedAsObservable
                        .Where(socket => socket == targetSocket)
                        .Do(_ => CompleteStep())
                        .Subscribe()
                        .AddTo(StepDisposable);
                    break;

                case SocketableEventType.Unsocketed:
                    Observable.EveryUpdate()
                        .Where(_ => Started && !socketable.IsSocketed && !socketable.IsReturning)
                        .Take(1)
                        .Do(_ => CompleteStep())
                        .Subscribe()
                        .AddTo(StepDisposable);
                    break;

                case SocketableEventType.Returned:
                    Observable.EveryUpdate()
                        .Where(_ => Started && !socketable.IsReturning && !socketable.IsSocketed)
                        .Skip(1)
                        .Take(1)
                        .Do(_ => CompleteStep())
                        .Subscribe()
                        .AddTo(StepDisposable);
                    break;
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                // Check if condition is already met
                if (completeIfAlreadyInState && CheckCurrentState())
                {
                    CompleteStep();
                    return;
                }

                Subscribe();
            }
        }
        
        private bool CheckCurrentState()
        {
            if (socketable == null) return false;

            return eventType switch
            {
                SocketableEventType.Socketed => socketable.IsSocketed,
                SocketableEventType.SocketedToSpecific => socketable.IsSocketed &&
                                                          socketable.CurrentSocket == targetSocket,
                SocketableEventType.Unsocketed => !socketable.IsSocketed,
                SocketableEventType.Returned => !socketable.IsSocketed && !socketable.IsReturning,
                _ => false
            };
        }
    }
}
