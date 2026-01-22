using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Container that holds multiple named ScriptableVariables as sub-assets within a single asset file.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/Variable Container")]
    public class VariableContainer : ScriptableObject
    {
        [SerializeField]
        [Tooltip("List of variables stored in this container. Add variables using the + button to create sub-assets.")]
        private List<ScriptableVariable> variables = new();

        /// <summary>
        /// Gets a read-only list of all variables in this container.
        /// </summary>
        public IReadOnlyList<ScriptableVariable> Variables => variables;

        /// <summary>
        /// Gets the number of variables in this container.
        /// </summary>
        public int Count => variables.Count;

        /// <summary>
        /// Gets a variable by index.
        /// </summary>
        /// <param name="index">The index of the variable</param>
        /// <returns>The variable at the specified index</returns>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public ScriptableVariable this[int index] => variables[index];

        /// <summary>
        /// Gets a typed variable by name.
        /// </summary>
        /// <typeparam name="T">The type of variable to get</typeparam>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable if found and of correct type, null otherwise</returns>

        public T Get<T>(string name) where T : ScriptableVariable
        {
            return variables.FirstOrDefault(v => v != null && v.name == name) as T;
        }

        /// <summary>
        /// Gets a variable by name (non-generic).
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable if found, null otherwise</returns>
        public ScriptableVariable Get(string name)
        {
            return variables.FirstOrDefault(v => v != null && v.name == name);
        }

        /// <summary>
        /// Tries to get a typed variable by name.
        /// </summary>
        /// <typeparam name="T">The type of variable to get</typeparam>
        /// <param name="name">The name of the variable</param>
        /// <param name="variable">The output variable if found</param>
        /// <returns>True if the variable was found and is of correct type</returns>
        public bool TryGet<T>(string name, out T variable) where T : ScriptableVariable
        {
            variable = Get<T>(name);
            return variable != null;
        }

        /// <summary>
        /// Checks if the container has a variable with the given name.
        /// </summary>
        /// <param name="name">The name to check for</param>
        /// <returns>True if a variable with the given name exists</returns>
        public bool Has(string name)
        {
            return variables.Any(v => v != null && v.name == name);
        }

        /// <summary>
        /// Gets all variables of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of variables to get</typeparam>
        /// <returns>An enumerable of all variables of the specified type</returns>

        public IEnumerable<T> GetAll<T>() where T : ScriptableVariable
        {
            return variables.OfType<T>();
        }

        /// <summary>
        /// Gets all numeric variables (IntVariable and FloatVariable).
        /// </summary>
        /// <returns>An enumerable of all numeric variables as INumericalVariable</returns>
        public IEnumerable<INumericalVariable> GetAllNumerical()
        {
            return variables.OfType<INumericalVariable>();
        }

        /// <summary>
        /// Gets the names of all variables in this container.
        /// </summary>
        /// <returns>An enumerable of variable names</returns>
        public IEnumerable<string> GetNames()
        {
            return variables.Where(v => v != null).Select(v => v.name);
        }

        /// <summary>
        /// Resets all variables in this container to their default values.
        /// </summary>
        public void ResetAll()
        {
            foreach (var variable in variables)
            {
                if (variable == null) continue;

                // Use reflection to call Reset() if available
                var resetMethod = variable.GetType().GetMethod("Reset", Type.EmptyTypes);
                resetMethod?.Invoke(variable, null);
            }
        }

        /// <summary>
        /// Raises all variables (triggers their events without changing values).
        /// </summary>
        /// <remarks>
        /// Useful for forcing UI updates when the container is first loaded.
        /// </remarks>
        public void RaiseAll()
        {
            foreach (var variable in variables)
            {
                variable?.Raise();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Adds a variable to the container.
        /// </summary>
        /// <param name="variable">The variable to add</param>
        /// <remarks>
        /// This method is called by the custom editor when adding new variables.
        /// The variable should be added as a sub-asset to this container's asset file.
        /// </remarks>
        public void EditorAddVariable(ScriptableVariable variable)
        {
            if (variable == null) return;
            if (!variables.Contains(variable))
            {
                variables.Add(variable);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Editor-only: Removes a variable from the container.
        /// </summary>
        /// <param name="variable">The variable to remove</param>

        public void EditorRemoveVariable(ScriptableVariable variable)
        {
            if (variable == null) return;
            if (variables.Remove(variable))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Editor-only: Removes a variable at the specified index.
        /// </summary>
        /// <param name="index">The index of the variable to remove</param>
        /// <returns>The removed variable, or null if index was invalid</returns>
        public ScriptableVariable EditorRemoveVariableAt(int index)
        {
            if (index < 0 || index >= variables.Count) return null;
            var variable = variables[index];
            variables.RemoveAt(index);
            UnityEditor.EditorUtility.SetDirty(this);
            return variable;
        }

        /// <summary>
        /// Editor-only: Cleans up null references in the variables list.
        /// </summary>
        public void EditorCleanupNulls()
        {
            int removed = variables.RemoveAll(v => v == null);
            if (removed > 0)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
