using System;
using Shababeek.Utilities;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Defines a conditional transition from one step to another in a branching sequence.
    /// Transitions are evaluated in order; the first matching condition wins.
    /// </summary>
    [Serializable]
    public class StepTransition
    {
        [Tooltip("Condition that must be satisfied to take this transition. If the variable is empty, the transition is unconditional.")]
        [SerializeField] internal BranchCondition condition = new();

        [Tooltip("The step to transition to when this transition is taken.")]
        [SerializeField] internal Step targetStep;

        [Tooltip("Optional GameEvent raised when this transition is taken.")]
        [SerializeField] internal GameEvent transitionEvent;

        [Tooltip("Descriptive label for this transition (shown in the editor).")]
        [SerializeField] internal string label;

        /// <summary>
        /// Gets the condition for this transition.
        /// </summary>
        public BranchCondition Condition => condition;

        /// <summary>
        /// Gets the target step for this transition.
        /// </summary>
        public Step TargetStep => targetStep;

        /// <summary>
        /// Gets the optional event raised when this transition is taken.
        /// </summary>
        public GameEvent TransitionEvent => transitionEvent;

        /// <summary>
        /// Evaluates whether this transition should be taken based on its condition.
        /// </summary>
        public bool Evaluate()
        {
            return condition == null || condition.Evaluate();
        }

        /// <summary>
        /// Gets a display label for the editor.
        /// </summary>
        public string GetDisplayLabel()
        {
            if (!string.IsNullOrEmpty(label)) return label;
            if (condition?.Variable == null) return "Default";
            return $"{condition.Variable.name} {condition.Comparison} ?";
        }
    }

    /// <summary>
    /// Groups all transitions originating from a single step.
    /// </summary>
    [Serializable]
    public class StepTransitionGroup
    {
        [Tooltip("The step these transitions originate from.")]
        public Step fromStep;

        [Tooltip("Ordered list of transitions. First matching condition wins.")]
        public System.Collections.Generic.List<StepTransition> transitions = new();

        public StepTransitionGroup() { }

        public StepTransitionGroup(Step fromStep)
        {
            this.fromStep = fromStep;
            transitions = new System.Collections.Generic.List<StepTransition>();
        }
    }
}
