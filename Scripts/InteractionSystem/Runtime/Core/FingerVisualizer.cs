
using System;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Visualizes a specific finger for debugging.
    /// </summary>
    [Obsolete("This class is only used for debugging and will be removed in the future")]
    [AddComponentMenu("")]
    public class FingerVisualizer : MonoBehaviour
    {
        [Tooltip("The finger to visualize.")]
        public FingerName finger;
    }
}
