
using UnityEditor;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Custom editor for SequenceBehaviour with conditional property display.
    /// </summary>
    //TODO: rewrite this from scratch it's broken as is
    [CustomEditor(typeof(SequenceBehaviour))]
    public class SequenceBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var sequence = (SequenceBehaviour)target;
            if (sequence.StarOnAwake)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"));
            }

            else
            {
                if (GUILayout.Button("Update Sequence"))
                {
                }
            }

        }

    }
}