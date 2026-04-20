using System;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// 32-bit category bitmask used to match <see cref="Socketable"/>s with <see cref="AbstractSocket"/>s,
    /// independent of GameObject physics layers. Bit names are defined in <see cref="SocketMaskRegistry"/>.
    /// </summary>
    [Serializable]
    public struct SocketMask : IEquatable<SocketMask>
    {
        [SerializeField] private int value;

        public int Value => value;
        public bool IsEmpty => value == 0;

        /// <summary>Returns true if any bit is set in both masks.</summary>
        public bool Overlaps(SocketMask other) => (value & other.value) != 0;

        public static SocketMask FromInt(int v) => new SocketMask { value = v };
        public static implicit operator int(SocketMask m) => m.value;

        public bool Equals(SocketMask other) => value == other.value;
        public override bool Equals(object obj) => obj is SocketMask m && Equals(m);
        public override int GetHashCode() => value;
    }
}
