using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Listens to multiple steps and raises events when any step starts or completes.
    /// Provides a way to respond to step events across multiple steps simultaneously.
    /// </summary>
    /// <remarks>
    /// This component subscribes to all assigned steps and raises Unity events
    /// when any step starts or completes, allowing for coordinated responses
    /// across multiple steps in a sequence.
    /// </remarks>
    [AddComponentMenu("Shababeek/Sequencing/Multi Step Listener")]
    public class MultiStepListener : MonoBehaviour
    {
        [Tooltip("Array of steps to listen to for status changes.")]
        [SerializeField] internal Step[] steps;
        
        [Tooltip("Unity event raised when any step starts.")]
        [SerializeField] private UnityEvent onStarted;
        
        [Tooltip("Unity event raised when any step completes.")]
        [SerializeField] private UnityEvent onEnded;
        
        private bool current;
        
        /// <summary>
        /// Indicates whether any of the listened steps are currently active.
        /// </summary>
        /// <value>True if any step is active, false otherwise.</value>
        public bool Current => current;
        

        /// <summary>
        /// Observable that fires when any step starts.
        /// </summary>
        /// <value>An observable that emits a Unit when any step starts.</value>
        public IObservable<Unit> OnStarted => onStarted.AsObservable();
        
        /// <summary>
        /// Observable that fires when any step completes.
        /// </summary>
        /// <value>An observable that emits a Unit when any step completes.</value>
        public IObservable<Unit> OnFinished => onEnded.AsObservable();
        private CompositeDisposable disposable;

        private void OnEnable()
        {
            disposable = new();
            foreach (var step in steps)
            {
                step.OnRaisedData.Do(OnStatusChanged).Subscribe().AddTo(disposable);
            }
        }

        private void OnDisable()
        {
            disposable.Dispose();
        }

        public void OnStatusChanged(SequenceStatus elementStatus)
        {
            switch (elementStatus)
            {
                case SequenceStatus.Started:
                    current = true;
                    onStarted?.Invoke();
                    break;
                case SequenceStatus.Completed:
                    current = false;
                    onEnded?.Invoke();
                    break;
            }
        }
    }
}