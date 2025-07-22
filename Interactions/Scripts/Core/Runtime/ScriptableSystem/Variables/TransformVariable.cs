using UnityEngine;

namespace Shababeek.Core
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/TransformVariable")]
    public class TransformVariable : ScriptableVariable<Transform>
    {
        public void SetPosition(Vector3 position)
        {
            if (Value != null)
            {
                Value.position = position;
            }
        }
        
        public void SetRotation(Quaternion rotation)
        {
            if (Value != null)
            {
                Value.rotation = rotation;
            }
        }
        
        public void SetRotation(Vector3 eulerAngles)
        {
            if (Value != null)
            {
                Value.rotation = Quaternion.Euler(eulerAngles);
            }
        }
        
        public void SetScale(Vector3 scale)
        {
            if (Value != null)
            {
                Value.localScale = scale;
            }
        }
        
        public void SetLocalPosition(Vector3 localPosition)
        {
            if (Value != null)
            {
                Value.localPosition = localPosition;
            }
        }
        
        public void SetLocalRotation(Quaternion localRotation)
        {
            if (Value != null)
            {
                Value.localRotation = localRotation;
            }
        }
        
        public void SetLocalScale(Vector3 localScale)
        {
            if (Value != null)
            {
                Value.localScale = localScale;
            }
        }
        
        public void LookAt(Vector3 target)
        {
            if (Value != null)
            {
                Value.LookAt(target);
            }
        }
        
        public void LookAt(Transform target)
        {
            if (Value != null && target != null)
            {
                Value.LookAt(target);
            }
        }
        
        public Vector3 Position => Value != null ? Value.position : Vector3.zero;
        public Quaternion Rotation => Value != null ? Value.rotation : Quaternion.identity;
        public Vector3 Scale => Value != null ? Value.localScale : Vector3.one;
        public Vector3 LocalPosition => Value != null ? Value.localPosition : Vector3.zero;
        public Quaternion LocalRotation => Value != null ? Value.localRotation : Quaternion.identity;
        public Vector3 LocalScale => Value != null ? Value.localScale : Vector3.one;
        public Vector3 Forward => Value != null ? Value.forward : Vector3.forward;
        public Vector3 Right => Value != null ? Value.right : Vector3.right;
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