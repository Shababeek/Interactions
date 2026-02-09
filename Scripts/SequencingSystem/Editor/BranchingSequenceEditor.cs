using System.Collections.Generic;
using Shababeek.Utilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Custom editor for BranchingSequence with step management and transition configuration.
    /// </summary>
    [CustomEditor(typeof(BranchingSequence))]
    public class BranchingSequenceEditor : Editor
    {
        private BranchingSequence _sequence;
        private ReorderableList _stepList;
        private int _selectedStepIndex = -1;
        private Editor _selectedStepEditor;

        private void OnEnable()
        {
            _sequence = (BranchingSequence)target;
            InitializeStepList();
        }

        private void InitializeStepList()
        {
            var stepsProp = serializedObject.FindProperty("allSteps");
            _stepList = new ReorderableList(serializedObject, stepsProp, true, true, true, true);
            _stepList.onAddCallback += OnAddStep;
            _stepList.onRemoveCallback += OnRemoveStep;
            _stepList.drawElementCallback += DrawStepElement;
            _stepList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Steps");
            _stepList.onReorderCallback += OnReorderSteps;
            _stepList.onSelectCallback += list => _selectedStepIndex = list.index;
        }

        #region Step List Callbacks

        private void DrawStepElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var stepProp = _stepList.serializedProperty.GetArrayElementAtIndex(index);
            if (stepProp.objectReferenceValue == null) return;

            var step = (Step)stepProp.objectReferenceValue;
            var elementName = step.name;
            var underscoreIndex = elementName.IndexOf('_') + 1;
            if (underscoreIndex > 0 && underscoreIndex < elementName.Length)
                elementName = elementName.Substring(underscoreIndex);

            var isEntry = _sequence.EntryStep == step;

            // Name field
            var nameRect = new Rect(rect.x, rect.y + 2, rect.width * 0.4f - 4, EditorGUIUtility.singleLineHeight);
            var newName = EditorGUI.TextField(nameRect, elementName);
            if (newName != elementName)
            {
                step.name = $"{_sequence.name}-{index}_{newName}";
                RenameAllSteps();
            }

            // Object field
            var objRect = new Rect(rect.x + rect.width * 0.4f, rect.y + 2, rect.width * 0.35f - 4,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(objRect, stepProp, GUIContent.none);

            // Entry indicator
            var entryRect = new Rect(rect.x + rect.width * 0.75f, rect.y + 2, rect.width * 0.25f,
                EditorGUIUtility.singleLineHeight);
            if (isEntry)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(0.4f, 0.9f, 0.4f);
                EditorGUI.LabelField(entryRect, "[Entry]", EditorStyles.boldLabel);
                GUI.color = prevColor;
            }
            else
            {
                if (GUI.Button(entryRect, "Set Entry"))
                {
                    serializedObject.FindProperty("entryStep").objectReferenceValue = step;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void OnAddStep(ReorderableList list)
        {
            var path = AssetDatabase.GetAssetPath(_sequence);
            var step = CreateInstance<Step>();
            var index = _sequence.AllSteps?.Count ?? 0;
            step.name = $"{_sequence.name}-{index}_step";

            if (_sequence.AllSteps == null) _sequence.Init();

            _sequence.AllSteps.Add(step);
            AssetDatabase.AddObjectToAsset(step, path);

            // Auto-set entry step if this is the first step
            if (_sequence.AllSteps.Count == 1)
            {
                serializedObject.Update();
                serializedObject.FindProperty("entryStep").objectReferenceValue = step;
                serializedObject.ApplyModifiedProperties();
            }

            AssetDatabase.ImportAsset(path);
            serializedObject.Update();
        }

        private void OnRemoveStep(ReorderableList list)
        {
            var step = _sequence.AllSteps[list.index];
            if (step == null) return;

            // Clean up transitions referencing this step
            CleanupTransitionsForStep(step);

            _sequence.AllSteps.RemoveAt(list.index);
            AssetDatabase.RemoveObjectFromAsset(step);

            // Clear entry step if it was removed
            if (_sequence.EntryStep == step)
            {
                serializedObject.Update();
                serializedObject.FindProperty("entryStep").objectReferenceValue =
                    _sequence.AllSteps.Count > 0 ? _sequence.AllSteps[0] : null;
                serializedObject.ApplyModifiedProperties();
            }

            AssetDatabase.SaveAssets();
            RenameAllSteps();

            if (_selectedStepIndex >= _sequence.AllSteps.Count)
                _selectedStepIndex = _sequence.AllSteps.Count - 1;

            _selectedStepEditor = null;
        }

        private void OnReorderSteps(ReorderableList list)
        {
            serializedObject.ApplyModifiedProperties();
            RenameAllSteps();
        }

        private void RenameAllSteps()
        {
            for (var i = 0; i < _sequence.AllSteps.Count; i++)
            {
                var step = _sequence.AllSteps[i];
                if (step == null) continue;
                var underscoreIndex = step.name.IndexOf('_');
                var baseName = underscoreIndex >= 0 ? step.name.Substring(underscoreIndex + 1) : step.name;
                step.name = $"{_sequence.name}-{i}_{baseName}";
            }

            var path = AssetDatabase.GetAssetPath(_sequence);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
        }

        private void CleanupTransitionsForStep(Step step)
        {
            for (int i = _sequence.TransitionGroups.Count - 1; i >= 0; i--)
            {
                var group = _sequence.TransitionGroups[i];
                if (group.fromStep == step)
                {
                    _sequence.TransitionGroups.RemoveAt(i);
                    continue;
                }

                group.transitions.RemoveAll(t => t.targetStep == step);
            }
        }

        #endregion

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Graph view button
            var prevBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.6f, 0.85f, 1f);
            if (GUILayout.Button("Open Graph View", GUILayout.Height(28)))
            {
                BranchingSequenceGraphWindow.Open(_sequence);
            }

            GUI.backgroundColor = prevBgColor;
            EditorGUILayout.Space(4);

            // Core properties
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pitch"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("volume"));

            EditorGUILayout.Space(4);

            // Entry step (read-only display, set via step list buttons)
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entryStep"));
            GUI.enabled = true;

            EditorGUILayout.Space(8);

            // Step list
            _stepList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);

            // Selected step: transitions + properties
            if (_selectedStepIndex >= 0 && _selectedStepIndex < _sequence.AllSteps.Count)
            {
                DrawSelectedStepPanel();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a step above to configure its transitions.", MessageType.Info);
            }

            EditorGUILayout.Space(8);
            DrawCreateInSceneButton();
            DrawRuntimeControls();

            serializedObject.ApplyModifiedProperties();
        }

        #region Selected Step Panel

        private void DrawSelectedStepPanel()
        {
            var step = _sequence.AllSteps[_selectedStepIndex];
            if (step == null) return;

            EditorGUILayout.LabelField($"Selected: {step.name}", EditorStyles.boldLabel);

            // Transitions section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Transitions are evaluated top-to-bottom. First matching condition wins. " +
                "Leave the variable empty for a default (unconditional) transition.",
                MessageType.Info);

            var group = FindOrCreateTransitionGroup(step);
            DrawTransitions(group);

            EditorGUILayout.EndVertical();

            // Step properties
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Step Properties", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            if (_selectedStepEditor == null || _selectedStepEditor.target != step)
                _selectedStepEditor = CreateEditor(step);
            _selectedStepEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        private StepTransitionGroup FindOrCreateTransitionGroup(Step step)
        {
            foreach (var group in _sequence.TransitionGroups)
            {
                if (group.fromStep == step) return group;
            }

            var newGroup = new StepTransitionGroup(step);
            _sequence.TransitionGroups.Add(newGroup);
            EditorUtility.SetDirty(_sequence);
            return newGroup;
        }

        private void DrawTransitions(StepTransitionGroup group)
        {
            for (int i = 0; i < group.transitions.Count; i++)
            {
                EditorGUILayout.BeginVertical("helpBox");
                DrawSingleTransition(group, i);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Transition"))
            {
                Undo.RecordObject(_sequence, "Add Transition");
                group.transitions.Add(new StepTransition());
                EditorUtility.SetDirty(_sequence);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSingleTransition(StepTransitionGroup group, int index)
        {
            var transition = group.transitions[index];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Transition {index}", EditorStyles.miniBoldLabel,
                GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                Undo.RecordObject(_sequence, "Remove Transition");
                group.transitions.RemoveAt(index);
                EditorUtility.SetDirty(_sequence);
                return;
            }

            EditorGUILayout.EndHorizontal();

            // Label
            EditorGUI.BeginChangeCheck();
            var newLabel = EditorGUILayout.TextField("Label", transition.label);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change Transition Label");
                transition.label = newLabel;
                EditorUtility.SetDirty(_sequence);
            }

            // Condition
            DrawCondition(transition.condition);

            // Target step (popup from available steps)
            DrawTargetStepPopup(transition);

            // Transition event
            EditorGUI.BeginChangeCheck();
            var newEvent = (GameEvent)EditorGUILayout.ObjectField("Transition Event",
                transition.transitionEvent, typeof(GameEvent), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change Transition Event");
                transition.transitionEvent = newEvent;
                EditorUtility.SetDirty(_sequence);
            }
        }

        private void DrawCondition(BranchCondition condition)
        {
            if (condition == null) return;

            // Variable selection
            var varProp = condition.Variable;
            EditorGUI.BeginChangeCheck();
            var newVar = (ScriptableVariable)EditorGUILayout.ObjectField("Variable",
                varProp, typeof(ScriptableVariable), false);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change Condition Variable");
                SetConditionVariable(condition, newVar);
                EditorUtility.SetDirty(_sequence);
            }

            if (newVar == null)
            {
                EditorGUILayout.LabelField("  (Unconditional — always taken)", EditorStyles.miniLabel);
                return;
            }

            // Comparison and value based on variable type
            DrawConditionValueFields(condition, newVar);
        }

        private void DrawConditionValueFields(BranchCondition condition, ScriptableVariable variable)
        {
            // Use SerializedObject reflection to edit the condition fields
            // Since BranchCondition is nested, we edit via Undo on the parent asset
            if (variable is BoolVariable)
            {
                DrawBoolCondition(condition);
            }
            else if (variable is IntVariable)
            {
                DrawNumericCondition(condition, isFloat: false);
            }
            else if (variable is FloatVariable)
            {
                DrawNumericCondition(condition, isFloat: true);
            }
            else if (variable is TextVariable)
            {
                DrawStringCondition(condition);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Variable type '{variable.GetType().Name}' is not supported for branching conditions.",
                    MessageType.Warning);
            }
        }

        private void DrawBoolCondition(BranchCondition condition)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Condition");

            EditorGUI.BeginChangeCheck();
            var comparisonOptions = new[] { "Equals", "Not Equals" };
            var currentComp = condition.Comparison == ComparisonType.NotEquals ? 1 : 0;
            var newComp = EditorGUILayout.Popup(currentComp, comparisonOptions, GUILayout.Width(100));

            var boolField = new SerializedObject(_sequence);
            // Direct field editing via reflection for bool value
            var currentBoolVal = GetConditionBoolValue(condition);
            var newBoolVal = EditorGUILayout.Toggle(currentBoolVal);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change Bool Condition");
                SetConditionComparison(condition, newComp == 0 ? ComparisonType.Equals : ComparisonType.NotEquals);
                SetConditionBoolValue(condition, newBoolVal);
                EditorUtility.SetDirty(_sequence);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawNumericCondition(BranchCondition condition, bool isFloat)
        {
            EditorGUI.BeginChangeCheck();

            var comparisonNames = new[]
                { "==", "!=", ">", "<", ">=", "<=" };
            var currentComp = (int)condition.Comparison;
            var newComp = EditorGUILayout.Popup("Comparison", currentComp, comparisonNames);

            float newFloatVal = GetConditionFloatValue(condition);
            int newIntVal = GetConditionIntValue(condition);

            if (isFloat)
                newFloatVal = EditorGUILayout.FloatField("Value", newFloatVal);
            else
                newIntVal = EditorGUILayout.IntField("Value", newIntVal);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change Numeric Condition");
                SetConditionComparison(condition, (ComparisonType)newComp);
                if (isFloat)
                    SetConditionFloatValue(condition, newFloatVal);
                else
                    SetConditionIntValue(condition, newIntVal);
                EditorUtility.SetDirty(_sequence);
            }
        }

        private void DrawStringCondition(BranchCondition condition)
        {
            EditorGUI.BeginChangeCheck();

            var comparisonOptions = new[] { "Equals", "Not Equals" };
            var currentComp = condition.Comparison == ComparisonType.NotEquals ? 1 : 0;
            var newComp = EditorGUILayout.Popup("Comparison", currentComp, comparisonOptions);

            var currentStr = GetConditionStringValue(condition);
            var newStr = EditorGUILayout.TextField("Value", currentStr);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change String Condition");
                SetConditionComparison(condition, newComp == 0 ? ComparisonType.Equals : ComparisonType.NotEquals);
                SetConditionStringValue(condition, newStr);
                EditorUtility.SetDirty(_sequence);
            }
        }

        private void DrawTargetStepPopup(StepTransition transition)
        {
            var stepNames = new string[_sequence.AllSteps.Count + 1];
            stepNames[0] = "(None — End Sequence)";
            int currentIndex = 0;

            for (int i = 0; i < _sequence.AllSteps.Count; i++)
            {
                var s = _sequence.AllSteps[i];
                stepNames[i + 1] = s != null ? s.name : "(null)";
                if (s == transition.targetStep)
                    currentIndex = i + 1;
            }

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup("Target Step", currentIndex, stepNames);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_sequence, "Change Target Step");
                transition.targetStep = newIndex == 0 ? null : _sequence.AllSteps[newIndex - 1];
                EditorUtility.SetDirty(_sequence);
            }
        }

        #endregion

        #region Condition Field Access (reflection helpers)

        // These use System.Reflection to access private serialized fields on BranchCondition
        // since it's a nested [Serializable] class edited through the parent ScriptableObject.

        private static readonly System.Reflection.FieldInfo VariableField =
            typeof(BranchCondition).GetField("variable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo ComparisonField =
            typeof(BranchCondition).GetField("comparison",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo BoolValueField =
            typeof(BranchCondition).GetField("boolValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo IntValueField =
            typeof(BranchCondition).GetField("intValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo FloatValueField =
            typeof(BranchCondition).GetField("floatValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo StringValueField =
            typeof(BranchCondition).GetField("stringValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static void SetConditionVariable(BranchCondition condition, ScriptableVariable variable)
            => VariableField.SetValue(condition, variable);

        private static void SetConditionComparison(BranchCondition condition, ComparisonType comparison)
            => ComparisonField.SetValue(condition, comparison);

        private static bool GetConditionBoolValue(BranchCondition condition)
            => (bool)BoolValueField.GetValue(condition);

        private static void SetConditionBoolValue(BranchCondition condition, bool value)
            => BoolValueField.SetValue(condition, value);

        private static int GetConditionIntValue(BranchCondition condition)
            => (int)IntValueField.GetValue(condition);

        private static void SetConditionIntValue(BranchCondition condition, int value)
            => IntValueField.SetValue(condition, value);

        private static float GetConditionFloatValue(BranchCondition condition)
            => (float)FloatValueField.GetValue(condition);

        private static void SetConditionFloatValue(BranchCondition condition, float value)
            => FloatValueField.SetValue(condition, value);

        private static string GetConditionStringValue(BranchCondition condition)
            => (string)StringValueField.GetValue(condition) ?? "";

        private static void SetConditionStringValue(BranchCondition condition, string value)
            => StringValueField.SetValue(condition, value);

        #endregion

        #region Scene Creation & Runtime

        private void DrawCreateInSceneButton()
        {
            if (GUILayout.Button("Create Sequence in Scene"))
            {
                var obj = new GameObject(_sequence.name);
                obj.AddComponent<BranchingSequenceBehaviour>().sequence = _sequence;

                var listenerObj = new GameObject($"{_sequence.name}_StepListeners");
                var listener = listenerObj.AddComponent<StepEventListener>();
                listener.StepList = new List<StepEventListener.StepWithEvents>();

                foreach (var step in _sequence.AllSteps)
                {
                    if (step != null) listener.AddStep(step);
                }

                listenerObj.transform.parent = obj.transform;
                Selection.activeGameObject = obj;
                Undo.RegisterCreatedObjectUndo(obj, "Create Branching Sequence in Scene");
            }
        }

        private void DrawRuntimeControls()
        {
            if (!Application.isPlaying || !_sequence.Started) return;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Restart"))
            {
                _sequence.Reset();
                _sequence.Begin();
            }

            if (_sequence.CurrentStep != null && GUILayout.Button("Skip Step"))
            {
                _sequence.CurrentStep.CompleteStep();
            }

            EditorGUILayout.EndHorizontal();

            if (_sequence.CurrentStep != null)
            {
                EditorGUILayout.HelpBox($"Current Step: {_sequence.CurrentStep.name}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Sequence completed.", MessageType.Info);
            }
        }

        #endregion
    }
}
