using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// GraphView for visualizing and editing Sequences as node graphs.
    /// </summary>
    public class SequenceGraphView : GraphView
    {
        private Sequence _sequence;
        private SequenceEditorWindow _editorWindow;
        private readonly Vector2 _defaultNodeSize = new(200, 150);

        public Sequence Sequence => _sequence;

        public SequenceGraphView(SequenceEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddStyles();

            graphViewChanged = OnGraphViewChanged;
        }

        private void AddStyles()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/_Shababeek/Hands/InteractionSystem/Scripts/SequencingSystem/Editor/NodeEditor/SequenceGraphStyles.uss");

            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
        }

        public void PopulateGraph(Sequence sequence)
        {
            _sequence = sequence;

            // Clear existing elements
            DeleteElements(graphElements.ToList());

            if (_sequence == null) return;

            // Create start node
            var startNode = CreateStartNode();
            AddElement(startNode);

            // Create step nodes
            StepNode previousNode = null;
            var stepNodes = new List<StepNode>();

            for (int i = 0; i < _sequence.Steps.Count; i++)
            {
                var step = _sequence.Steps[i];
                var stepNode = CreateStepNode(step, i);
                stepNodes.Add(stepNode);
                AddElement(stepNode);
            }

            // Create end node
            var endNode = CreateEndNode();
            AddElement(endNode);

            // Connect nodes
            if (stepNodes.Count > 0)
            {
                // Connect start to first step
                var startEdge = ConnectNodes(startNode.OutputPort, stepNodes[0].InputPort);
                AddElement(startEdge);

                // Connect steps in sequence
                for (int i = 0; i < stepNodes.Count - 1; i++)
                {
                    var edge = ConnectNodes(stepNodes[i].OutputPort, stepNodes[i + 1].InputPort);
                    AddElement(edge);
                }

                // Connect last step to end
                var endEdge = ConnectNodes(stepNodes[^1].OutputPort, endNode.InputPort);
                AddElement(endEdge);
            }
            else
            {
                // Connect start directly to end if no steps
                var edge = ConnectNodes(startNode.OutputPort, endNode.InputPort);
                AddElement(edge);
            }

            // Layout nodes
            LayoutNodes(startNode, stepNodes, endNode);
        }

        private StartNode CreateStartNode()
        {
            var node = new StartNode
            {
                title = "Start"
            };

            node.SetPosition(new Rect(100, 200, 150, 100));
            return node;
        }

        private EndNode CreateEndNode()
        {
            var node = new EndNode
            {
                title = "End"
            };

            return node;
        }

        private StepNode CreateStepNode(Step step, int index)
        {
            var node = new StepNode(step, index, this);
            return node;
        }

        private Edge ConnectNodes(Port output, Port input)
        {
            var edge = new Edge
            {
                output = output,
                input = input
            };

            edge.input.Connect(edge);
            edge.output.Connect(edge);

            return edge;
        }

        private void LayoutNodes(StartNode startNode, List<StepNode> stepNodes, EndNode endNode)
        {
            const float startX = 100;
            const float startY = 200;
            const float horizontalSpacing = 280;

            startNode.SetPosition(new Rect(startX, startY, 150, 100));

            float currentX = startX + horizontalSpacing;

            foreach (var stepNode in stepNodes)
            {
                // Check if step has saved position
                var savedPos = GetSavedNodePosition(stepNode.Step);
                if (savedPos.HasValue)
                {
                    stepNode.SetPosition(new Rect(savedPos.Value, _defaultNodeSize));
                }
                else
                {
                    stepNode.SetPosition(new Rect(currentX, startY, _defaultNodeSize.x, _defaultNodeSize.y));
                }
                currentX += horizontalSpacing;
            }

            endNode.SetPosition(new Rect(currentX, startY, 150, 100));
        }

        private Vector2? GetSavedNodePosition(Step step)
        {
            // Check if step has saved editor position in metadata
            var path = AssetDatabase.GetAssetPath(step);
            if (string.IsNullOrEmpty(path)) return null;

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null) return null;

            var userData = importer.userData;
            if (string.IsNullOrEmpty(userData)) return null;

            // Parse position from userData (format: "stepName:x,y;...")
            var stepName = step.name;
            var entries = userData.Split(';');
            foreach (var entry in entries)
            {
                if (entry.StartsWith(stepName + ":"))
                {
                    var coords = entry.Substring(stepName.Length + 1).Split(',');
                    if (coords.Length == 2 &&
                        float.TryParse(coords[0], out float x) &&
                        float.TryParse(coords[1], out float y))
                    {
                        return new Vector2(x, y);
                    }
                }
            }

            return null;
        }

        public void SaveNodePositions()
        {
            if (_sequence == null) return;

            var positions = new List<string>();

            foreach (var element in graphElements)
            {
                if (element is StepNode stepNode)
                {
                    var pos = stepNode.GetPosition().position;
                    positions.Add($"{stepNode.Step.name}:{pos.x},{pos.y}");
                }
            }

            var path = AssetDatabase.GetAssetPath(_sequence);
            if (string.IsNullOrEmpty(path)) return;

            var importer = AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.userData = string.Join(";", positions);
                importer.SaveAndReimport();
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (element is StepNode)
                    {
                        // Mark for save
                        EditorUtility.SetDirty(_sequence);
                    }
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is StepNode stepNode)
                    {
                        RemoveStep(stepNode.Step);
                    }
                    else if (element is Edge edge)
                    {
                        // Handle edge removal - reorder steps if needed
                    }
                }
            }

            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public void AddNewStep()
        {
            if (_sequence == null) return;

            var path = AssetDatabase.GetAssetPath(_sequence);
            var step = ScriptableObject.CreateInstance<Step>();
            var index = _sequence.Steps?.Count ?? 0;
            step.name = $"{_sequence.name}-{index}_NewStep";

            if (_sequence.Steps == null)
            {
                _sequence.Init();
            }
            _sequence.Steps.Add(step);

            AssetDatabase.AddObjectToAsset(step, path);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();

            PopulateGraph(_sequence);
        }

        private void RemoveStep(Step step)
        {
            if (_sequence == null || step == null) return;

            _sequence.Steps.Remove(step);
            AssetDatabase.RemoveObjectFromAsset(step);
            AssetDatabase.SaveAssets();

            // Rename remaining steps to maintain order
            for (int i = 0; i < _sequence.Steps.Count; i++)
            {
                var s = _sequence.Steps[i];
                var semiIndex = s.name.IndexOf('_');
                if (semiIndex > 0)
                {
                    s.name = $"{_sequence.name}-{i}_{s.name.Substring(semiIndex + 1)}";
                }
            }

            var assetPath = AssetDatabase.GetAssetPath(_sequence);
            AssetDatabase.ImportAsset(assetPath);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Add Step", _ => AddNewStep());
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
        }
    }
}
