
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Visualizes a specific finger for debugging or editor purposes.
    /// </summary>
    public class FingerVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The finger to visualize.
        /// </summary>
        [Tooltip("The finger to visualize.")]
        public FingerName finger;
        
    }
}
