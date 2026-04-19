using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Socketable))]
    [CanEditMultipleObjects]
    public class SocketableEditor : Editor
    {
        private SerializedProperty shouldReturnToParentProp;
        private SerializedProperty useSmoothReturnProp;
        private SerializedProperty returnDurationProp;

        private SerializedProperty indicatorProp;

        private SerializedProperty onSocketedProp;
        private SerializedProperty debugKeyProp;
        private SerializedProperty detectionRadiusProp;
        private SerializedProperty detectionOffsetProp;
        private SerializedProperty socketLayerMaskProp;
        private SerializedProperty categoryProp;
        private SerializedProperty matchModeProp;

        private SerializedProperty socketProp;
        private SerializedProperty isSocketedProp;
        
        private Socketable socketable;

        private void OnEnable()
        {
            socketable = (Socketable)target;

            shouldReturnToParentProp = serializedObject.FindProperty("shouldReturnToLastSocket");
            useSmoothReturnProp = serializedObject.FindProperty("useSmoothReturn");
            returnDurationProp = serializedObject.FindProperty("returnDuration");
            indicatorProp = serializedObject.FindProperty("indicator");

            onSocketedProp = serializedObject.FindProperty("onSocketed");
            debugKeyProp = serializedObject.FindProperty("debugKey");
            detectionRadiusProp = serializedObject.FindProperty("detectionRadius");
            detectionOffsetProp = serializedObject.FindProperty("detectionOffset");
            socketLayerMaskProp = serializedObject.FindProperty("socketLayerMask");
            categoryProp = serializedObject.FindProperty("category");
            matchModeProp = serializedObject.FindProperty("matchMode");

            socketProp = serializedObject.FindProperty("socket");
            isSocketedProp = serializedObject.FindProperty("isSocketed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Validation and Help (moved to beginning)
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(shouldReturnToParentProp,
                new GUIContent("Return to Parent",
                    "If true, the object will return to its original position when unsocketed"));
            if (shouldReturnToParentProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(useSmoothReturnProp,
                    new GUIContent("Use Smooth Return",
                        "If true, the object will smoothly animate back to its original position"));

                if (useSmoothReturnProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(returnDurationProp,
                        new GUIContent("Return Duration", "Duration of the smooth return animation in seconds"));
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.LabelField("Socket Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(indicatorProp,
                new GUIContent("Indicator", "Visual indicator to show socket position"));
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Socket Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(detectionRadiusProp,
                new GUIContent("Detection Radius", "Radius of the detection sphere for finding nearby sockets"));
            EditorGUILayout.PropertyField(detectionOffsetProp,
                new GUIContent("Detection Offset", "Local space offset for the detection sphere center"));
            EditorGUILayout.PropertyField(matchModeProp,
                new GUIContent("Match Mode", "LayerMask = legacy physics-layer filtering. Category = bitmask match via AbstractSocket.Accepts (recommended for grabbed objects)."));

            var mode = (SocketMatchMode)matchModeProp.enumValueIndex;
            EditorGUI.indentLevel++;
            if (mode == SocketMatchMode.LayerMask)
            {
                EditorGUILayout.HelpBox("LayerMask mode is legacy. When the grabbed object's layer changes (e.g. to Hand), detection can break. Switch to Category for layer-independent matching.", MessageType.Warning);
                EditorGUILayout.PropertyField(socketLayerMaskProp,
                    new GUIContent("Socket Layer Mask", "Legacy. Physics layers considered as sockets."));
            }
            else
            {
                EditorGUILayout.PropertyField(categoryProp,
                    new GUIContent("Category", "Bitmask matched against AbstractSocket.acceptedCategories via (a & b) != 0."));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onSocketedProp,
                new GUIContent("On Socketed", "Event triggered when object is socketed"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(debugKeyProp,
                new GUIContent("Debug Key", "Keyboard key to manually trigger socket/unsocket for debugging"));

            EditorGUILayout.LabelField("Status (Read-only)", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(socketProp, new GUIContent("Current Socket", "Currently detected socket"));
            EditorGUILayout.PropertyField(isSocketedProp,
                new GUIContent("Is Socketed", "Whether the object is currently socketed"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            DrawDebugSection();
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }
        

        private void DrawDebugSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug & Testing", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Force Return", GUILayout.Height(25)))
            {
                // Force the object to return to its original position
                if (Application.isPlaying)
                {
                    socketable.ForceReturn();
                    Debug.Log($"Forced immediate return for {socketable.name}");
                }
                else
                {
                    Debug.Log("Force Return can only be used in Play mode");
                }
            }
            
            if (GUILayout.Button("Smooth Return", GUILayout.Height(25)))
            {
                // Force smooth return to original position
                if (Application.isPlaying)
                {
                    socketable.ForceReturnWithTween();
                    Debug.Log($"Forced smooth return for {socketable.name}");
                }
                else
                {
                    Debug.Log("Smooth Return can only be used in Play mode");
                }
            }
            

            EditorGUILayout.EndHorizontal();

            if (socketable.IsSocketed)
            {
                EditorGUILayout.HelpBox(
                    $"Currently socketed to: {(socketable.CurrentSocket != null ? socketable.CurrentSocket.name : "Unknown")}",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Not currently socketed", MessageType.Info);
            }

            if (Application.isPlaying)
            {
                var socketableComponent = socketable.GetComponent<Socketable>();
                if (socketableComponent != null && socketableComponent.IsReturning)
                {
                    EditorGUILayout.HelpBox("Currently returning to original position...", MessageType.Warning);
                }
            }
        }
    }
}