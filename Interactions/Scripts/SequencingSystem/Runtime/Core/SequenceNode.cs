using System;
using Shababeek.Interactions.Core;
using Shababeek.Utilities;
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
    /// TODO: Add more Node types in the future, like SubSequence, and ParallelSequence
    public abstract class SequenceNode : GameEvent<SequenceStatus>
    {
        [SerializeField] protected SequenceStatus status = SequenceStatus.Inactive;
        [SerializeField, ReadOnly] internal AudioSource audioObject;
        public abstract void Begin();
        protected override SequenceStatus DefaultValue => status;
    }
}