using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a LayerMask with layer manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/LayerMaskVariable")]
    public class LayerMaskVariable : ScriptableVariable<LayerMask>
    {
        /// <summary>
        /// Adds a layer to the mask by layer index.
        /// </summary>
        public void AddLayer(int layer)
        {
            Value |= (1 << layer);
        }

        /// <summary>
        /// Removes a layer from the mask by layer index.
        /// </summary>
        public void RemoveLayer(int layer)
        {
            Value &= ~(1 << layer);
        }

        /// <summary>
        /// Toggles a layer in the mask by layer index.
        /// </summary>
        public void ToggleLayer(int layer)
        {
            Value ^= (1 << layer);
        }

        /// <summary>
        /// Checks if the mask contains a specific layer by layer index.
        /// </summary>
        public bool ContainsLayer(int layer)
        {
            return (Value & (1 << layer)) != 0;
        }

        /// <summary>
        /// Adds a layer to the mask by layer name.
        /// </summary>
        public void AddLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
                AddLayer(layer);
        }

        /// <summary>
        /// Removes a layer from the mask by layer name.
        /// </summary>
        public void RemoveLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
                RemoveLayer(layer);
        }

        /// <summary>
        /// Checks if the mask contains a specific layer by layer name.
        /// </summary>
        public bool ContainsLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer != -1 && ContainsLayer(layer);
        }

        /// <summary>
        /// Clears all layers from the mask.
        /// </summary>
        public void Clear()
        {
            Value = 0;
        }

        /// <summary>
        /// Sets all layers in the mask.
        /// </summary>
        public void SetAll()
        {
            Value = -1;
        }

        /// <summary>
        /// Gets the number of layers in the mask.
        /// </summary>
        public int LayerCount
        {
            get
            {
                int count = 0;
                int mask = Value;
                while (mask != 0)
                {
                    count++;
                    mask &= mask - 1; // Remove the lowest set bit
                }

                return count;
            }
        }

        // Equality operators
        public static bool operator ==(LayerMaskVariable a, LayerMaskVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(LayerMaskVariable a, LayerMaskVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(LayerMaskVariable a, LayerMask b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(LayerMaskVariable a, LayerMask b)
        {
            return !(a == b);
        }

        public static bool operator ==(LayerMask a, LayerMaskVariable b)
        {
            return b == a;
        }

        public static bool operator !=(LayerMask a, LayerMaskVariable b)
        {
            return !(b == a);
        }

        // Bitwise operators
        public static LayerMask operator |(LayerMaskVariable a, LayerMaskVariable b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value | b.Value;
        }

        public static LayerMask operator |(LayerMaskVariable a, LayerMask b)
        {
            if (a == null) return b;
            return a.Value | b;
        }

        public static LayerMask operator |(LayerMask a, LayerMaskVariable b)
        {
            if (b == null) return a;
            return a | b.Value;
        }

        public static LayerMask operator &(LayerMaskVariable a, LayerMaskVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value & b.Value;
        }

        public static LayerMask operator &(LayerMaskVariable a, LayerMask b)
        {
            if (a == null) return 0;
            return a.Value & b;
        }

        public static LayerMask operator &(LayerMask a, LayerMaskVariable b)
        {
            if (b == null) return 0;
            return a & b.Value;
        }

        public static LayerMask operator ^(LayerMaskVariable a, LayerMaskVariable b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value ^ b.Value;
        }

        public static LayerMask operator ^(LayerMaskVariable a, LayerMask b)
        {
            if (a == null) return b;
            return a.Value ^ b;
        }

        public static LayerMask operator ^(LayerMask a, LayerMaskVariable b)
        {
            if (b == null) return a;
            return a ^ b.Value;
        }

        public static LayerMask operator ~(LayerMaskVariable a)
        {
            if (a == null) return 0;
            return ~a.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is LayerMaskVariable other) return this == other;
            if (obj is LayerMask layerMaskValue) return this == layerMaskValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// A reference that can point to either a LayerMaskVariable or use a constant LayerMask value.
    /// </summary>
    [System.Serializable]
    public class LayerMaskReference : VariableReference<LayerMask>
    {
    }
}