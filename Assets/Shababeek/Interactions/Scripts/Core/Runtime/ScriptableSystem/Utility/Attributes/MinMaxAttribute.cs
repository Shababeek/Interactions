using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Attribute to define a minimum and maximum range for a float value in the inspector.
    /// </summary>
    public class MinMaxAttribute : PropertyAttribute
    {
        public float MinLimit = 0;
        public float MaxLimit = 1;
        public bool ShowEditRange;
        public bool ShowDebugValues;

        public MinMaxAttribute(int min, int max)
        {
            MinLimit = min;
            MaxLimit = max;
        }
    }
}