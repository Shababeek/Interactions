using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a Transform reference with transform manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/TransformVariable")]
    public class TransformVariable : ScriptableVariable<Transform>
    {
        /// <summary>
        /// Sets the world position of the transform.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            if (Value != null)
            {
                Value.position = position;
            }
        }

        /// <summary>
        /// Sets the world rotation of the transform.
        /// </summary>
        public void SetRotation(Quaternion rotation)
        {
            if (Value != null)
            {
                Value.rotation = rotation;
            }
        }

        /// <summary>
        /// Sets the world rotation using euler angles.
        /// </summary>
        public void SetRotation(Vector3 eulerAngles)
        {
            if (Value != null)
            {
                Value.rotation = Quaternion.Euler(eulerAngles);
            }
        }

        /// <summary>
        /// Sets the local scale of the transform.
        /// </summary>
        public void SetScale(Vector3 scale)
        {
            if (Value != null)
            {
                Value.localScale = scale;
            }
        }

        /// <summary>
        /// Sets the local position of the transform.
        /// </summary>
        public void SetLocalPosition(Vector3 localPosition)
        {
            if (Value != null)
            {
                Value.localPosition = localPosition;
            }
        }

        /// <summary>
        /// Sets the local rotation of the transform.
        /// </summary>
        public void SetLocalRotation(Quaternion localRotation)
        {
            if (Value != null)
            {
                Value.localRotation = localRotation;
            }
        }

        /// <summary>
        /// Sets the local scale of the transform.
        /// </summary>
        public void SetLocalScale(Vector3 localScale)
        {
            if (Value != null)
            {
                Value.localScale = localScale;
            }
        }

        /// <summary>
        /// Rotates the transform to look at a target position.
        /// </summary>
        public void LookAt(Vector3 target)
        {
            if (Value != null)
            {
                Value.LookAt(target);
            }
        }

        /// <summary>
        /// Rotates the transform to look at a target transform.
        /// </summary>
        public void LookAt(Transform target)
        {
            if (Value != null && target != null)
            {
                Value.LookAt(target);
            }
        }

        /// <summary>
        /// Gets the world position of the transform.
        /// </summary>
        public Vector3 Position => Value != null ? Value.position : Vector3.zero;
        
        /// <summary>
        /// Gets the world rotation of the transform.
        /// </summary>
        public Quaternion Rotation => Value != null ? Value.rotation : Quaternion.identity;
        
        /// <summary>
        /// Gets the local scale of the transform.
        /// </summary>
        public Vector3 Scale => Value != null ? Value.localScale : Vector3.one;
        
        /// <summary>
        /// Gets the local position of the transform.
        /// </summary>
        public Vector3 LocalPosition => Value != null ? Value.localPosition : Vector3.zero;
        
        /// <summary>
        /// Gets the local rotation of the transform.
        /// </summary>
        public Quaternion LocalRotation => Value != null ? Value.localRotation : Quaternion.identity;
        
        /// <summary>
        /// Gets the local scale of the transform.
        /// </summary>
        public Vector3 LocalScale => Value != null ? Value.localScale : Vector3.one;
        
        /// <summary>
        /// Gets the forward direction vector of the transform.
        /// </summary>
        public Vector3 Forward => Value != null ? Value.forward : Vector3.forward;
        
        /// <summary>
        /// Gets the right direction vector of the transform.
        /// </summary>
        public Vector3 Right => Value != null ? Value.right : Vector3.right;
        
        /// <summary>
        /// Gets the up direction vector of the transform.
        /// </summary>
        public Vector3 Up => Value != null ? Value.up : Vector3.up;

        // Equality operators
        public static bool operator ==(TransformVariable a, TransformVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(TransformVariable a, TransformVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(TransformVariable a, Transform b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(TransformVariable a, Transform b)
        {
            return !(a == b);
        }

        public static bool operator ==(Transform a, TransformVariable b)
        {
            return b == a;
        }

        public static bool operator !=(Transform a, TransformVariable b)
        {
            return !(b == a);
        }

        public override bool Equals(object obj)
        {
            if (obj is TransformVariable other) return this == other;
            if (obj is Transform transformValue) return this == transformValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}