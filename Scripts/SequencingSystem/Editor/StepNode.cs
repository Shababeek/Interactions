using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Visual node representing a Step in the branching sequence graph.
    /// </summary>
    public class StepNode : Node
    {
        /// <summary>
        /// Gets the Step this node represents.
        /// </summary>
        public Step Step { get; }

        /// <summary>
        /// Gets the input port for incoming transitions.
        /// </summary>
        public Port InputPort { get; }

        /// <summary>
        /// Gets the output port for outgoing transitions.
        /// </summary>
        public Port OutputPort { get; }

        private static readonly Color EntryColor = new(0.18f, 0.5f, 0.22f);
        private static readonly Color HighlightColor = new(1f, 0.75f, 0.1f);
        private static readonly Color AudioInfoColor = new(0.65f, 0.65f, 0.65f);

        public StepNode(Step step, bool isEntry, bool isCurrent = false)
        {
            Step = step;
            title = GetDisplayName();

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            BuildInfoSection();

            if (isEntry) ApplyEntryStyle();
            if (isCurrent) SetHighlight(true);

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<MouseDownEvent>(OnDoubleClick);
        }

        /// <summary>
        /// Toggles the runtime highlight border on this node.
        /// </summary>
        public void SetHighlight(bool active)
        {
            var color = active ? HighlightColor : Color.clear;
            float width = active ? 3 : 0;

            style.borderBottomColor = new StyleColor(color);
            style.borderTopColor = new StyleColor(color);
            style.borderLeftColor = new StyleColor(color);
            style.borderRightColor = new StyleColor(color);
            style.borderBottomWidth = width;
            style.borderTopWidth = width;
            style.borderLeftWidth = width;
            style.borderRightWidth = width;
        }

        private string GetDisplayName()
        {
            if (Step == null) return "(null)";
            var stepName = Step.name;
            var underscoreIndex = stepName.IndexOf('_');
            return underscoreIndex >= 0 && underscoreIndex < stepName.Length - 1
                ? stepName.Substring(underscoreIndex + 1)
                : stepName;
        }

        private void ApplyEntryStyle()
        {
            titleContainer.style.backgroundColor = new StyleColor(EntryColor);

            var badge = new Label("ENTRY")
            {
                style =
                {
                    fontSize = 9,
                    color = new StyleColor(new Color(0.8f, 1f, 0.8f)),
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginLeft = 4,
                    marginRight = 4,
                    paddingLeft = 4,
                    paddingRight = 4,
                    backgroundColor = new StyleColor(new Color(0.1f, 0.35f, 0.12f)),
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3
                }
            };
            titleContainer.Add(badge);
        }

        private void BuildInfoSection()
        {
            if (Step == null) return;

            var so = new SerializedObject(Step);
            var audioClipProp = so.FindProperty("audioClip");
            var audioOnlyProp = so.FindProperty("audioOnly");

            if (audioClipProp?.objectReferenceValue == null) return;

            var clipName = audioClipProp.objectReferenceValue.name;
            var isAudioOnly = audioOnlyProp?.boolValue ?? false;

            var audioLabel = new Label(isAudioOnly ? $"\u266A {clipName} (auto)" : $"\u266A {clipName}")
            {
                style =
                {
                    color = new StyleColor(AudioInfoColor),
                    fontSize = 10,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingBottom = 4
                }
            };
            extensionContainer.Add(audioLabel);
        }

        private void OnDoubleClick(MouseDownEvent evt)
        {
            if (evt.clickCount != 2 || Step == null) return;
            EditorGUIUtility.PingObject(Step);
            Selection.activeObject = Step;
        }
    }
}
