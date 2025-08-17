using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Attribute to define a minimum and maximum range for a float value in the inspector.
    /// </summary>
    /// <remarks>
    /// This attribute creates a slider in the inspector that constrains a float value
    /// between the specified minimum and maximum values. It can also show debug values
    /// and allow editing of the range limits.
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField]
    /// [MinMax(0f, 100f)]
    /// private float health; // Will show as a slider from 0 to 100
    /// 
    /// [SerializeField]
    /// [MinMax(0f, 1f, true, true)]
    /// private float alpha; // Will show as a slider with editable range and debug values
    /// </code>
    /// </example>
    public class MinMaxAttribute : PropertyAttribute
    {
        [Header("Range Limits")]
        [Tooltip("The minimum value that the slider can reach. Values below this will be clamped.")]
        public float MinLimit = 0f;
        
        [Tooltip("The maximum value that the slider can reach. Values above this will be clamped.")]
        public float MaxLimit = 1f;
        
        [Tooltip("When enabled, allows the user to edit the minimum and maximum range values in the inspector.")]
        public bool ShowEditRange;
        
        [Tooltip("When enabled, displays debug information about the current value and range in the inspector.")]
        public bool ShowDebugValues;

        /// <summary>
        /// Initializes a new instance of the MinMaxAttribute with integer limits.
        /// </summary>
        /// <param name="min">The minimum value for the range</param>
        /// <param name="max">The maximum value for the range</param>
        /// <remarks>
        /// The min and max values define the boundaries of the slider in the inspector.
        /// Values outside this range will be automatically clamped.
        /// </remarks>
        public MinMaxAttribute(int min, int max)
        {
            MinLimit = min;
            MaxLimit = max;
        }
        
        /// <summary>
        /// Initializes a new instance of the MinMaxAttribute with float limits.
        /// </summary>
        /// <param name="min">The minimum value for the range</param>
        /// <param name="max">The maximum value for the range</param>
        /// <remarks>
        /// The min and max values define the boundaries of the slider in the inspector.
        /// Values outside this range will be automatically clamped.
        /// </remarks>
        public MinMaxAttribute(float min, float max)
        {
            MinLimit = min;
            MaxLimit = max;
        }
        
        /// <summary>
        /// Initializes a new instance of the MinMaxAttribute with additional options.
        /// </summary>
        /// <param name="min">The minimum value for the range</param>
        /// <param name="max">The maximum value for the range</param>
        /// <param name="showEditRange">Whether to show editable range controls</param>
        /// <param name="showDebugValues">Whether to show debug information</param>
        /// <remarks>
        /// This constructor allows you to configure the display options for the attribute.
        /// </remarks>
        public MinMaxAttribute(float min, float max, bool showEditRange, bool showDebugValues)
        {
            MinLimit = min;
            MaxLimit = max;
            ShowEditRange = showEditRange;
            ShowDebugValues = showDebugValues;
        }
    }
}