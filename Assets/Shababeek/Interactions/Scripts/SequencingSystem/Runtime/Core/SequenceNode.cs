using System;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum SequenceStatus
    {
        Inactive,
        Started,
        Completed
    }
    public abstract class SequenceNode : GameEvent<SequenceStatus>
    {
        [SerializeField] protected SequenceStatus status = SequenceStatus.Inactive;
        [SerializeField,ReadOnly]internal AudioSource audioObject;
        public abstract void Begin();
        protected override SequenceStatus DefaultValue => status;
    }
}