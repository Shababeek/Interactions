using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Container that holds multiple named ScriptableVariables and GameEvents as sub-assets.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variable Container")]
    public class VariableContainer : ScriptableObject
    {
        [SerializeField] private List<ScriptableVariable> variables = new();
        [SerializeField] private List<GameEvent> events = new();

        public IReadOnlyList<ScriptableVariable> Variables => variables;
        public IReadOnlyList<GameEvent> Events => events;
        public int VariableCount => variables.Count;
        public int EventCount => events.Count;

        public ScriptableVariable GetVariable(int index) => variables[index];
        public GameEvent GetEvent(int index) => events[index];

        public T GetVariable<T>(string variableName) where T : ScriptableVariable
        {
            return variables.FirstOrDefault(v => v != null && v.name == variableName) as T;
        }

        public ScriptableVariable GetVariable(string variableName)
        {
            return variables.FirstOrDefault(v => v != null && v.name == variableName);
        }

        public T GetEvent<T>(string eventName) where T : GameEvent
        {
            return events.FirstOrDefault(e => e != null && e.name == eventName) as T;
        }

        public GameEvent GetEvent(string eventName)
        {
            return events.FirstOrDefault(e => e != null && e.name == eventName);
        }

        public bool TryGetVariable<T>(string variableName, out T variable) where T : ScriptableVariable
        {
            variable = GetVariable<T>(variableName);
            return variable != null;
        }

        public bool TryGetEvent<T>(string eventName, out T gameEvent) where T : GameEvent
        {
            gameEvent = GetEvent<T>(eventName);
            return gameEvent != null;
        }

        public bool HasVariable(string variableName)
        {
            return variables.Any(v => v != null && v.name == variableName);
        }

        public bool HasEvent(string eventName)
        {
            return events.Any(e => e != null && e.name == eventName);
        }

        public IEnumerable<T> GetAllVariables<T>() where T : ScriptableVariable
        {
            return variables.OfType<T>();
        }

        public IEnumerable<T> GetAllEvents<T>() where T : GameEvent
        {
            return events.OfType<T>();
        }

        public IEnumerable<INumericalVariable> GetAllNumerical()
        {
            return variables.OfType<INumericalVariable>();
        }

        public IEnumerable<string> GetVariableNames()
        {
            return variables.Where(v => v != null).Select(v => v.name);
        }

        public IEnumerable<string> GetEventNames()
        {
            return events.Where(e => e != null).Select(e => e.name);
        }

        public void ResetAllVariables()
        {
            foreach (var variable in variables)
            {
                if (variable == null) continue;
                var resetMethod = variable.GetType().GetMethod("Reset", Type.EmptyTypes);
                resetMethod?.Invoke(variable, null);
            }
        }

        public void RaiseAllVariables()
        {
            foreach (var variable in variables)
            {
                variable?.Raise();
            }
        }

        public void RaiseAllEvents()
        {
            foreach (var evt in events)
            {
                evt?.Raise();
            }
        }

#if UNITY_EDITOR
        public void EditorAddVariable(ScriptableVariable variable)
        {
            if (variable == null) return;
            if (!variables.Contains(variable))
            {
                variables.Add(variable);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public void EditorRemoveVariable(ScriptableVariable variable)
        {
            if (variable == null) return;
            if (variables.Remove(variable))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public ScriptableVariable EditorRemoveVariableAt(int index)
        {
            if (index < 0 || index >= variables.Count) return null;
            var variable = variables[index];
            variables.RemoveAt(index);
            UnityEditor.EditorUtility.SetDirty(this);
            return variable;
        }

        public void EditorAddEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;
            if (!events.Contains(gameEvent))
            {
                events.Add(gameEvent);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public void EditorRemoveEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;
            if (events.Remove(gameEvent))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public GameEvent EditorRemoveEventAt(int index)
        {
            if (index < 0 || index >= events.Count) return null;
            var evt = events[index];
            events.RemoveAt(index);
            UnityEditor.EditorUtility.SetDirty(this);
            return evt;
        }

        public void EditorCleanupNulls()
        {
            int removedVars = variables.RemoveAll(v => v == null);
            int removedEvents = events.RemoveAll(e => e == null);
            if (removedVars > 0 || removedEvents > 0)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
