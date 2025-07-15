using System;
using UnityEngine;
using Shababeek.Core;
using UniRx;

namespace Shababeek.Sequencing
{
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/Actions/VariableAction")]
    public class VariableAction : AbstractSequenceAction
    {
        public enum VariableOperation
        {
            Set,
            Check,
            Increment,
            Decrement
        }

        public enum ComparisonType
        {
            Equals,
            NotEquals,
            GreaterThan,
            LessThan,
            GreaterThanOrEqual,
            LessThanOrEqual,
            Contains,
            StartsWith
        }
        [SerializeField] private ScriptableVariable variableReference;
        [SerializeField] private VariableOperation operation = VariableOperation.Set;
        [HideInInspector, SerializeField] private string stringValue;
        [HideInInspector, SerializeField] private float floatValue;
        [HideInInspector, SerializeField] private int intValue;
        [HideInInspector, SerializeField] private bool boolValue;
        [HideInInspector, SerializeField] private Vector3 vector3Value;
        [HideInInspector, SerializeField] private Quaternion quaternionValue;
        [HideInInspector, SerializeField] private Color colorValue;
        [HideInInspector, SerializeField] private ComparisonType comparisonType = ComparisonType.Equals;
        private CompositeDisposable _disposables;

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                if (operation == VariableOperation.Check)
                {
                    _disposables?.Dispose();
                    _disposables = new CompositeDisposable();
                    variableReference.OnRaised
                        .Select(_ => CheckVariable())
                        .Do(_ => Step.CompleteStep())
                        .Subscribe().AddTo(_disposables);
                }
                else
                {
                    ExecuteOperation();
                    Step.CompleteStep();
                }
            }
            else
            {
                _disposables?.Dispose();
            }
        }

        private void ExecuteOperation()
        {
            if (variableReference == null) return;

            switch (operation)
            {
                case VariableOperation.Set:
                    SetVariableValue();
                    break;
                case VariableOperation.Increment:
                    IncrementVariable();
                    break;
                case VariableOperation.Decrement:
                    DecrementVariable();
                    break;
            }
        }

        private void SetVariableValue()
        {
            if (variableReference is FloatVariable floatVar)
                floatVar.Value = floatValue;
            else if (variableReference is IntVariable intVar)
                intVar.Value = intValue;
            else if (variableReference is TextVariable textVar)
                textVar.Value = stringValue;
            else if (variableReference is BoolVariable boolVar)
                boolVar.Value = boolValue;
            else if (variableReference is Vector3Variable vector3Var)
                vector3Var.Value = vector3Value;
            else if (variableReference is QuaternionVariable quaternionVar)
                quaternionVar.Value = quaternionValue;
            else if (variableReference is ColorVariable colorVar)
                colorVar.Value = colorValue;
        }

        private void IncrementVariable()
        {
            if (variableReference is FloatVariable floatVar)
                floatVar.Value += 1f;
            else if (variableReference is IntVariable intVar)
                intVar.Value += 1;
        }

        private void DecrementVariable()
        {
            if (variableReference is FloatVariable floatVar)
                floatVar.Value -= 1f;
            else if (variableReference is IntVariable intVar)
                intVar.Value -= 1;
        }

        private bool CheckVariable()
        {
            if (variableReference is FloatVariable floatVar)
                return Compare(floatVar.Value, floatValue);
            if (variableReference is IntVariable intVar)
                return Compare(intVar.Value, intValue);
            if (variableReference is TextVariable textVar)
                return Compare(textVar.Value, stringValue);
            if (variableReference is BoolVariable boolVar)
                return Compare(boolVar.Value, boolValue);
            return false;
        }
        
        private bool Compare(float a, float b)
        {
            switch (comparisonType)
            {
                case ComparisonType.Equals: return Mathf.Approximately(a, b);
                case ComparisonType.NotEquals: return !Mathf.Approximately(a, b);
                case ComparisonType.GreaterThan: return a > b;
                case ComparisonType.LessThan: return a < b;
                case ComparisonType.GreaterThanOrEqual: return a >= b;
                case ComparisonType.LessThanOrEqual: return a <= b;
                default: return false;
            }
        }

        private bool Compare(int a, int b)
        {
            switch (comparisonType)
            {
                case ComparisonType.Equals: return a == b;
                case ComparisonType.NotEquals: return a != b;
                case ComparisonType.GreaterThan: return a > b;
                case ComparisonType.LessThan: return a < b;
                case ComparisonType.GreaterThanOrEqual: return a >= b;
                case ComparisonType.LessThanOrEqual: return a <= b;
                default: return false;
            }
        }

        private bool Compare(string a, string b)
        {
            switch (comparisonType)
            {
                case ComparisonType.Equals: return a == b;
                case ComparisonType.NotEquals: return a != b;
                case ComparisonType.Contains: return a.Contains(b);
                case ComparisonType.StartsWith: return a.StartsWith(b);
                default: return false;
            }
        }

        private bool Compare(bool a, bool b)
        {
            switch (comparisonType)
            {
                case ComparisonType.Equals: return a == b;
                case ComparisonType.NotEquals: return a != b;
                default: return false;
            }
        }
    }
}