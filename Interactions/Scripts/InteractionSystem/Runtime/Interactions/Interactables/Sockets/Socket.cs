
using Shababeek.Utilities;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Simple socket implementation that can hold one socketable object at a time.
    /// </summary>
    public class Socket : AbstractSocket
    {
        [Tooltip("The currently socketed object (read-only).")]
        [ReadOnly] [SerializeField] private Socketable current;
        
        public override Transform Insert(Socketable socketable)
        {
            current = socketable;
            return base.Insert(socketable);
        }

        public override void Remove(Socketable socketable)
        {
            current = null;
            base.Remove(socketable);
        }

        public override bool CanSocket()
        {
            return !current;// true if current is null
        }
    }
}