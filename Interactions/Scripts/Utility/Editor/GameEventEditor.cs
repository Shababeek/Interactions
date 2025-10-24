using Shababeek.Utilities;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for GameEvent that adds a "Raise" button in Play mode.
    /// </summary>
    [CustomEditor(typeof(GameEvent),true)]
    public class GameEventEditor : Editor
    {
        /// <summary>
        /// Renders the custom inspector GUI with a Raise button.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying && GUILayout.Button("Raise"))
            {
                var @event = (GameEvent)target;
                @event.Raise();
            }
        }
    }
}