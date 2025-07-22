
using Shababeek.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    public class Socket : AbstractSocket
    {
        [ReadOnly] [SerializeField] private Socketable current;
        public override Transform Insert(Socketable socketable)
        {
            this.current = socketable;
            return base.Insert(socketable);
        }

        public override void Remove(Socketable socketable)
        {
            this.current = null;
            base.Remove(socketable);
        }

        public override bool CanSocket()
        {
            return current;
        }
    }
}