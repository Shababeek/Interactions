using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a GameObject reference with helper methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/GameObjectVariable")]
    public class GameObjectVariable : ScriptableVariable<GameObject>
    {
        /// <summary>
        /// Sets the active state of the GameObject.
        /// </summary>
        public void SetActive(bool active)
        {
            if (Value != null)
            {
                Value.SetActive(active);
            }
        }

        /// <summary>
        /// Gets a component of the specified type from the GameObject.
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            return Value != null ? Value.GetComponent<T>() : null;
        }

        /// <summary>
        /// Checks if the GameObject has a component of the specified type.
        /// </summary>
        public bool HasComponent<T>() where T : Component
        {
            return Value != null && Value.GetComponent<T>() != null;
        }

        /// <summary>
        /// Gets the Transform component of the GameObject.
        /// </summary>
        public Transform Transform => Value != null ? Value.transform : null;
        
        /// <summary>
        /// Gets whether the GameObject is active in the hierarchy.
        /// </summary>
        public bool IsActive => Value != null && Value.activeInHierarchy;

        // Equality operators
        public static bool operator ==(GameObjectVariable a, GameObjectVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(GameObjectVariable a, GameObjectVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(GameObjectVariable a, GameObject b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(GameObjectVariable a, GameObject b)
        {
            return !(a == b);
        }

        public static bool operator ==(GameObject a, GameObjectVariable b)
        {
            return b == a;
        }

        public static bool operator !=(GameObject a, GameObjectVariable b)
        {
            return !(b == a);
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}