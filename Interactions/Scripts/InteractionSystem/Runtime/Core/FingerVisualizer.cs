
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Visualizes a specific finger for debugging or editor purposes.
    /// </summary>
    [Obsolete("This class is only used for debugging and will be removed in the future")]
    public class FingerVisualizer : MonoBehaviour
    {

        //TODO: remove this class, I only use it for debugging and will be removed in the future
        /// <summary>
        /// The finger to visualize.
        /// </summary>
        [Tooltip("The finger to visualize.")]
        public FingerName finger;
        
    }
}
