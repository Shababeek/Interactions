
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Simple socket implementation that can hold one socketable object at a time.
    /// </summary>
    public class Socket : AbstractSocket
    {
        [Tooltip("The socketable object currently in this socket.")]
        [ReadOnly] [SerializeField] private Socketable current;

        [Tooltip("Transform defining where the socketable object should be positioned when inserted.")]
        [SerializeField] Transform pivot;
        public override Transform Pivot => pivot ? pivot : transform;

        internal override Transform Insert(Socketable socketable)
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