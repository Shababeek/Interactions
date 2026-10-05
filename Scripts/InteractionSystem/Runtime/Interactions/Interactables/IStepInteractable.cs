using System;

namespace Shababeek.Interactions
{
    /// <summary>
    /// An interactable that rests on discrete steps (dial, slider). Lets step-driven feedback
    /// such as <see cref="Feedback.StepLabelHighlighter"/> work with any of them.
    /// </summary>
    public interface IStepInteractable
    {
        /// <summary>Current step index (0-based).</summary>
        int CurrentStep { get; }

        /// <summary>Number of discrete steps.</summary>
        int NumberOfSteps { get; }

        /// <summary>Fired when the current step changes (including while being moved).</summary>
        IObservable<int> OnStepChanged { get; }

        /// <summary>Fired when a step is committed (after the snap completes).</summary>
        IObservable<int> OnStepConfirmed { get; }

        /// <summary>
        /// World position of a step, for placing step labels. Rotary steps sit on a ring of
        /// <paramref name="radius"/> around the axis; linear steps ignore it.
        /// </summary>
        UnityEngine.Vector3 GetStepWorldPosition(int step, float radius);
    }
}
