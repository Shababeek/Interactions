using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a list of strings.
    /// Useful for dialogue options, item names, tags, or any list of text values.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/StringListVariable")]
    public class StringListVariable : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The list of strings stored in this variable.")]
        private List<string> value = new();

        [SerializeField]
        [Tooltip("Default values to reset to.")]
        private List<string> defaultValue = new();

        private readonly Subject<Unit> _onRaised = new();

        /// <summary>
        /// Observable that fires when the variable is raised (value changed).
        /// </summary>
        public IObservable<Unit> OnRaised => _onRaised;

        /// <summary>
        /// Gets or sets the list of strings.
        /// Setting the value raises the OnRaised event.
        /// </summary>
        public List<string> Value
        {
            get => value;
            set
            {
                this.value = value ?? new List<string>();
                Raise();
            }
        }

        /// <summary>
        /// Gets the number of strings in the list.
        /// </summary>
        public int Count => value.Count;

        /// <summary>
        /// Gets or sets the string at the specified index.
        /// </summary>
        public string this[int index]
        {
            get => value[index];
            set
            {
                this.value[index] = value;
                Raise();
            }
        }

        /// <summary>
        /// Adds a string to the list.
        /// </summary>
        public void Add(string item)
        {
            value.Add(item);
            Raise();
        }

        /// <summary>
        /// Adds a string to the list only if it doesn't already exist.
        /// </summary>
        public bool AddUnique(string item)
        {
            if (value.Contains(item)) return false;
            value.Add(item);
            Raise();
            return true;
        }

        /// <summary>
        /// Adds multiple strings to the list.
        /// </summary>
        public void AddRange(IEnumerable<string> items)
        {
            value.AddRange(items);
            Raise();
        }

        /// <summary>
        /// Removes a string from the list.
        /// </summary>
        public bool Remove(string item)
        {
            bool removed = value.Remove(item);
            if (removed) Raise();
            return removed;
        }

        /// <summary>
        /// Removes the string at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            value.RemoveAt(index);
            Raise();
        }

        /// <summary>
        /// Inserts a string at the specified index.
        /// </summary>
        public void Insert(int index, string item)
        {
            value.Insert(index, item);
            Raise();
        }

        /// <summary>
        /// Clears all strings from the list.
        /// </summary>
        public void Clear()
        {
            value.Clear();
            Raise();
        }

        /// <summary>
        /// Checks if the list contains the specified string.
        /// </summary>
        public bool Contains(string item) => value.Contains(item);

        /// <summary>
        /// Gets the index of the specified string, or -1 if not found.
        /// </summary>
        public int IndexOf(string item) => value.IndexOf(item);

        /// <summary>
        /// Gets a random string from the list, or null if empty.
        /// </summary>
        public string GetRandom()
        {
            if (value.Count == 0) return null;
            return value[UnityEngine.Random.Range(0, value.Count)];
        }

        /// <summary>
        /// Gets a random string from the list and removes it.
        /// </summary>
        public string PopRandom()
        {
            if (value.Count == 0) return null;
            int index = UnityEngine.Random.Range(0, value.Count);
            string item = value[index];
            value.RemoveAt(index);
            Raise();
            return item;
        }

        /// <summary>
        /// Gets the first string in the list, or null if empty.
        /// </summary>
        public string First => value.Count > 0 ? value[0] : null;

        /// <summary>
        /// Gets the last string in the list, or null if empty.
        /// </summary>
        public string Last => value.Count > 0 ? value[value.Count - 1] : null;

        /// <summary>
        /// Joins all strings with the specified separator.
        /// </summary>
        public string Join(string separator = ", ") => string.Join(separator, value);

        /// <summary>
        /// Sorts the list alphabetically.
        /// </summary>
        public void Sort()
        {
            value.Sort();
            Raise();
        }

        /// <summary>
        /// Reverses the order of strings in the list.
        /// </summary>
        public void Reverse()
        {
            value.Reverse();
            Raise();
        }

        /// <summary>
        /// Shuffles the list randomly.
        /// </summary>
        public void Shuffle()
        {
            for (int i = value.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (value[i], value[j]) = (value[j], value[i]);
            }
            Raise();
        }

        /// <summary>
        /// Manually raises the OnRaised event to notify subscribers.
        /// </summary>
        public void Raise()
        {
            _onRaised.OnNext(Unit.Default);
        }

        /// <summary>
        /// Resets the list to its default values.
        /// </summary>
        public void Reset()
        {
            value = new List<string>(defaultValue);
            Raise();
        }

        /// <summary>
        /// Sets the current value as the default value.
        /// </summary>
        [ContextMenu("Set Current As Default")]
        public void SetCurrentAsDefault()
        {
            defaultValue = new List<string>(value);
        }

        /// <summary>
        /// Returns an enumerator for iterating over the strings.
        /// </summary>
        public List<string>.Enumerator GetEnumerator() => value.GetEnumerator();

        /// <summary>
        /// Creates a copy of the list.
        /// </summary>
        public List<string> ToList() => new List<string>(value);

        /// <summary>
        /// Creates an array copy of the list.
        /// </summary>
        public string[] ToArray() => value.ToArray();

        // Implicit conversion to List<string>
        public static implicit operator List<string>(StringListVariable variable)
        {
            return variable ? variable.Value : null;
        }

        // Use reference equality for Equals (standard object behavior)
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return $"StringListVariable({Count} items)";
        }
    }
}
