using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Kandooz.Kuest.Editor")]

namespace Kandooz.Kuest
{
    [CreateAssetMenu(menuName = "Kandooz/SequenceSystem/Sequence")]
    public class Sequence : SequenceNode
    {
        [SerializeField] public GameObject questObject;
        [HideInInspector] [SerializeField] private List<Step> steps;
        private int currentStepIndex;
        public Step CurrentStep => steps[currentStepIndex];
        public List<Step> Steps=>steps;
        public override void Begin()
        {
            currentStepIndex = 0;
            foreach (var step in steps)
            {
                step.Initialize(this);
            }
            steps[currentStepIndex].Begin();
            Raise(SequenceStatus.Started);
        }
        
        internal void CompleteStep(Step step)
        {
            if (steps[currentStepIndex] != step) return;
            currentStepIndex++;
            if (currentStepIndex < steps.Count)
            {
                steps[currentStepIndex].Begin();
                return;
            }
            Raise(SequenceStatus.Completed);
        }
    }
}