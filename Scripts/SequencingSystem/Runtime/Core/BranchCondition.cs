using System;
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Comparison operators for branch condition evaluation.
    /// </summary>
    public enum ComparisonType
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual
    }

    /// <summary>
    /// Evaluates a ScriptableVariable against a target value for branching decisions.
    /// </summary>
    [Serializable]
    public class BranchCondition
    {
        [Tooltip("The ScriptableVariable to evaluate. Leave empty for an unconditional (always true) transition.")]
        [SerializeField] private ScriptableVariable variable;

        [Tooltip("The comparison operator to use when evaluating the condition.")]
        [SerializeField] private ComparisonType comparison = ComparisonType.Equals;

        [Tooltip("Target value when the variable is a BoolVariable.")]
        [SerializeField] private bool boolValue = true;

        [Tooltip("Target value when the variable is an IntVariable.")]
        [SerializeField] private int intValue;

        [Tooltip("Target value when the variable is a FloatVariable.")]
        [SerializeField] private float floatValue;

        [Tooltip("Target value when the variable is a TextVariable.")]
        [SerializeField] private string stringValue;

        /// <summary>
        /// Gets the ScriptableVariable being evaluated.
        /// </summary>
        public ScriptableVariable Variable => variable;

        /// <summary>
        /// Gets the comparison operator.
        /// </summary>
        public ComparisonType Comparison => comparison;

        /// <summary>
        /// Evaluates whether this condition is satisfied based on the current variable value.
        /// Returns true if no variable is assigned (unconditional).
        /// </summary>
        public bool Evaluate()
        {
            if (variable == null) return true;

            return variable switch
            {
                BoolVariable boolVar => EvaluateBool(boolVar.Value),
                IntVariable intVar => EvaluateNumeric(intVar.Value, intValue),
                FloatVariable floatVar => EvaluateNumeric(floatVar.Value, floatValue),
                TextVariable textVar => EvaluateString(textVar.Value),
                _ => true
            };
        }

        private bool EvaluateBool(bool value)
        {
            return comparison switch
            {
                ComparisonType.Equals => value == boolValue,
                ComparisonType.NotEquals => value != boolValue,
                _ => value == boolValue
            };
        }

        private bool EvaluateNumeric<T>(T value, T target) where T : IComparable<T>
        {
            int result = value.CompareTo(target);
            return comparison switch
            {
                ComparisonType.Equals => result == 0,
                ComparisonType.NotEquals => result != 0,
                ComparisonType.GreaterThan => result > 0,
                ComparisonType.LessThan => result < 0,
                ComparisonType.GreaterOrEqual => result >= 0,
                ComparisonType.LessOrEqual => result <= 0,
                _ => false
            };
        }

        private bool EvaluateString(string value)
        {
            return comparison switch
            {
                ComparisonType.Equals => string.Equals(value, stringValue, StringComparison.Ordinal),
                ComparisonType.NotEquals => !string.Equals(value, stringValue, StringComparison.Ordinal),
                _ => string.Equals(value, stringValue, StringComparison.Ordinal)
            };
        }
    }
}
