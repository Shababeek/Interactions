using UnityEngine;

namespace Kandooz.Kuest
{
    public class Step : SequenceNode
    {
        [SerializeField] private SequenceStatus stepStatus = SequenceStatus.Inactive;
        private Sequence parentSequence;
        private bool finished = false;

        public SequenceStatus StepStatus
        {
            get => stepStatus;
            protected set
            {
                if (value == stepStatus) return;
                stepStatus = value;
                Raise(value);
            }
        }
        public override void Begin()
        {
            StepStatus = SequenceStatus.Started;
            if (finished)
            {
                StepStatus = SequenceStatus.Completed;
            }
        }

        protected override void Complete()
        {
            StepStatus = SequenceStatus.Completed;
            parentSequence.CompleteStep(this);
        }

        public void OnActionCompleted()
        {
            if (stepStatus == SequenceStatus.Started)
            {
                Complete();
            }
            else
            {
                finished = true;
            }
        }

        public void Initialize(Sequence sequence)
        {
            finished = false;
            stepStatus = SequenceStatus.Inactive;
            parentSequence = sequence;
        }
    }
}