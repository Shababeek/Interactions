using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Attribute that makes a serialized field read-only in the Unity Inspector.
    /// </summary>
    public class ReadOnly : PropertyAttribute
    {
        // This attribute class is intentionally empty as it only serves as a marker
        // for the custom property drawer to identify read-only fields.
    }
}
