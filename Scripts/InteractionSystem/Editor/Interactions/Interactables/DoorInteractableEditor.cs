using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for <see cref="DoorInteractable"/>. Adds "Simulate Open" / "Simulate Closed"
    /// buttons that pose the door in the editor (edit or play mode) so its swing can be previewed
    /// without entering VR, plus the standard rotary-lever settings and events.
    /// </summary>
    [CustomEditor(typeof(DoorInteractable))]
    [CanEditMultipleObjects]
    public class DoorInteractableEditor : ConstrainedInteractableEditor
    {
        private DoorInteractable _door;

        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _angleRangeProp;
        private SerializedProperty _projectionDistanceProp;
        private SerializedProperty _hingeOffsetProp;
        private SerializedProperty _startLockedProp;

        private SerializedProperty _onMovedProp;
        private SerializedProperty _onOpenedProp;
        private SerializedProperty _onClosedProp;
        private SerializedProperty _onUnlockedProp;
        private SerializedProperty _onLockedProp;

        private SerializedProperty _isLockedProp;
        private SerializedProperty _currentAngleProp;
        private SerializedProperty _currentNormalizedAngleProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _door = target as DoorInteractable;
            _rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            _angleRangeProp = serializedObject.FindProperty("angleRange");
            _projectionDistanceProp = serializedObject.FindProperty("projectionDistance");
            _hingeOffsetProp = serializedObject.FindProperty("hingeOffset");
            _startLockedProp = serializedObject.FindProperty("startLocked");

            _onMovedProp = serializedObject.FindProperty("onMoved");
            _onOpenedProp = serializedObject.FindProperty("onOpened");
            _onClosedProp = serializedObject.FindProperty("onClosed");
            _onUnlockedProp = serializedObject.FindProperty("onUnlocked");
            _onLockedProp = serializedObject.FindProperty("onLocked");

            _isLockedProp = serializedObject.FindProperty("isLocked");
            _currentAngleProp = serializedObject.FindProperty("currentAngle");
            _currentNormalizedAngleProp = serializedObject.FindProperty("currentNormalizedAngle");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Hinged door / cabinet door. Author it in the CLOSED pose and set Angle Range to " +
                "(0, openAngle) so 0 = closed. Starts latched when Start Locked is on; a Door Handle " +
                "releases the latch, then push/pull swings it.",
                MessageType.Info);
        }

        protected override void DrawImportantSettings()
        {
            EditorGUILayout.LabelField("Simulate", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Simulate Closed"))
                SimulateAll(0f);

            if (GUILayout.Button("Simulate Open"))
                SimulateAll(1f);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Door Settings", EditorStyles.boldLabel);

            base.DrawCustomProperties();

            SafePropertyField(_rotationAxisProp, "Rotation Axis", "The hinge axis the door swings around.");
            SafePropertyField(_hingeOffsetProp, new GUIContent("Hinge Offset",
                "Local offset from the door origin to the hinge it swings around. Drag the yellow " +
                "handle in the Scene view to place it on the door's edge."));
            SafePropertyField(_projectionDistanceProp, "Projection Distance",
                "Reference distance for angle calculation (affects sensitivity).");
            SafePropertyField(_startLockedProp, "Start Locked",
                "Whether the door begins latched shut and refuses to swing until unlocked.");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Swing Limits (0 = closed)", EditorStyles.boldLabel);
            SafePropertyField(_angleRangeProp, new GUIContent("Angle Range",
                "Swing limits in degrees. Keep x = 0 (closed); y is the fully-open angle."));
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Door Events", EditorStyles.boldLabel);
            SafePropertyField(_onMovedProp, new GUIContent("On Moved"));
            SafePropertyField(_onOpenedProp, new GUIContent("On Opened"));
            SafePropertyField(_onClosedProp, new GUIContent("On Closed"));
            SafePropertyField(_onUnlockedProp, new GUIContent("On Unlocked"));
            SafePropertyField(_onLockedProp, new GUIContent("On Locked"));
            EditorGUILayout.Space();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Door Debug", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            SafePropertyField(_isLockedProp, new GUIContent("Is Locked"));
            SafePropertyField(_currentAngleProp, new GUIContent("Current Angle"));
            SafePropertyField(_currentNormalizedAngleProp, new GUIContent("Normalized Angle"));
            EditorGUI.EndDisabledGroup();
        }

        private void OnSceneGUI()
        {
            if (_door == null) _door = target as DoorInteractable;
            if (_door == null || _door.InteractableObject == null) return;

            Transform io = _door.InteractableObject;
            Transform parent = io.parent;

            Vector3 restLocal = io.localPosition;
            Vector3 hingeLocal = restLocal + _door.HingeOffset;
            Vector3 hingeWorld = parent != null ? parent.TransformPoint(hingeLocal) : hingeLocal;

            // Hinge axis line through the pivot.
            var (axis, _) = _door.GetRotationAxis();
            float size = HandleUtility.GetHandleSize(hingeWorld);
            Handles.color = Color.yellow;
            Handles.DrawLine(hingeWorld - axis * size * 1.5f, hingeWorld + axis * size * 1.5f);
            Handles.SphereHandleCap(0, hingeWorld, Quaternion.identity, size * 0.1f, EventType.Repaint);
            Handles.Label(hingeWorld + Vector3.up * size * 0.2f, "Hinge");

            EditorGUI.BeginChangeCheck();
            Vector3 newWorld = Handles.PositionHandle(hingeWorld, io.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_door, "Move Door Hinge");
                Vector3 newLocal = parent != null ? parent.InverseTransformPoint(newWorld) : newWorld;
                _door.HingeOffset = newLocal - restLocal;
                EditorUtility.SetDirty(_door);
            }
        }

        private void SimulateAll(float normalized)
        {
            foreach (var obj in targets)
            {
                if (obj is not DoorInteractable door || door.InteractableObject == null) continue;

                Undo.RecordObject(door.InteractableObject, "Simulate Door");
                Undo.RecordObject(door, "Simulate Door");
                door.SetOpenAmount(normalized);
                EditorUtility.SetDirty(door.InteractableObject);
                EditorUtility.SetDirty(door);
            }
        }
    }
}
