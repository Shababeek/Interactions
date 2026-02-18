
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Simple socket implementation that can hold one socketable object at a time.
    /// </summary>
    public class Socket : AbstractSocket
    {
        [ReadOnly] [SerializeField] private Socketable current;

        [SerializeField] Transform pivot;
        public override Transform Pivot => pivot ? pivot : transform;

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