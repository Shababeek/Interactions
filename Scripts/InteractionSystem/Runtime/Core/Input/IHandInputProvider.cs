using System;
using UniRx;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Interface for providing hand input data including finger curl values and button states.
    /// Implementations can provide input from various sources (controllers, hand tracking, etc).
    /// </summary>
    public interface IHandInputProvider
    {
        /// <summary>
        /// Observable for trigger button state changes.
        /// </summary>
        IObservable<VRButtonState> TriggerObservable { get; }
        
        /// <summary>
        /// Observable for grip button state changes.
        /// </summary>
        IObservable<VRButtonState> GripObservable { get; }
        
        /// <summary>
        /// Observable for A button state changes.
        /// </summary>
        IObservable<VRButtonState> AButtonObservable { get; }
        
        /// <summary>
        /// Observable for B button state changes.
        /// </summary>
        IObservable<VRButtonState> BButtonObservable { get; }
        /// <summary>
        /// Observable for both A and B.
        /// </summary>

        IObservable<VRButtonState> ThumbButtonObservable=> AButtonObservable.Merge(BButtonObservable);
        
        /// <summary>
        /// Gets the curl value for a specific finger (0 = extended, 1 = curled).
        /// </summary>
        /// <param name="fingerIndex">Finger index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</param>
        float this[int fingerIndex] { get; }
        
        /// <summary>
        /// Gets the curl value for a specific finger by name.
        /// </summary>
        /// <param name="finger">Finger name.</param>
        float this[FingerName finger] { get; }
        
        /// <summary>
        /// Checks if this input provider is currently active and providing data.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Priority of this input provider (higher priority providers are used first).
        /// </summary>
        int Priority { get; }
    }
}