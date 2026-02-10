using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// GraphView that visualizes a BranchingSequence as a node graph with steps and transitions.
    /// </summary>
    public class BranchingSequenceGraphView : GraphView
    {
        private readonly BranchingSequence _sequence;
        private readonly Dictionary<Step, StepNode> _nodeMap = new();

        private static readonly Color UnconditionalEdgeColor = new(0.5f, 0.85f, 0.5f);
        private static readonly Color ConditionalEdgeColor = new(0.4f, 0.65f, 1f);
        private static readonly Color ConditionMetColor = new(0.2f, 0.85f, 0.2f);
        private static readonly Color ConditionNotMetColor = new(0.85f, 0.25f, 0.25f);

        /// <summary>
        /// Fired when a transition edge is selected. Provides the transition and its source step.
        /// </summary>
        public Action<StepTransition, Step> OnTransitionSelected;

        /// <summary>
        /// Fired when a step node is selected.
        /// </summary>
        public Action<Step> OnStepSelected;

        /// <summary>
        /// Fired when the selection is cleared.
        /// </summary>
        public Action OnSelectionCleared;

        private Edge _lastSelectedEdge;
        private StepNode _lastSelectedNode;

        public BranchingSequenceGraphView(BranchingSequence sequence)
        {
            _sequence = sequence;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            style.flexGrow = 1;

            graphViewChanged = OnGraphViewChanged;

            schedule.Execute(PollSelection).Every(100);
        }

        /// <summary>
        /// Returns compatible ports for edge creation. Only allows output-to-input on different nodes.
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();
            ports.ForEach(port =>
            {
                if (port.direction != startPort.direction && port.node != startPort.node)
                    compatible.Add(port);
            });
            return compatible;
        }

        /// <summary>
        /// Rebuilds the entire graph from the BranchingSequence data.
        /// </summary>
        public void RefreshGraph()
        {
            ClearGraph();
            BuildNodes();
            BuildEdges();
        }

        /// <summary>
        /// Runs a BFS-based auto-layout algorithm to arrange nodes in layers.
        /// </summary>
        public void AutoLayout()
        {
            if (_sequence.AllSteps == null || _sequence.AllSteps.Count == 0) return;

            const float xSpacing = 280f;
            const float ySpacing = 120f;

            var layers = AssignLayers();
            var layerGroups = GroupByLayer(layers);

            Undo.RecordObject(_sequence, "Auto Layout");

            foreach (var kvp in layerGroups)
            {
                float totalHeight = (kvp.Value.Count - 1) * ySpacing;
                float startY = -totalHeight / 2f;

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var step = kvp.Value[i];
                    var pos = new Vector2(kvp.Key * xSpacing, startY + i * ySpacing);
                    _sequence.SetStepPosition(step, pos);

                    if (_nodeMap.TryGetValue(step, out var node))
                        node.SetPosition(new Rect(pos, node.GetPosition().size));
                }
            }

            EditorUtility.SetDirty(_sequence);
        }

        /// <summary>
        /// Saves all current node positions back to the BranchingSequence asset.
        /// </summary>
        public void SavePositions()
        {
            if (_sequence == null) return;

            foreach (var kvp in _nodeMap)
            {
                var pos = kvp.Value.GetPosition().position;
                _sequence.SetStepPosition(kvp.Key, pos);
            }

            EditorUtility.SetDirty(_sequence);
        }

        /// <summary>
        /// Updates the runtime highlight to show the currently executing step.
        /// </summary>
        public void UpdateRuntimeHighlight(Step currentStep)
        {
            foreach (var kvp in _nodeMap)
                kvp.Value.SetHighlight(kvp.Key == currentStep);
        }

        /// <summary>
        /// Evaluates all transition conditions and colors edges green (met) or red (not met).
        /// </summary>
        public void UpdateRuntimeEdgeColors()
        {
            graphElements.ForEach(element =>
            {
                if (element is not Edge edge || edge.userData is not EdgeData data) return;

                bool conditionMet;
                try
                {
                    conditionMet = data.Transition.Evaluate();
                }
                catch
                {
                    conditionMet = false;
                }

                var color = conditionMet ? ConditionMetColor : ConditionNotMetColor;
                ApplyEdgeColor(edge, color);
            });
        }

        /// <summary>
        /// Resets edge colors to the default edit-mode scheme (green=unconditional, blue=conditional).
        /// </summary>
        public void ResetEdgeColors()
        {
            graphElements.ForEach(element =>
            {
                if (element is not Edge edge || edge.userData is not EdgeData data) return;

                bool isUnconditional = data.Transition.condition?.Variable == null;
                var color = isUnconditional ? UnconditionalEdgeColor : ConditionalEdgeColor;
                ApplyEdgeColor(edge, color);
            });
        }

        /// <summary>
        /// Updates the tooltip on a specific transition's edge after editing.
        /// </summary>
        public void UpdateEdgeForTransition(StepTransition transition)
        {
            graphElements.ForEach(element =>
            {
                if (element is not Edge edge || edge.userData is not EdgeData data) return;
                if (data.Transition != transition) return;

                edge.tooltip = BuildTransitionTooltip(transition);

                if (!Application.isPlaying)
                {
                    bool isUnconditional = transition.condition?.Variable == null;
                    var color = isUnconditional ? UnconditionalEdgeColor : ConditionalEdgeColor;
                    ApplyEdgeColor(edge, color);
                }
            });
        }

        #region Graph Construction

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            HandleMovedElements(change);
            HandleEdgeCreation(change);
            HandleElementRemoval(change);
            return change;
        }

        private void HandleMovedElements(GraphViewChange change)
        {
            if (change.movedElements == null) return;

            Undo.RecordObject(_sequence, "Move Step Node");
            foreach (var element in change.movedElements)
            {
                if (element is StepNode stepNode)
                {
                    var pos = stepNode.GetPosition().position;
                    _sequence.SetStepPosition(stepNode.Step, pos);
                }
            }

            EditorUtility.SetDirty(_sequence);
        }

        private void HandleEdgeCreation(GraphViewChange change)
        {
            if (change.edgesToCreate == null) return;

            foreach (var edge in change.edgesToCreate)
            {
                if (edge.output.node is not StepNode fromNode || edge.input.node is not StepNode toNode)
                    continue;

                Undo.RecordObject(_sequence, "Create Transition");

                var group = FindOrCreateTransitionGroup(fromNode.Step);
                var transition = new StepTransition { targetStep = toNode.Step };
                group.transitions.Add(transition);

                edge.userData = new EdgeData { FromStep = fromNode.Step, Transition = transition };

                bool isUnconditional = transition.condition?.Variable == null;
                ScheduleEdgeColorUpdate(edge, isUnconditional);

                edge.tooltip = BuildTransitionTooltip(transition);

                EditorUtility.SetDirty(_sequence);
            }
        }

        private void HandleElementRemoval(GraphViewChange change)
        {
            if (change.elementsToRemove == null) return;

            foreach (var element in change.elementsToRemove)
            {
                if (element is not Edge edge || edge.userData is not EdgeData edgeData) continue;

                Undo.RecordObject(_sequence, "Remove Transition");

                foreach (var group in _sequence.TransitionGroups)
                {
                    if (group.fromStep != edgeData.FromStep) continue;
                    group.transitions.Remove(edgeData.Transition);
                    break;
                }

                EditorUtility.SetDirty(_sequence);
            }
        }

        private void ClearGraph()
        {
            _nodeMap.Clear();
            _lastSelectedEdge = null;
            _lastSelectedNode = null;
            DeleteElements(graphElements.ToList());
        }

        private void BuildNodes()
        {
            if (_sequence.AllSteps == null) return;

            bool hasAnyPosition = false;

            foreach (var step in _sequence.AllSteps)
            {
                if (step == null) continue;

                bool isEntry = step == _sequence.EntryStep;
                bool isCurrent = Application.isPlaying && step == _sequence.CurrentStep;

                var node = new StepNode(step, isEntry, isCurrent);

                var savedPos = _sequence.GetStepPosition(step);
                if (!float.IsNaN(savedPos.x))
                {
                    node.SetPosition(new Rect(savedPos, Vector2.zero));
                    hasAnyPosition = true;
                }

                AddElement(node);
                _nodeMap[step] = node;
            }

            if (!hasAnyPosition && _sequence.AllSteps.Count > 0)
                AutoLayout();
        }

        private void BuildEdges()
        {
            if (_sequence.TransitionGroups == null) return;

            foreach (var group in _sequence.TransitionGroups)
            {
                if (group?.fromStep == null) continue;
                if (!_nodeMap.TryGetValue(group.fromStep, out var fromNode)) continue;

                foreach (var transition in group.transitions)
                {
                    if (transition?.targetStep == null) continue;
                    if (!_nodeMap.TryGetValue(transition.targetStep, out var toNode)) continue;

                    var edge = new Edge
                    {
                        output = fromNode.OutputPort,
                        input = toNode.InputPort
                    };

                    edge.userData = new EdgeData { FromStep = group.fromStep, Transition = transition };

                    bool isUnconditional = transition.condition?.Variable == null;
                    ScheduleEdgeColorUpdate(edge, isUnconditional);

                    edge.tooltip = BuildTransitionTooltip(transition);

                    fromNode.OutputPort.Connect(edge);
                    toNode.InputPort.Connect(edge);
                    AddElement(edge);
                }
            }
        }

        #endregion

        #region Selection Tracking

        private void PollSelection()
        {
            Edge selectedEdge = null;
            StepNode selectedNode = null;

            foreach (var sel in selection)
            {
                if (sel is Edge edge) { selectedEdge = edge; break; }
                if (sel is StepNode node) { selectedNode = node; break; }
            }

            // Edges take priority over nodes
            if (selectedEdge != _lastSelectedEdge || selectedNode != _lastSelectedNode)
            {
                _lastSelectedEdge = selectedEdge;
                _lastSelectedNode = selectedNode;

                if (selectedEdge?.userData is EdgeData data)
                    OnTransitionSelected?.Invoke(data.Transition, data.FromStep);
                else if (selectedNode != null)
                    OnStepSelected?.Invoke(selectedNode.Step);
                else
                    OnSelectionCleared?.Invoke();
            }
        }

        #endregion

        #region Edge Helpers

        private static void ApplyEdgeColor(Edge edge, Color color)
        {
            if (edge.edgeControl == null) return;
            edge.edgeControl.inputColor = color;
            edge.edgeControl.outputColor = color;
        }

        private void ScheduleEdgeColorUpdate(Edge edge, bool isUnconditional)
        {
            var color = isUnconditional ? UnconditionalEdgeColor : ConditionalEdgeColor;
            edge.schedule.Execute(() => ApplyEdgeColor(edge, color));
        }

        private static string BuildTransitionTooltip(StepTransition transition)
        {
            if (transition.condition?.Variable == null)
            {
                return string.IsNullOrEmpty(transition.label)
                    ? "Default (unconditional)"
                    : transition.label;
            }

            var c = transition.condition;
            string prefix = string.IsNullOrEmpty(transition.label) ? "" : $"{transition.label}: ";
            return $"{prefix}{c.Variable.name} {c.Comparison} ?";
        }

        #endregion

        #region Layout Helpers

        private StepTransitionGroup FindOrCreateTransitionGroup(Step step)
        {
            foreach (var group in _sequence.TransitionGroups)
            {
                if (group.fromStep == step) return group;
            }

            var newGroup = new StepTransitionGroup(step);
            _sequence.TransitionGroups.Add(newGroup);
            return newGroup;
        }

        private Dictionary<Step, int> AssignLayers()
        {
            var layers = new Dictionary<Step, int>();
            var visited = new HashSet<Step>();
            var queue = new Queue<Step>();

            Step startStep = _sequence.EntryStep;
            if (startStep == null && _sequence.AllSteps.Count > 0)
                startStep = _sequence.AllSteps.FirstOrDefault(s => s != null);

            if (startStep != null)
            {
                queue.Enqueue(startStep);
                layers[startStep] = 0;
                visited.Add(startStep);
            }

            while (queue.Count > 0)
            {
                var step = queue.Dequeue();
                int layer = layers[step];

                foreach (var group in _sequence.TransitionGroups)
                {
                    if (group?.fromStep != step) continue;
                    foreach (var t in group.transitions)
                    {
                        if (t?.targetStep == null || visited.Contains(t.targetStep)) continue;
                        layers[t.targetStep] = layer + 1;
                        visited.Add(t.targetStep);
                        queue.Enqueue(t.targetStep);
                    }
                }
            }

            int maxLayer = layers.Count > 0 ? layers.Values.Max() + 1 : 0;
            foreach (var step in _sequence.AllSteps)
            {
                if (step == null || layers.ContainsKey(step)) continue;
                layers[step] = maxLayer++;
            }

            return layers;
        }

        private static SortedDictionary<int, List<Step>> GroupByLayer(Dictionary<Step, int> layers)
        {
            var groups = new SortedDictionary<int, List<Step>>();
            foreach (var kvp in layers)
            {
                if (!groups.ContainsKey(kvp.Value))
                    groups[kvp.Value] = new List<Step>();
                groups[kvp.Value].Add(kvp.Key);
            }

            return groups;
        }

        #endregion

        internal class EdgeData
        {
            public Step FromStep;
            public StepTransition Transition;
        }
    }
}
