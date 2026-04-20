using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Marks a <see cref="SocketMask"/> field so its inspector drawer exposes a single-category popup
    /// instead of a multi-select bitmask. The serialized value still holds a bitmask (a single bit set),
    /// so existing <see cref="SocketMask"/> matching logic works unchanged.
    /// </summary>
    public class SingleSocketCategoryAttribute : PropertyAttribute
    {
    }
}
