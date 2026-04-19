namespace Shababeek.Interactions
{
    /// <summary>
    /// Strategy used by <see cref="Socketable"/> to gate which sockets it will attach to.
    /// Only one path runs at runtime.
    /// </summary>
    public enum SocketMatchMode
    {
        /// <summary>
        /// Legacy. Filters sockets by Unity physics LayerMask during OverlapSphere.
        /// Breaks when the grabbed object's collider layer changes (e.g. to Hand).
        /// </summary>
        LayerMask = 0,

        /// <summary>
        /// Recommended. Filters sockets by bitmask category via AbstractSocket.Accepts.
        /// Layer-independent — survives hand-grab layer changes.
        /// </summary>
        Category = 1,
    }
}
