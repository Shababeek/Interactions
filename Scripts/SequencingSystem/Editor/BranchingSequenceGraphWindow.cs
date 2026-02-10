using Shababeek.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// EditorWindow that hosts the BranchingSequence graph view with a toolbar
    /// and a detail panel for editing selected edges and step nodes.
    /// </summary>
    public class BranchingSequenceGraphWindow : EditorWindow
    {
        [SerializeField] private BranchingSequence sequence;
        private BranchingSequenceGraphView _graphView;
        private Step _lastCurrentStep;
        private bool _wasPlayingAndStarted;

        // Detail panel state
        private VisualElement _detailPanel;
        private Label _detailHeader;
        private IMGUIContainer _detailImgui;

        // Selection state
        private enum SelectionMode { None, Transition, Step }

        private SelectionMode _selectionMode = SelectionMode.None;
        private StepTransition _selectedTransition;
        private Step _selectedFromStep;
        private Step _selectedStep;
        private Editor _selectedStepEditor;

        /// <summary>
        /// Opens the graph window for the specified BranchingSequence asset.
        /// </summary>
        public static void Open(BranchingSequence sequence)
        {
            var window = GetWindow<BranchingSequenceGraphWindow>();
            window.sequence = sequence;
            window.titleContent = new GUIContent("Sequence Graph",
                EditorGUIUtility.IconContent("d_AnimatorController Icon").image);
            window.BuildGraphView();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update += OnEditorUpdate;

            if (sequence != null)
                BuildGraphView();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.update -= OnEditorUpdate;
            _graphView?.SavePositions();
            DestroyStepEditor();
        }

        private void BuildGraphView()
        {
            rootVisualElement.Clear();
            ClearSelection();

            if (sequence == null)
            {
                rootVisualElement.Add(new Label("No BranchingSequence selected.")
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        marginTop = 40,
                        color = new StyleColor(new Color(0.6f, 0.6f, 0.6f))
                    }
                });
                return;
            }

            BuildToolbar();

            // Content area: graph + detail panel side by side
            var contentContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };

            _graphView = new BranchingSequenceGraphView(sequence);
            _graphView.OnTransitionSelected = OnTransitionSelected;
            _graphView.OnStepSelected = OnStepSelected;
            _graphView.OnSelectionCleared = OnSelectionCleared;
            contentContainer.Add(_graphView);

            BuildDetailPanel(contentContainer);

            rootVisualElement.Add(contentContainer);

            _graphView.RefreshGraph();
            _graphView.schedule.Execute(() => _graphView.FrameAll()).ExecuteLater(100);
        }

        #region Toolbar

        private void BuildToolbar()
        {
            var toolbar = new Toolbar();

            var sequenceLabel = new ToolbarButton(() => { Selection.activeObject = sequence; })
            {
                text = sequence.name,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingLeft = 8,
                    paddingRight = 12,
                    minWidth = 100
                }
            };
            sequenceLabel.tooltip = "Click to select the BranchingSequence asset";
            toolbar.Add(sequenceLabel);

            toolbar.Add(new ToolbarSpacer());
            toolbar.Add(new ToolbarButton(() => _graphView?.FrameAll()) { text = "Frame All" });

            toolbar.Add(new ToolbarButton(() =>
            {
                _graphView?.AutoLayout();
                _graphView?.schedule.Execute(() => _graphView.FrameAll()).ExecuteLater(50);
            })
            { text = "Auto Layout" });

            toolbar.Add(new ToolbarButton(() =>
            {
                _graphView?.SavePositions();
                BuildGraphView();
            })
            { text = "Refresh" });

            rootVisualElement.Add(toolbar);
        }

        #endregion

        #region Detail Panel

        private void BuildDetailPanel(VisualElement parent)
        {
            _detailPanel = new VisualElement
            {
                style =
                {
                    width = 300,
                    minWidth = 250,
                    borderLeftWidth = 1,
                    borderLeftColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f)),
                    backgroundColor = new StyleColor(new Color(0.22f, 0.22f, 0.22f))
                }
            };

            // Panel header
            _detailHeader = new Label("Details")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13,
                    paddingLeft = 10,
                    paddingTop = 8,
                    paddingBottom = 6,
                    backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f)),
                    borderBottomWidth = 1,
                    borderBottomColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f))
                }
            };
            _detailPanel.Add(_detailHeader);

            // IMGUI content inside a scroll view
            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                style = { flexGrow = 1 }
            };

            _detailImgui = new IMGUIContainer(DrawDetailPanel)
            {
                style =
                {
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 8
                }
            };
            scrollView.Add(_detailImgui);
            _detailPanel.Add(scrollView);

            parent.Add(_detailPanel);
        }

        private void OnTransitionSelected(StepTransition transition, Step fromStep)
        {
            DestroyStepEditor();
            _selectionMode = SelectionMode.Transition;
            _selectedTransition = transition;
            _selectedFromStep = fromStep;
            _selectedStep = null;
            _detailHeader.text = "Transition Details";
            _detailPanel.style.display = DisplayStyle.Flex;
        }

        private void OnStepSelected(Step step)
        {
            _selectionMode = SelectionMode.Step;
            _selectedTransition = null;
            _selectedFromStep = null;
            _selectedStep = step;
            _detailHeader.text = "Step Properties";
            _detailPanel.style.display = DisplayStyle.Flex;

            if (_selectedStepEditor == null || _selectedStepEditor.target != step)
            {
                DestroyStepEditor();
                if (step != null)
                    _selectedStepEditor = Editor.CreateEditor(step);
            }
        }

        private void OnSelectionCleared()
        {
            ClearSelection();
        }

        private void ClearSelection()
        {
            DestroyStepEditor();
            _selectionMode = SelectionMode.None;
            _selectedTransition = null;
            _selectedFromStep = null;
            _selectedStep = null;

            if (_detailHeader != null)
                _detailHeader.text = "Details";
        }

        private void DestroyStepEditor()
        {
            if (_selectedStepEditor != null)
            {
                DestroyImmediate(_selectedStepEditor);
                _selectedStepEditor = null;
            }
        }

        private void DrawDetailPanel()
        {
            switch (_selectionMode)
            {
                case SelectionMode.Transition:
                    DrawTransitionDetails();
                    break;
                case SelectionMode.Step:
                    DrawStepDetails();
                    break;
                default:
                    EditorGUILayout.HelpBox(
                        "Select a step node or transition edge to edit its properties.",
                        MessageType.Info);
                    break;
            }
        }

        #endregion

        #region Step Details

        private void DrawStepDetails()
        {
            if (_selectedStep == null)
            {
                EditorGUILayout.HelpBox("Selected step is null.", MessageType.Warning);
                return;
            }

            // Step name
            var displayName = _selectedStep.name;
            var underscoreIndex = displayName.IndexOf('_');
            if (underscoreIndex >= 0 && underscoreIndex < displayName.Length - 1)
                displayName = displayName.Substring(underscoreIndex + 1);

            // Entry badge
            bool isEntry = _selectedStep == sequence.EntryStep;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
            if (isEntry)
            {
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                GUILayout.Label("ENTRY", EditorStyles.miniButton, GUILayout.Width(50));
                GUI.backgroundColor = prevBg;
            }
            else
            {
                if (GUILayout.Button("Set Entry", EditorStyles.miniButton, GUILayout.Width(65)))
                {
                    var so = new SerializedObject(sequence);
                    so.FindProperty("entryStep").objectReferenceValue = _selectedStep;
                    so.ApplyModifiedProperties();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Draw the full step inspector
            if (_selectedStepEditor != null && _selectedStepEditor.target != null)
            {
                _selectedStepEditor.OnInspectorGUI();
            }

            EditorGUILayout.Space(8);

            // Outgoing transitions summary
            DrawStepTransitionsSummary();
        }

        private void DrawStepTransitionsSummary()
        {
            EditorGUILayout.LabelField("Outgoing Transitions", EditorStyles.boldLabel);

            bool hasTransitions = false;
            foreach (var group in sequence.TransitionGroups)
            {
                if (group?.fromStep != _selectedStep) continue;
                hasTransitions = true;

                for (int i = 0; i < group.transitions.Count; i++)
                {
                    var t = group.transitions[i];
                    if (t == null) continue;

                    string label = t.GetDisplayLabel();
                    string target = t.targetStep != null ? t.targetStep.name : "(End Sequence)";

                    EditorGUILayout.BeginHorizontal("helpBox");
                    EditorGUILayout.LabelField($"{label}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"\u2192 {target}", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (!hasTransitions)
            {
                EditorGUILayout.LabelField("  No transitions (sequence ends here)", EditorStyles.miniLabel);
            }
        }

        #endregion

        #region Transition Details

        private void DrawTransitionDetails()
        {
            if (_selectedTransition == null)
            {
                EditorGUILayout.HelpBox("Select a transition edge to edit its details.", MessageType.Info);
                return;
            }

            var t = _selectedTransition;

            // From step (read-only)
            GUI.enabled = false;
            EditorGUILayout.ObjectField("From", _selectedFromStep, typeof(Step), false);
            GUI.enabled = true;

            EditorGUILayout.Space(4);

            // Label
            EditorGUI.BeginChangeCheck();
            var newLabel = EditorGUILayout.TextField("Label", t.label);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change Transition Label");
                t.label = newLabel;
                MarkDirtyAndUpdateEdge(t);
            }

            EditorGUILayout.Space(8);

            // --- Condition section ---
            EditorGUILayout.LabelField("Condition", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newVar = (ScriptableVariable)EditorGUILayout.ObjectField("Variable",
                t.condition.Variable, typeof(ScriptableVariable), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change Condition Variable");
                BranchConditionHelper.SetVariable(t.condition, newVar);
                MarkDirtyAndUpdateEdge(t);
            }

            if (newVar == null)
            {
                EditorGUILayout.LabelField("  (Unconditional \u2014 always taken)", EditorStyles.miniLabel);
            }
            else
            {
                DrawConditionValueFields(t.condition, newVar);
            }

            EditorGUILayout.Space(8);

            // --- Target section ---
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            DrawTargetStepPopup(t);

            // Event
            EditorGUI.BeginChangeCheck();
            var newEvent = (GameEvent)EditorGUILayout.ObjectField("Event",
                t.transitionEvent, typeof(GameEvent), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change Transition Event");
                t.transitionEvent = newEvent;
                MarkDirtyAndUpdateEdge(t);
            }

            // --- Runtime status ---
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(12);
                EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);

                bool met;
                try { met = t.Evaluate(); }
                catch { met = false; }

                var prevColor = GUI.color;
                GUI.color = met ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.35f, 0.35f);
                EditorGUILayout.LabelField(
                    met ? "\u2713 Condition Met" : "\u2717 Condition Not Met",
                    EditorStyles.boldLabel);
                GUI.color = prevColor;
            }
        }

        #endregion

        #region Condition Drawing

        private void DrawConditionValueFields(BranchCondition condition, ScriptableVariable variable)
        {
            if (variable is BoolVariable) DrawBoolCondition(condition);
            else if (variable is IntVariable) DrawNumericCondition(condition, false);
            else if (variable is FloatVariable) DrawNumericCondition(condition, true);
            else if (variable is TextVariable) DrawStringCondition(condition);
            else
                EditorGUILayout.HelpBox($"Variable type '{variable.GetType().Name}' is not supported.",
                    MessageType.Warning);
        }

        private void DrawBoolCondition(BranchCondition condition)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Condition");

            EditorGUI.BeginChangeCheck();
            var compOptions = new[] { "Equals", "Not Equals" };
            int currentComp = condition.Comparison == ComparisonType.NotEquals ? 1 : 0;
            int newComp = EditorGUILayout.Popup(currentComp, compOptions, GUILayout.Width(90));
            bool currentBool = BranchConditionHelper.GetBool(condition);
            bool newBool = EditorGUILayout.Toggle(currentBool);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change Bool Condition");
                BranchConditionHelper.SetComparison(condition,
                    newComp == 0 ? ComparisonType.Equals : ComparisonType.NotEquals);
                BranchConditionHelper.SetBool(condition, newBool);
                MarkDirtyAndUpdateEdge(_selectedTransition);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawNumericCondition(BranchCondition condition, bool isFloat)
        {
            EditorGUI.BeginChangeCheck();

            var compNames = new[] { "==", "!=", ">", "<", ">=", "<=" };
            int currentComp = (int)condition.Comparison;
            int newComp = EditorGUILayout.Popup("Comparison", currentComp, compNames);

            float newFloatVal = BranchConditionHelper.GetFloat(condition);
            int newIntVal = BranchConditionHelper.GetInt(condition);

            if (isFloat)
                newFloatVal = EditorGUILayout.FloatField("Value", newFloatVal);
            else
                newIntVal = EditorGUILayout.IntField("Value", newIntVal);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change Numeric Condition");
                BranchConditionHelper.SetComparison(condition, (ComparisonType)newComp);
                if (isFloat) BranchConditionHelper.SetFloat(condition, newFloatVal);
                else BranchConditionHelper.SetInt(condition, newIntVal);
                MarkDirtyAndUpdateEdge(_selectedTransition);
            }
        }

        private void DrawStringCondition(BranchCondition condition)
        {
            EditorGUI.BeginChangeCheck();

            var compOptions = new[] { "Equals", "Not Equals" };
            int currentComp = condition.Comparison == ComparisonType.NotEquals ? 1 : 0;
            int newComp = EditorGUILayout.Popup("Comparison", currentComp, compOptions);
            string currentStr = BranchConditionHelper.GetString(condition);
            string newStr = EditorGUILayout.TextField("Value", currentStr);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change String Condition");
                BranchConditionHelper.SetComparison(condition,
                    newComp == 0 ? ComparisonType.Equals : ComparisonType.NotEquals);
                BranchConditionHelper.SetString(condition, newStr);
                MarkDirtyAndUpdateEdge(_selectedTransition);
            }
        }

        private void DrawTargetStepPopup(StepTransition transition)
        {
            var steps = sequence.AllSteps;
            var names = new string[steps.Count + 1];
            names[0] = "(None \u2014 End Sequence)";
            int currentIndex = 0;

            for (int i = 0; i < steps.Count; i++)
            {
                var s = steps[i];
                names[i + 1] = s != null ? s.name : "(null)";
                if (s == transition.targetStep) currentIndex = i + 1;
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Target Step", currentIndex, names);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sequence, "Change Target Step");
                transition.targetStep = newIndex > 0 && newIndex <= steps.Count
                    ? steps[newIndex - 1]
                    : null;
                MarkDirtyAndUpdateEdge(transition);
            }
        }

        private void MarkDirtyAndUpdateEdge(StepTransition transition)
        {
            EditorUtility.SetDirty(sequence);
            _graphView?.UpdateEdgeForTransition(transition);
        }

        #endregion

        #region Runtime Updates

        private void OnUndoRedo()
        {
            if (_graphView != null && sequence != null)
            {
                _graphView.SavePositions();
                _graphView.RefreshGraph();
            }
        }

        private void OnEditorUpdate()
        {
            if (sequence == null || _graphView == null) return;

            bool isPlayingAndStarted = Application.isPlaying && sequence.Started;

            // Runtime edge coloring
            if (isPlayingAndStarted)
            {
                _graphView.UpdateRuntimeEdgeColors();
                Repaint();
            }
            else if (_wasPlayingAndStarted)
            {
                _graphView.ResetEdgeColors();
                Repaint();
            }

            _wasPlayingAndStarted = isPlayingAndStarted;

            // Runtime step highlight
            if (!Application.isPlaying) return;
            if (sequence.CurrentStep == _lastCurrentStep) return;
            _lastCurrentStep = sequence.CurrentStep;
            _graphView.UpdateRuntimeHighlight(_lastCurrentStep);
            Repaint();
        }

        #endregion
    }
}
