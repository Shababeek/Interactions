using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Types of sequence control operations.
    /// </summary>
    public enum SequenceControlOperation
    {
        /// <summary>Start a sequence and wait for it to complete.</summary>
        StartAndWait = 0,
        /// <summary>Start a sequence without waiting (fire and forget).</summary>
        StartOnly = 1,
        /// <summary>Wait for a sequence to complete (must already be running).</summary>
        WaitForCompletion = 2,
        /// <summary>Wait for a specific step in a sequence to complete.</summary>
        WaitForStep = 3,
    }

    /// <summary>
    /// Controls other sequences from within a step.
    /// Can start sequences, wait for completion, or synchronize with specific steps.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/SequenceControlAction")]
    public class SequenceControlAction : AbstractSequenceAction
    {
        [Tooltip("The sequence to control.")]
        [SerializeField] private Sequence targetSequence;

        [Tooltip("The operation to perform on the target sequence.")]
        [SerializeField] private SequenceControlOperation operation = SequenceControlOperation.StartAndWait;

        [Tooltip("The step index to wait for (only used with WaitForStep operation).")]
        [SerializeField] private int targetStepIndex = 0;

        private void Subscribe()
        {
            if (targetSequence == null) return;

            switch (operation)
            {
                case SequenceControlOperation.StartAndWait:
                    targetSequence.Begin();
                    WaitForSequenceCompletion();
                    break;

                case SequenceControlOperation.StartOnly:
                    targetSequence.Begin();
                    CompleteStep();
                    break;

                case SequenceControlOperation.WaitForCompletion:
                    WaitForSequenceCompletion();
                    break;

                case SequenceControlOperation.WaitForStep:
                    WaitForStepCompletion();
                    break;
            }
        }

        private void WaitForSequenceCompletion()
        {
            targetSequence.OnRaisedData
                .Where(status => status == SequenceStatus.Completed)
                .Take(1)
                .Do(_ => CompleteStep())
                .Subscribe()
                .AddTo(StepDisposable);
        }

        private void WaitForStepCompletion()
        {
            if (targetStepIndex < 0 || targetStepIndex >= targetSequence.Steps.Count)
            {
                Debug.LogWarning($"[SequenceControlAction] Invalid step index {targetStepIndex} for sequence {targetSequence.name}");
                CompleteStep();
                return;
            }

            var targetStep = targetSequence.Steps[targetStepIndex];
            targetStep.OnRaisedData
                .Where(status => status == SequenceStatus.Completed)
                .Take(1)
                .Do(_ => CompleteStep())
                .Subscribe()
                .AddTo(StepDisposable);
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                Subscribe();
            }
            // StepDisposable cleanup is handled by base class
        }
    }
}
