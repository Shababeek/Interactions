
namespace Shababeek.Core
{
    /// <summary>
    /// Interface for objects that can be tweened (animated) over time.
    /// </summary>
    /// <remarks>
    /// Implement this interface to create custom tweenable objects that can be managed

    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyTweenable : ITweenable
    /// {
    ///     public bool Tween(float scaledDeltaTime) { /* implementation */ }
    ///     public async UniTask TweenAsync(float duration) { /* implementation */ }
    /// }
    /// </code>
    /// </example>
    public interface ITweenable
    {
        /// <summary>
        /// Performs one step of the tween animation.
        /// </summary>
        /// <param name="scaledDeltaTime">The scaled delta time for this frame</param>
        /// <returns>True if the tween has completed, false if it's still running</returns>
        /// <remarks>
        /// This method is called by VariableTweener each frame to update the tween.
        /// Return true when the tween is complete to remove it from the tweening system.
        /// </remarks>
        bool Tween(float scaledDeltaTime);
    }
}