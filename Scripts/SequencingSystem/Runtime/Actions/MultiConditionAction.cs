using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Modes for combining multiple conditions.
    /// </summary>
    public enum MultiConditionMode
    {
        /// <summary>All conditions must be completed to finish the step.</summary>
        All = 0,
        /// <summary>Any single condition can complete the step.</summary>
        Any = 1,
        /// <summary>A specific number of conditions must be completed.</summary>
        Count = 2,
    }

    /// <summary>
    /// Completes a step when multiple child actions satisfy a condition.
    /// Can require all, any, or a specific count of actions to complete.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/MultiConditionAction")]
    public class MultiConditionAction : AbstractSequenceAction
    {
        [Tooltip("The mode for combining conditions.")]
        [SerializeField] private MultiConditionMode mode = MultiConditionMode.All;

        [Tooltip("The number of conditions required (only used with Count mode).")]
        [SerializeField] private int requiredCount = 1;

        [Tooltip("The child actions that act as conditions. These should be on child GameObjects.")]
        [SerializeField] private List<AbstractSequenceAction> conditions = new List<AbstractSequenceAction>();

        [Tooltip("If true, automatically finds child actions on Awake.")]
        [SerializeField] private bool autoFindChildActions = true;

        private HashSet<AbstractSequenceAction> _completedConditions = new HashSet<AbstractSequenceAction>();

        private void Awake()
        {
            if (autoFindChildActions)
            {
                // Find all child actions, excluding self
                var childActions = GetComponentsInChildren<AbstractSequenceAction>()
                    .Where(a => a != this)
                    .ToList();

                if (childActions.Count > 0)
                {
                    conditions = childActions;
                }
            }
        }

        private void Subscribe()
        {
            _completedConditions.Clear();

            foreach (var condition in conditions)
            {
                if (condition == null || condition == this) continue;

                condition.Step.OnRaisedData
                    .Where(status => status == SequenceStatus.Completed)
                    .Do(_ => OnConditionCompleted(condition))
                    .Subscribe()
                    .AddTo(StepDisposable);
            }
        }

        private void OnConditionCompleted(AbstractSequenceAction action)
        {
            _completedConditions.Add(action);

            bool shouldComplete = mode switch
            {
                MultiConditionMode.All => _completedConditions.Count >= conditions.Count,
                MultiConditionMode.Any => _completedConditions.Count >= 1,
                MultiConditionMode.Count => _completedConditions.Count >= requiredCount,
                _ => false
            };

            if (shouldComplete)
            {
                CompleteStep();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                Subscribe();
            }
            // StepDisposable cleanup is handled by base class
        }

        /// <summary>
        /// Gets the number of completed conditions.
        /// </summary>
        public int CompletedCount => _completedConditions.Count;

        /// <summary>
        /// Gets the total number of conditions.
        /// </summary>
        public int TotalCount => conditions.Count;

        /// <summary>
        /// Gets the completion progress as a value between 0 and 1.
        /// </summary>
        public float Progress => conditions.Count > 0 ? (float)_completedConditions.Count / conditions.Count : 0f;
    }
}
