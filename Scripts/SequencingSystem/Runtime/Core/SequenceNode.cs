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
        /// <summary>
        /// Live run status. Deliberately not serialized: this is runtime state, and persisting it
        /// writes a half-finished playthrough back into the asset on disk.
        /// </summary>
        [NonSerialized] protected SequenceStatus status = SequenceStatus.Inactive;

        /// <summary>
        /// Voice-over channel. Exclusive: a new line stops the previous one, so narration never overlaps.
        /// Assigned by the owning <see cref="Sequence"/> at runtime.
        /// </summary>
        [NonSerialized] internal AudioSource voiceSource;

        /// <summary>
        /// Sound-effect channel. Played with PlayOneShot so stings layer instead of cutting each other,
        /// and never interrupt the voice channel. Assigned by the owning <see cref="Sequence"/> at runtime.
        /// </summary>
        [NonSerialized] internal AudioSource sfxSource;

        public abstract void Begin();
        protected override SequenceStatus DefaultValue => status;
    }
}
