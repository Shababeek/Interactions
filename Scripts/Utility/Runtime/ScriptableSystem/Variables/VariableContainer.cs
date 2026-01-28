using System;
using System.Collections.Generic;
using System.IO;
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

        #region Persistence

        /// <summary>
        /// Data structure for serializing variable values to JSON.
        /// </summary>
        [Serializable]
        public class VariableContainerData
        {
            public string containerName;
            public string savedAt;
            public List<VariableData> variables = new();
        }

        [Serializable]
        public class VariableData
        {
            public string name;
            public string type;
            public string value;
        }

        /// <summary>
        /// Saves all variable values to a JSON file.
        /// </summary>
        /// <param name="filePath">Full path to save file (including .json extension).</param>
        /// <returns>True if save was successful.</returns>
        public bool SaveToFile(string filePath)
        {
            try
            {
                var data = new VariableContainerData
                {
                    containerName = name,
                    savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                foreach (var variable in variables)
                {
                    if (variable == null) continue;

                    var varData = new VariableData
                    {
                        name = variable.name,
                        type = variable.GetType().Name
                    };

                    // Serialize value based on type
                    varData.value = SerializeVariableValue(variable);
                    data.variables.Add(varData);
                }

                string json = JsonUtility.ToJson(data, true);

                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
                Debug.Log($"VariableContainer '{name}' saved to: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save VariableContainer: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads variable values from a JSON file.
        /// </summary>
        /// <param name="filePath">Full path to save file.</param>
        /// <returns>True if load was successful.</returns>
        public bool LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found: {filePath}");
                    return false;
                }

                string json = File.ReadAllText(filePath);
                var data = JsonUtility.FromJson<VariableContainerData>(json);

                if (data == null || data.variables == null)
                {
                    Debug.LogError("Invalid save data format");
                    return false;
                }

                int loadedCount = 0;
                foreach (var varData in data.variables)
                {
                    var variable = GetVariable(varData.name);
                    if (variable == null)
                    {
                        Debug.LogWarning($"Variable '{varData.name}' not found in container, skipping");
                        continue;
                    }

                    if (DeserializeVariableValue(variable, varData.value))
                    {
                        loadedCount++;
                    }
                }

                Debug.Log($"VariableContainer '{name}' loaded from: {filePath} ({loadedCount}/{data.variables.Count} variables)");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load VariableContainer: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves to the default save location (Application.persistentDataPath).
        /// </summary>
        /// <param name="fileName">Optional file name (defaults to container name).</param>
        public bool Save(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = $"{name}.json";

            string path = Path.Combine(Application.persistentDataPath, "VariableContainers", fileName);
            return SaveToFile(path);
        }

        /// <summary>
        /// Loads from the default save location.
        /// </summary>
        /// <param name="fileName">Optional file name (defaults to container name).</param>
        public bool Load(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = $"{name}.json";

            string path = Path.Combine(Application.persistentDataPath, "VariableContainers", fileName);
            return LoadFromFile(path);
        }

        /// <summary>
        /// Checks if a save file exists at the default location.
        /// </summary>
        public bool SaveExists(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = $"{name}.json";

            string path = Path.Combine(Application.persistentDataPath, "VariableContainers", fileName);
            return File.Exists(path);
        }

        /// <summary>
        /// Deletes the save file at the default location.
        /// </summary>
        public bool DeleteSave(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = $"{name}.json";

            string path = Path.Combine(Application.persistentDataPath, "VariableContainers", fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted save file: {path}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the default save path for this container.
        /// </summary>
        public string GetDefaultSavePath(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = $"{name}.json";

            return Path.Combine(Application.persistentDataPath, "VariableContainers", fileName);
        }

        /// <summary>
        /// Serializes a variable's value to a JSON string.
        /// </summary>
        private string SerializeVariableValue(ScriptableVariable variable)
        {
            switch (variable)
            {
                case IntVariable intVar:
                    return intVar.Value.ToString();

                case FloatVariable floatVar:
                    return floatVar.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

                case BoolVariable boolVar:
                    return boolVar.Value.ToString();

                case TextVariable textVar:
                    return textVar.Value ?? "";

                case Vector2Variable vec2Var:
                    return JsonUtility.ToJson(vec2Var.Value);

                case Vector3Variable vec3Var:
                    return JsonUtility.ToJson(vec3Var.Value);

                case QuaternionVariable quatVar:
                    return JsonUtility.ToJson(quatVar.Value);

                case ColorVariable colorVar:
                    return JsonUtility.ToJson(colorVar.Value);

                case Vector2IntVariable vec2IntVar:
                    return JsonUtility.ToJson(vec2IntVar.Value);

                case StringListVariable listVar:
                    return JsonUtility.ToJson(new StringListWrapper { items = listVar.Value });

                default:
                    Debug.LogWarning($"Unsupported variable type for serialization: {variable.GetType().Name}");
                    return "";
            }
        }

        /// <summary>
        /// Deserializes a value string into a variable.
        /// </summary>
        private bool DeserializeVariableValue(ScriptableVariable variable, string value)
        {
            try
            {
                switch (variable)
                {
                    case IntVariable intVar:
                        intVar.Value = int.Parse(value);
                        return true;

                    case FloatVariable floatVar:
                        floatVar.Value = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                        return true;

                    case BoolVariable boolVar:
                        boolVar.Value = bool.Parse(value);
                        return true;

                    case TextVariable textVar:
                        textVar.Value = value;
                        return true;

                    case Vector2Variable vec2Var:
                        vec2Var.Value = JsonUtility.FromJson<Vector2>(value);
                        return true;

                    case Vector3Variable vec3Var:
                        vec3Var.Value = JsonUtility.FromJson<Vector3>(value);
                        return true;

                    case QuaternionVariable quatVar:
                        quatVar.Value = JsonUtility.FromJson<Quaternion>(value);
                        return true;

                    case ColorVariable colorVar:
                        colorVar.Value = JsonUtility.FromJson<Color>(value);
                        return true;

                    case Vector2IntVariable vec2IntVar:
                        vec2IntVar.Value = JsonUtility.FromJson<Vector2Int>(value);
                        return true;

                    case StringListVariable listVar:
                        var wrapper = JsonUtility.FromJson<StringListWrapper>(value);
                        listVar.Value = wrapper?.items ?? new List<string>();
                        return true;

                    default:
                        Debug.LogWarning($"Unsupported variable type for deserialization: {variable.GetType().Name}");
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize variable '{variable.name}': {e.Message}");
                return false;
            }
        }

        [Serializable]
        private class StringListWrapper
        {
            public List<string> items;
        }

        #endregion

#if UNITY_EDITOR
        public void EditorAddVariable(ScriptableVariable variable)
        {
            if (variable == null) return;
            if (variables.Contains(variable)) return;
            variables.Add(variable);
            UnityEditor.EditorUtility.SetDirty(this);
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
            var removedVars = variables.RemoveAll(v => v == null);
            var removedEvents = events.RemoveAll(e => e == null);
            if (removedVars > 0 || removedEvents > 0)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
