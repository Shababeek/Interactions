using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Represents the start point of a sequence in the graph editor.
    /// </summary>
    public class StartNode : BaseSequenceNode
    {
        public Port OutputPort => base.OutputPort;

        public StartNode()
        {
            title = "▶ Start";

            // Style the node
            titleContainer.style.backgroundColor = new Color(0.2f, 0.6f, 0.3f);

            // Add output port only
            base.OutputPort = CreateOutputPort("Begin");

            // Add description
            var description = new Label("Sequence starts here");
            description.style.fontSize = 10;
            description.style.color = new Color(0.7f, 0.7f, 0.7f);
            description.style.paddingLeft = 8;
            description.style.paddingBottom = 8;
            mainContainer.Add(description);

            RefreshExpandedState();
            RefreshPorts();

            // Disable deletion
            capabilities &= ~Capabilities.Deletable;
        }
    }
}
