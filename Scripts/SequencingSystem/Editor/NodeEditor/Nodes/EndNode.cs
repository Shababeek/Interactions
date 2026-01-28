using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Represents the end point of a sequence in the graph editor.
    /// </summary>
    public class EndNode : BaseSequenceNode
    {
        public Port InputPort => base.InputPort;

        public EndNode()
        {
            title = "■ End";

            // Style the node
            titleContainer.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f);

            // Add input port only
            base.InputPort = CreateInputPort("Complete");

            // Add description
            var description = new Label("Sequence ends here");
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
