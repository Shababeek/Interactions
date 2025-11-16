using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a Quaternion value with rotation manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/QuaternionVariable")]
    public class QuaternionVariable : ScriptableVariable<Quaternion>
    {
        /// <summary>
        /// Sets the rotation using euler angles.
        /// </summary>
        public void SetRotation(Vector3 eulerAngles)
        {
            Value = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// Sets the rotation using individual euler angle components.
        /// </summary>
        public void SetRotation(float x, float y, float z)
        {
            Value = Quaternion.Euler(x, y, z);
        }

        /// <summary>
        /// Rotates around an axis by the specified angle.
        /// </summary>
        public void Rotate(Vector3 axis, float angle)
        {
            Value *= Quaternion.AngleAxis(angle, axis);
        }

        /// <summary>
        /// Sets the rotation to look in the specified direction.
        /// </summary>
        public void LookAt(Vector3 direction)
        {
            Value = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Gets the euler angle representation of the rotation.
        /// </summary>
        public Vector3 EulerAngles => Value.eulerAngles;
        
        /// <summary>
        /// Gets the forward direction based on this rotation.
        /// </summary>
        public Vector3 Forward => Value * Vector3.forward;
        
        /// <summary>
        /// Gets the right direction based on this rotation.
        /// </summary>
        public Vector3 Right => Value * Vector3.right;
        
        /// <summary>
        /// Gets the up direction based on this rotation.
        /// </summary>
        public Vector3 Up => Value * Vector3.up;

        // Equality operators
        public static bool operator ==(QuaternionVariable a, QuaternionVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(QuaternionVariable a, QuaternionVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(QuaternionVariable a, Quaternion b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(QuaternionVariable a, Quaternion b)
        {
            return !(a == b);
        }

        public static bool operator ==(Quaternion a, QuaternionVariable b)
        {
            return b == a;
        }

        public static bool operator !=(Quaternion a, QuaternionVariable b)
        {
            return !(b == a);
        }

        // Quaternion multiplication operator
        public static Quaternion operator *(QuaternionVariable a, QuaternionVariable b)
        {
            if (a == null && b == null) return Quaternion.identity;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value * b.Value;
        }

        public static Quaternion operator *(QuaternionVariable a, Quaternion b)
        {
            if (a == null) return b;
            return a.Value * b;
        }

        public static Quaternion operator *(Quaternion a, QuaternionVariable b)
        {
            if (b == null) return a;
            return a * b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is QuaternionVariable other) return this == other;
            if (obj is Quaternion quaternionValue) return this == quaternionValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}