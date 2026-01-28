using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Base class for all nodes in the sequence graph editor.
    /// </summary>
    public abstract class BaseSequenceNode : Node
    {
        protected Port InputPort;
        protected Port OutputPort;

        public Port GetInputPort() => InputPort;
        public Port GetOutputPort() => OutputPort;

        protected Port CreateInputPort(string portName = "In", Port.Capacity capacity = Port.Capacity.Single)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, typeof(bool));
            port.portName = portName;
            port.portColor = new Color(0.2f, 0.8f, 0.4f);
            inputContainer.Add(port);
            return port;
        }

        protected Port CreateOutputPort(string portName = "Out", Port.Capacity capacity = Port.Capacity.Single)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, typeof(bool));
            port.portName = portName;
            port.portColor = new Color(0.2f, 0.8f, 0.4f);
            outputContainer.Add(port);
            return port;
        }

        protected void AddTitleLabel(string text, Color color)
        {
            var label = new Label(text);
            label.style.fontSize = 12;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = color;
            label.style.paddingLeft = 8;
            label.style.paddingTop = 4;
            titleContainer.Insert(0, label);
        }
    }
}
