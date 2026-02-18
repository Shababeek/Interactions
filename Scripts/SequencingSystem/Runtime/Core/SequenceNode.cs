using System;
using Shababeek.Interactions.Core;
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents the status of a sequence node in the sequencing system.
    /// </summary>
    public enum SequenceStatus
    {
        Inactive,
        Started,
        Completed
    }

    /// <summary>
    /// Base class for sequence nodes in the sequencing system.
    /// This class provides the structure for sequence nodes like steps and Sequences
    /// </summary>
    public abstract class SequenceNode : GameEvent<SequenceStatus>
    {
        [SerializeField] protected SequenceStatus status = SequenceStatus.Inactive;
        [SerializeField, ReadOnly] internal AudioSource audioObject;

        /// <summary>
        /// Begins execution of this sequence node.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Gets the audio pitch for this sequence node.
        /// </summary>
        internal virtual float SequencePitch => 1f;

        /// <summary>
        /// Called when a child step completes execution.
        /// </summary>
        internal virtual void CompleteStep(Step step) { }

        protected override SequenceStatus DefaultValue => status;
    }
}