using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Core.Editor
{
    [CustomEditor(typeof(VRInteractionZoneVisualizer))]
    public class VRInteractionZoneVisualizerEditor : UnityEditor.Editor
    {
        #region Constants
        
        // Zone colors (when selected)
        private static readonly Color optimalZoneColor = new Color(0f, 1f, 0f, 0.7f);
        private static readonly Color extendedZoneColor = new Color(1f, 1f, 0f, 0.65f);
        private static readonly Color maximumZoneColor = new Color(1f, 0.5f, 0f, 0.6f);
        private static readonly Color deadZoneColor = new Color(1f, 0f, 0f, 0.9f);
        private static readonly Color playAreaColor = new Color(0f, 1f, 1f, 0.15f); 
        private static readonly Color playAreaOutlineColor = Color.cyan;
        private static readonly Color headReferenceColor = Color.cyan;
        private static readonly Color referenceLinesColor = Color.white;
        private static readonly Color playerBodyColor = new Color(0f, 0.5f, 1f, 0.3f);
        private static readonly Color playerBodyOutlineColor = new Color(0f, 0.5f, 1f, 0.8f);
        private static readonly Color directionArrowColor = new Color(1f, 0.8f, 0f, 0.7f);
        
        // Legend colors
        private static readonly Color legendOptimalColor = new Color(0f, 1f, 0f, 1f);
        private static readonly Color legendExtendedColor = new Color(1f, 1f, 0f, 1f);
        private static readonly Color legendMaximumColor = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color legendDeadZoneColor = new Color(1f, 0f, 0f, 1f);
        private static readonly Color legendPlayAreaColor = new Color(0f, 1f, 1f, 1f);
        
        // GUI constants
        private const float colorBoxSize = 16f;
        private const float legendItemSpacing = 4f;
        private const float headIndicatorSize = 0.05f;
        private const float labelOffsetAboveHead = 0.3f;
        private const float playAreaLabelOffsetAboveFloor = 0.1f;
        
        // Player visualization constants
        private const int cylinderSegments = 20;
        private const float directionArrowLength = 0.5f;
        private const float directionArrowHeadSize = 0.15f;
        private const float directionArrowHeadAngle = 25f;
        
        // Opacity multiplier for unselected state
        private const float unselectedOpacityMultiplier = 0.6f;
        
        #endregion
        
        private SerializedProperty vrModeProp;
        private SerializedProperty playerRadiusProp;
        private SerializedProperty playAreaWidthProp;
        private SerializedProperty playAreaDepthProp;
        private SerializedProperty seatedOptimalMinProp;
        private SerializedProperty seatedOptimalMaxProp;
        private SerializedProperty seatedExtendedMaxProp;
        private SerializedProperty seatedMaximumMaxProp;
        private SerializedProperty roomScaleAtEdgeMaxProp;
        private SerializedProperty roomScaleExtendedMaxProp;
        private SerializedProperty roomScaleMaximumMaxProp;
        private SerializedProperty heightMinPercentProp;
        private SerializedProperty heightMaxPercentProp;
        private SerializedProperty optimalHeightMinPercentProp;
        private SerializedProperty optimalHeightMaxPercentProp;
        private SerializedProperty showOptimalZoneProp;
        private SerializedProperty showExtendedZoneProp;
        private SerializedProperty showMaximumZoneProp;
        private SerializedProperty showDeadZoneProp;
        private SerializedProperty deadZoneRadiusProp;
        private SerializedProperty showPlayAreaBoundsProp;
        private SerializedProperty showPlayerRepresentationProp;
        private SerializedProperty arcSegmentsProp;
        private SerializedProperty seatedArcAngleProp;
        
        private void OnEnable()
        {
            vrModeProp = serializedObject.FindProperty("vrMode");
            playerRadiusProp = serializedObject.FindProperty("playerRadius");
            playAreaWidthProp = serializedObject.FindProperty("playAreaWidth");
            playAreaDepthProp = serializedObject.FindProperty("playAreaDepth");
            seatedOptimalMinProp = serializedObject.FindProperty("seatedOptimalMin");
            seatedOptimalMaxProp = serializedObject.FindProperty("seatedOptimalMax");
            seatedExtendedMaxProp = serializedObject.FindProperty("seatedExtendedMax");
            seatedMaximumMaxProp = serializedObject.FindProperty("seatedMaximumMax");
            roomScaleAtEdgeMaxProp = serializedObject.FindProperty("roomScaleAtEdgeMax");
            roomScaleExtendedMaxProp = serializedObject.FindProperty("roomScaleExtendedMax");
            roomScaleMaximumMaxProp = serializedObject.FindProperty("roomScaleMaximumMax");
            heightMinPercentProp = serializedObject.FindProperty("heightMinPercent");
            heightMaxPercentProp = serializedObject.FindProperty("heightMaxPercent");
            optimalHeightMinPercentProp = serializedObject.FindProperty("optimalHeightMinPercent");
            optimalHeightMaxPercentProp = serializedObject.FindProperty("optimalHeightMaxPercent");
            showOptimalZoneProp = serializedObject.FindProperty("showOptimalZone");
            showExtendedZoneProp = serializedObject.FindProperty("showExtendedZone");
            showMaximumZoneProp = serializedObject.FindProperty("showMaximumZone");
            showDeadZoneProp = serializedObject.FindProperty("showDeadZone");
            deadZoneRadiusProp = serializedObject.FindProperty("deadZoneRadius");
            showPlayAreaBoundsProp = serializedObject.FindProperty("showPlayAreaBounds");
            showPlayerRepresentationProp = serializedObject.FindProperty("showPlayerRepresentation");
            arcSegmentsProp = serializedObject.FindProperty("arcSegments");
            seatedArcAngleProp = serializedObject.FindProperty("seatedArcAngle");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var visualizer = (VRInteractionZoneVisualizer)target;
            var previousMode = (VRMode)vrModeProp.enumValueIndex;
            
            // VR Mode
            EditorGUILayout.LabelField("VR Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(vrModeProp);
            
            EditorGUILayout.Space();
            
            // Player Representation
            EditorGUILayout.LabelField("Player Representation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playerRadiusProp);
            
            EditorGUILayout.Space();
            
            // Mode-specific fields
            if ((VRMode)vrModeProp.enumValueIndex == VRMode.RoomScale)
            {
                DrawRoomScaleFields();
            }
            else
            {
                DrawSeatedFields();
            }
            
            // Height Ranges (common to both modes)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Height Ranges (% of player height)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMinPercentProp);
            EditorGUILayout.PropertyField(heightMaxPercentProp);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optimal Height Range (% of player height)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(optimalHeightMinPercentProp);
            EditorGUILayout.PropertyField(optimalHeightMaxPercentProp);
            
            // Visualization Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visualization Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showOptimalZoneProp);
            EditorGUILayout.PropertyField(showExtendedZoneProp);
            EditorGUILayout.PropertyField(showMaximumZoneProp);
            EditorGUILayout.PropertyField(showDeadZoneProp);
            EditorGUILayout.PropertyField(deadZoneRadiusProp);
            
            if ((VRMode)vrModeProp.enumValueIndex == VRMode.RoomScale)
            {
                EditorGUILayout.PropertyField(showPlayAreaBoundsProp);
            }
            
            EditorGUILayout.PropertyField(showPlayerRepresentationProp);
            
            // Arc Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Arc Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(arcSegmentsProp);
            
            if ((VRMode)vrModeProp.enumValueIndex == VRMode.Seated)
            {
                EditorGUILayout.PropertyField(seatedArcAngleProp);
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Add helpful info box
            EditorGUILayout.Space();
            string helpText = (VRMode)vrModeProp.enumValueIndex == VRMode.RoomScale
                ? "Room-scale: Player can walk within the play area. Objects can be placed inside the bounds or at various reach distances from the edges."
                : "Seated: Player is stationary. Objects should be placed within comfortable forward reach distance.";
            
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            
            // Show calculated heights
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Calculated Heights", EditorStyles.boldLabel);
            float playerHeight = visualizer.GetPlayerHeight();
            EditorGUILayout.LabelField($"Player Height: {playerHeight:F2}m");
            EditorGUILayout.LabelField($"Player Radius: {visualizer.PlayerRadius:F2}m");
            EditorGUILayout.LabelField($"Height Min: {visualizer.GetHeightFromPercent(visualizer.HeightMinPercent):F2}m ({visualizer.HeightMinPercent:F0}%)");
            EditorGUILayout.LabelField($"Height Max: {visualizer.GetHeightFromPercent(visualizer.HeightMaxPercent):F2}m ({visualizer.HeightMaxPercent:F0}%)");
            EditorGUILayout.LabelField($"Optimal Min: {visualizer.GetHeightFromPercent(visualizer.OptimalHeightMinPercent):F2}m ({visualizer.OptimalHeightMinPercent:F0}%)");
            EditorGUILayout.LabelField($"Optimal Max: {visualizer.GetHeightFromPercent(visualizer.OptimalHeightMaxPercent):F2}m ({visualizer.OptimalHeightMaxPercent:F0}%)");
            
            // Draw legend
            EditorGUILayout.Space();
            DrawInspectorLegend(visualizer);
            
            // Add button to apply preset
            EditorGUILayout.Space();
            if (GUILayout.Button("Apply VR Mode Preset", GUILayout.Height(30)))
            {
                Undo.RecordObject(visualizer, "Apply VR Mode Preset");
                visualizer.ApplyVRModePreset();
                EditorUtility.SetDirty(visualizer);
            }
            
            // Auto-apply preset when mode changes
            var currentMode = (VRMode)vrModeProp.enumValueIndex;
            if (previousMode != currentMode)
            {
                if (EditorUtility.DisplayDialog(
                    "Apply VR Mode Preset?",
                    $"Do you want to apply the default preset values for {currentMode} mode?",
                    "Yes", "No"))
                {
                    Undo.RecordObject(visualizer, "Apply VR Mode Preset");
                    visualizer.ApplyVRModePreset();
                    EditorUtility.SetDirty(visualizer);
                }
            }
        }
        
        private void DrawRoomScaleFields()
        {
            EditorGUILayout.LabelField("Room-Scale Play Area", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playAreaWidthProp, new GUIContent("Play Area Width"));
            EditorGUILayout.PropertyField(playAreaDepthProp, new GUIContent("Play Area Depth"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Distance Zones - Room-Scale (from play area edge)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(roomScaleAtEdgeMaxProp, new GUIContent("At Edge Max"));
            EditorGUILayout.PropertyField(roomScaleExtendedMaxProp, new GUIContent("Extended Max"));
            EditorGUILayout.PropertyField(roomScaleMaximumMaxProp, new GUIContent("Maximum Max"));
        }
        
        private void DrawSeatedFields()
        {
            EditorGUILayout.LabelField("Distance Zones - Seated (forward from head)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(seatedOptimalMinProp, new GUIContent("Optimal Min"));
            EditorGUILayout.PropertyField(seatedOptimalMaxProp, new GUIContent("Optimal Max"));
            EditorGUILayout.PropertyField(seatedExtendedMaxProp, new GUIContent("Extended Max"));
            EditorGUILayout.PropertyField(seatedMaximumMaxProp, new GUIContent("Maximum Max"));
        }
        
        private void DrawInspectorLegend(VRInteractionZoneVisualizer visualizer)
        {
            EditorGUILayout.LabelField("Interaction Zones Legend", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (visualizer.VrMode == VRMode.Seated)
            {
                DrawSeatedLegend(visualizer);
            }
            else
            {
                DrawRoomScaleLegend(visualizer);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSeatedLegend(VRInteractionZoneVisualizer visualizer)
        {
            if (visualizer.ShowOptimalZone)
            {
                DrawLegendItem(legendOptimalColor, "Optimal Zone", 
                    $"{visualizer.SeatedOptimalMin:F1}m - {visualizer.SeatedOptimalMax:F1}m");
            }
            
            if (visualizer.ShowExtendedZone)
            {
                DrawLegendItem(legendExtendedColor, "Extended Reach", 
                    $"{visualizer.SeatedOptimalMax:F1}m - {visualizer.SeatedExtendedMax:F1}m");
            }
            
            if (visualizer.ShowMaximumZone)
            {
                DrawLegendItem(legendMaximumColor, "Maximum Reach", 
                    $"{visualizer.SeatedExtendedMax:F1}m - {visualizer.SeatedMaximumMax:F1}m");
            }
            
            if (visualizer.ShowDeadZone)
            {
                DrawLegendItem(legendDeadZoneColor, "Dead Zone", 
                    $"< {visualizer.DeadZoneRadius:F2}m (too close)");
            }
        }
        
        private void DrawRoomScaleLegend(VRInteractionZoneVisualizer visualizer)
        {
            if (visualizer.ShowPlayAreaBounds)
            {
                DrawLegendItem(legendPlayAreaColor, "Play Area (Walkable)", 
                    $"{visualizer.PlayAreaWidth:F1}m × {visualizer.PlayAreaDepth:F1}m");
            }
            
            if (visualizer.ShowOptimalZone)
            {
                DrawLegendItem(legendOptimalColor, "At Edge", 
                    $"0m - {visualizer.RoomScaleAtEdgeMax:F1}m from play area edge");
            }
            
            if (visualizer.ShowExtendedZone)
            {
                DrawLegendItem(legendExtendedColor, "Extended Reach", 
                    $"{visualizer.RoomScaleAtEdgeMax:F1}m - {visualizer.RoomScaleExtendedMax:F1}m from edge");
            }
            
            if (visualizer.ShowMaximumZone)
            {
                DrawLegendItem(legendMaximumColor, "Maximum Reach", 
                    $"{visualizer.RoomScaleExtendedMax:F1}m - {visualizer.RoomScaleMaximumMax:F1}m from edge");
            }
            
            if (visualizer.ShowDeadZone)
            {
                DrawLegendItem(legendDeadZoneColor, "Dead Zone", 
                    $"< {visualizer.DeadZoneRadius:F2}m around head");
            }
        }
        
        private void DrawLegendItem(Color color, string label, string description)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Color box
            Rect colorRect = GUILayoutUtility.GetRect(colorBoxSize, colorBoxSize, GUILayout.Width(colorBoxSize), GUILayout.Height(colorBoxSize));
            EditorGUI.DrawRect(colorRect, color);
            
            // Label and description
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(legendItemSpacing);
        }
        
        // Draw when selected (full opacity)
        private void OnSceneGUI()
        {
            var visualizer = (VRInteractionZoneVisualizer)target;
            
            if (visualizer.GetCameraRig() == null) return;
            
            DrawVisualization(visualizer, false);
        }
        
        // Draw when not selected (50% opacity)
        [DrawGizmo(GizmoType.NonSelected | GizmoType.NotInSelectionHierarchy)]
        static void DrawGizmosUnselected(VRInteractionZoneVisualizer visualizer, GizmoType gizmoType)
        {
            if (visualizer.GetCameraRig() == null) return;
            
            DrawVisualization(visualizer, true);
        }
        
        private static void DrawVisualization(VRInteractionZoneVisualizer visualizer, bool dimmed)
        {
            // Draw player representation first (so it's behind other elements)
            if (visualizer.ShowPlayerRepresentation)
            {
                DrawPlayerRepresentation(visualizer, dimmed);
            }
            
            if (visualizer.VrMode == VRMode.Seated)
            {
                DrawSeatedMode(visualizer, dimmed);
            }
            else
            {
                DrawRoomScaleMode(visualizer, dimmed);
            }
        }
        
        private static Color DimColor(Color color, bool dimmed)
        {
            if (!dimmed) return color;
            return new Color(color.r, color.g, color.b, color.a * unselectedOpacityMultiplier);
        }
        
        #region Player Representation
        
        private static void DrawPlayerRepresentation(VRInteractionZoneVisualizer visualizer, bool dimmed)
        {
            Vector3 rigPosition = visualizer.transform.position;
            float playerHeight = visualizer.GetPlayerHeight()*.8f;
            float playerRadius = visualizer.PlayerRadius;
            Vector3 forward = visualizer.transform.forward;
            
            // Draw cylinder body
            Handles.color = DimColor(playerBodyColor, dimmed);
            DrawCylinder(rigPosition, playerRadius, playerHeight);
            
            // Draw cylinder outline
            Handles.color = DimColor(playerBodyOutlineColor, dimmed);
            DrawCylinderWireframe(rigPosition, playerRadius, playerHeight);
            
            // Draw direction arrow on the ground
            DrawDirectionArrow(rigPosition, forward, dimmed);
        }
        
        private static void DrawCylinder(Vector3 position, float radius, float height)
        {
            // Draw filled discs at top and bottom
            Handles.DrawSolidDisc(position, Vector3.up, radius);
            Handles.DrawSolidDisc(position + Vector3.up * height, Vector3.up, radius);
            
            // Draw side faces (approximated with quads)
            Vector3[] bottomCircle = new Vector3[cylinderSegments];
            Vector3[] topCircle = new Vector3[cylinderSegments];
            
            for (int i = 0; i < cylinderSegments; i++)
            {
                float angle = (float)i / cylinderSegments * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                bottomCircle[i] = position + offset;
                topCircle[i] = position + offset + Vector3.up * height;
            }
            
            // Draw quad strips
            for (int i = 0; i < cylinderSegments; i++)
            {
                int next = (i + 1) % cylinderSegments;
                Vector3[] quad = new Vector3[]
                {
                    bottomCircle[i],
                    bottomCircle[next],
                    topCircle[next],
                    topCircle[i]
                };
                Handles.DrawSolidRectangleWithOutline(quad, Handles.color, Color.clear);
            }
        }
        
        private static void DrawCylinderWireframe(Vector3 position, float radius, float height)
        {
            // Draw circles at top and bottom
            Handles.DrawWireDisc(position, Vector3.up, radius);
            Handles.DrawWireDisc(position + Vector3.up * height, Vector3.up, radius);
            
            // Draw vertical lines
            for (int i = 0; i < 8; i++)
            {
                float angle = (float)i / 8f * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Handles.DrawLine(position + offset, position + offset + Vector3.up * height);
            }
        }
        
        private static void DrawDirectionArrow(Vector3 position, Vector3 forward, bool dimmed)
        {
            Handles.color = DimColor(directionArrowColor, dimmed);
            
            // Offset slightly above ground
            Vector3 arrowStart = position + Vector3.up * 0.01f;
            Vector3 arrowEnd = arrowStart + forward * directionArrowLength;
            
            // Draw main arrow line
            Handles.DrawLine(arrowStart, arrowEnd, 3f);
            
            // Draw arrow head
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 arrowLeft = arrowEnd - forward * directionArrowHeadSize + right * directionArrowHeadSize * Mathf.Tan(directionArrowHeadAngle * Mathf.Deg2Rad);
            Vector3 arrowRight = arrowEnd - forward * directionArrowHeadSize - right * directionArrowHeadSize * Mathf.Tan(directionArrowHeadAngle * Mathf.Deg2Rad);
            
            Handles.DrawLine(arrowEnd, arrowLeft, 3f);
            Handles.DrawLine(arrowEnd, arrowRight, 3f);
            
            // Draw filled triangle for arrow head
            Vector3[] arrowTriangle = new Vector3[] { arrowEnd, arrowLeft, arrowRight };
            Handles.DrawAAConvexPolygon(arrowTriangle);
        }
        
        #endregion
        
        #region Seated Mode Visualization
        
        private static void DrawSeatedMode(VRInteractionZoneVisualizer visualizer, bool dimmed)
        {
            Vector3 headPosition = visualizer.GetHeadPosition();
            Vector3 forward = visualizer.transform.forward;
            Vector3 right = visualizer.transform.right;
            
            float heightMin = visualizer.GetHeightFromPercent(visualizer.HeightMinPercent);
            float heightMax = visualizer.GetHeightFromPercent(visualizer.HeightMaxPercent);
            float optimalHeightMin = visualizer.GetHeightFromPercent(visualizer.OptimalHeightMinPercent);
            float optimalHeightMax = visualizer.GetHeightFromPercent(visualizer.OptimalHeightMaxPercent);
            
            // Draw head reference indicator
            Handles.color = DimColor(headReferenceColor, dimmed);
            Handles.SphereHandleCap(0, headPosition, Quaternion.identity, headIndicatorSize, EventType.Repaint);
            
            // Dead Zone
            if (visualizer.ShowDeadZone)
            {
                Handles.color = DimColor(deadZoneColor, dimmed);
                Handles.DrawWireDisc(headPosition, Vector3.up, visualizer.DeadZoneRadius);
                Handles.DrawWireDisc(headPosition, Vector3.forward, visualizer.DeadZoneRadius);
                Handles.DrawWireDisc(headPosition, Vector3.right, visualizer.DeadZoneRadius);
            }
            
            // Maximum Reach Zone
            if (visualizer.ShowMaximumZone)
            {
                Handles.color = DimColor(maximumZoneColor, dimmed);
                DrawSeatedInteractionZone(visualizer, headPosition, forward, right,
                    visualizer.SeatedExtendedMax, visualizer.SeatedMaximumMax,
                    heightMin, heightMax);
            }
            
            // Extended Reach Zone
            if (visualizer.ShowExtendedZone)
            {
                Handles.color = DimColor(extendedZoneColor, dimmed);
                DrawSeatedInteractionZone(visualizer, headPosition, forward, right,
                    visualizer.SeatedOptimalMax, visualizer.SeatedExtendedMax,
                    heightMin, heightMax);
            }
            
            // Optimal Grab Zone
            if (visualizer.ShowOptimalZone)
            {
                Handles.color = DimColor(optimalZoneColor, dimmed);
                DrawSeatedInteractionZone(visualizer, headPosition, forward, right,
                    visualizer.SeatedOptimalMin, visualizer.SeatedOptimalMax,
                    optimalHeightMin, optimalHeightMax);
            }
            
            // Draw reference lines
            Handles.color = DimColor(referenceLinesColor, dimmed);
            Handles.DrawLine(headPosition, headPosition + forward * visualizer.SeatedMaximumMax);
            Handles.DrawLine(headPosition + Vector3.up * heightMin,
                headPosition + Vector3.up * heightMax);
            
            // Only draw labels when selected
            if (!dimmed)
            {
                DrawSeatedLabels(visualizer, headPosition, forward);
            }
        }
        
        private static void DrawSeatedInteractionZone(VRInteractionZoneVisualizer visualizer, Vector3 origin,
            Vector3 forward, Vector3 right, float minDist, float maxDist, float minHeight, float maxHeight)
        {
            float angleStep = (visualizer.SeatedArcAngle * 2f) / visualizer.ArcSegments;
            
            // Draw arcs at min and max height
            DrawArc(visualizer, origin + Vector3.up * minHeight, forward, minDist, maxDist, angleStep, visualizer.SeatedArcAngle);
            DrawArc(visualizer, origin + Vector3.up * maxHeight, forward, minDist, maxDist, angleStep, visualizer.SeatedArcAngle);
            
            // Draw vertical connecting lines
            for (int i = 0; i <= visualizer.ArcSegments; i++)
            {
                float angle = -visualizer.SeatedArcAngle + (angleStep * i);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                
                Vector3 innerBottom = origin + Vector3.up * minHeight + direction * minDist;
                Vector3 innerTop = origin + Vector3.up * maxHeight + direction * minDist;
                Handles.DrawLine(innerBottom, innerTop);
                
                Vector3 outerBottom = origin + Vector3.up * minHeight + direction * maxDist;
                Vector3 outerTop = origin + Vector3.up * maxHeight + direction * maxDist;
                Handles.DrawLine(outerBottom, outerTop);
            }
        }
        
        private static void DrawSeatedLabels(VRInteractionZoneVisualizer visualizer, Vector3 headPosition, Vector3 forward)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            
            Handles.Label(headPosition + Vector3.up * labelOffsetAboveHead, "[Seated Mode]", style);
            style.fontStyle = FontStyle.Normal;
            
            if (visualizer.ShowOptimalZone)
            {
                Vector3 optimalPos = headPosition + forward * ((visualizer.SeatedOptimalMin + visualizer.SeatedOptimalMax) / 2f);
                Handles.Label(optimalPos, "OPTIMAL", style);
            }
            
            if (visualizer.ShowExtendedZone)
            {
                Vector3 extendedPos = headPosition + forward * ((visualizer.SeatedOptimalMax + visualizer.SeatedExtendedMax) / 2f);
                style.normal.textColor = Color.yellow;
                Handles.Label(extendedPos, "EXTENDED", style);
            }
            
            if (visualizer.ShowMaximumZone)
            {
                Vector3 maxPos = headPosition + forward * ((visualizer.SeatedExtendedMax + visualizer.SeatedMaximumMax) / 2f);
                style.normal.textColor = new Color(1f, 0.5f, 0f);
                Handles.Label(maxPos, "MAXIMUM", style);
            }
        }
        
        #endregion
        
        #region Room-Scale Mode Visualization
        
        private static void DrawRoomScaleMode(VRInteractionZoneVisualizer visualizer, bool dimmed)
        {
            Vector3 headPosition = visualizer.GetHeadPosition();
            Vector3 rigPosition = visualizer.transform.position;
            
            float heightMin = visualizer.GetHeightFromPercent(visualizer.HeightMinPercent);
            float heightMax = visualizer.GetHeightFromPercent(visualizer.HeightMaxPercent);
            float optimalHeightMin = visualizer.GetHeightFromPercent(visualizer.OptimalHeightMinPercent);
            float optimalHeightMax = visualizer.GetHeightFromPercent(visualizer.OptimalHeightMaxPercent);
            
            float halfWidth = visualizer.PlayAreaWidth / 2f;
            float halfDepth = visualizer.PlayAreaDepth / 2f;
            
            // Draw head reference indicator
            Handles.color = DimColor(headReferenceColor, dimmed);
            Handles.SphereHandleCap(0, headPosition, Quaternion.identity, headIndicatorSize, EventType.Repaint);
            
            // Dead Zone
            if (visualizer.ShowDeadZone)
            {
                Handles.color = DimColor(deadZoneColor, dimmed);
                Handles.DrawWireDisc(headPosition, Vector3.up, visualizer.DeadZoneRadius);
            }
            
            // Draw play area bounds (from floor)
            if (visualizer.ShowPlayAreaBounds)
            {
                Handles.color = DimColor(playAreaOutlineColor, dimmed);
                Vector3[] playAreaCorners = new Vector3[]
                {
                    rigPosition + new Vector3(-halfWidth, heightMin, -halfDepth),
                    rigPosition + new Vector3(halfWidth, heightMin, -halfDepth),
                    rigPosition + new Vector3(halfWidth, heightMin, halfDepth),
                    rigPosition + new Vector3(-halfWidth, heightMin, halfDepth)
                };
                
                Handles.DrawSolidRectangleWithOutline(playAreaCorners, DimColor(playAreaColor, dimmed), DimColor(playAreaOutlineColor, dimmed));
                
                // Draw vertical lines at corners
                for (int i = 0; i < 4; i++)
                {
                    Vector3 bottom = playAreaCorners[i];
                    Vector3 top = bottom + Vector3.up * (heightMax - heightMin);
                    Handles.DrawLine(bottom, top);
                }
                
                // Draw top rectangle
                Vector3[] topCorners = new Vector3[]
                {
                    rigPosition + new Vector3(-halfWidth, heightMax, -halfDepth),
                    rigPosition + new Vector3(halfWidth, heightMax, -halfDepth),
                    rigPosition + new Vector3(halfWidth, heightMax, halfDepth),
                    rigPosition + new Vector3(-halfWidth, heightMax, halfDepth)
                };
                Handles.DrawSolidRectangleWithOutline(topCorners, DimColor(new Color(0f, 1f, 1f, 0.05f), dimmed), DimColor(playAreaOutlineColor, dimmed));
            }
            
            // Draw reach zones from play area edges (relative to head position, not rig position)
            if (visualizer.ShowMaximumZone)
            {
                Handles.color = DimColor(maximumZoneColor, dimmed);
                DrawRoomScaleReachZone(visualizer, rigPosition, headPosition, halfWidth, halfDepth,
                    visualizer.RoomScaleExtendedMax, visualizer.RoomScaleMaximumMax,
                    heightMin, heightMax);
            }
            
            if (visualizer.ShowExtendedZone)
            {
                Handles.color = DimColor(extendedZoneColor, dimmed);
                DrawRoomScaleReachZone(visualizer, rigPosition, headPosition, halfWidth, halfDepth,
                    visualizer.RoomScaleAtEdgeMax, visualizer.RoomScaleExtendedMax,
                    heightMin, heightMax);
            }
            
            if (visualizer.ShowOptimalZone)
            {
                Handles.color = DimColor(optimalZoneColor, dimmed);
                DrawRoomScaleReachZone(visualizer, rigPosition, headPosition, halfWidth, halfDepth,
                    0f, visualizer.RoomScaleAtEdgeMax,
                    optimalHeightMin, optimalHeightMax);
            }
            
            // Only draw labels when selected
            if (!dimmed)
            {
                DrawRoomScaleLabels(visualizer, rigPosition, heightMin);
            }
        }
        
        private static void DrawRoomScaleReachZone(VRInteractionZoneVisualizer visualizer, Vector3 rigPosition, Vector3 headPosition,
            float halfWidth, float halfDepth, float innerOffset, float outerOffset,
            float minHeightPercent, float maxHeightPercent)
        {
            // Heights are relative to head position (0% = head level)
            float minHeight = headPosition.y + minHeightPercent;
            float maxHeight = headPosition.y + maxHeightPercent;
            
            // Bottom rectangles
            Vector3[] bottomInner = new Vector3[]
            {
                rigPosition + new Vector3(-halfWidth - innerOffset, minHeight - rigPosition.y, -halfDepth - innerOffset),
                rigPosition + new Vector3(halfWidth + innerOffset, minHeight - rigPosition.y, -halfDepth - innerOffset),
                rigPosition + new Vector3(halfWidth + innerOffset, minHeight - rigPosition.y, halfDepth + innerOffset),
                rigPosition + new Vector3(-halfWidth - innerOffset, minHeight - rigPosition.y, halfDepth + innerOffset)
            };
            
            Vector3[] bottomOuter = new Vector3[]
            {
                rigPosition + new Vector3(-halfWidth - outerOffset, minHeight - rigPosition.y, -halfDepth - outerOffset),
                rigPosition + new Vector3(halfWidth + outerOffset, minHeight - rigPosition.y, -halfDepth - outerOffset),
                rigPosition + new Vector3(halfWidth + outerOffset, minHeight - rigPosition.y, halfDepth + outerOffset),
                rigPosition + new Vector3(-halfWidth - outerOffset, minHeight - rigPosition.y, halfDepth + outerOffset)
            };
            
            // Top rectangles
            Vector3[] topInner = new Vector3[]
            {
                rigPosition + new Vector3(-halfWidth - innerOffset, maxHeight - rigPosition.y, -halfDepth - innerOffset),
                rigPosition + new Vector3(halfWidth + innerOffset, maxHeight - rigPosition.y, -halfDepth - innerOffset),
                rigPosition + new Vector3(halfWidth + innerOffset, maxHeight - rigPosition.y, halfDepth + innerOffset),
                rigPosition + new Vector3(-halfWidth - innerOffset, maxHeight - rigPosition.y, halfDepth + innerOffset)
            };
            
            Vector3[] topOuter = new Vector3[]
            {
                rigPosition + new Vector3(-halfWidth - outerOffset, maxHeight - rigPosition.y, -halfDepth - outerOffset),
                rigPosition + new Vector3(halfWidth + outerOffset, maxHeight - rigPosition.y, -halfDepth - outerOffset),
                rigPosition + new Vector3(halfWidth + outerOffset, maxHeight - rigPosition.y, halfDepth + outerOffset),
                rigPosition + new Vector3(-halfWidth - outerOffset, maxHeight - rigPosition.y, halfDepth + outerOffset)
            };
            
            // Draw bottom rectangles
            Handles.DrawPolyLine(bottomOuter[0], bottomOuter[1], bottomOuter[2], bottomOuter[3], bottomOuter[0]);
            Handles.DrawPolyLine(bottomInner[0], bottomInner[1], bottomInner[2], bottomInner[3], bottomInner[0]);
            
            // Draw top rectangles
            Handles.DrawPolyLine(topOuter[0], topOuter[1], topOuter[2], topOuter[3], topOuter[0]);
            Handles.DrawPolyLine(topInner[0], topInner[1], topInner[2], topInner[3], topInner[0]);
            
            // Connect corners with vertical lines
            for (int i = 0; i < 4; i++)
            {
                Handles.DrawLine(bottomInner[i], topInner[i]);
                Handles.DrawLine(bottomOuter[i], topOuter[i]);
                
                // Connect inner to outer at bottom and top
                Handles.DrawLine(bottomInner[i], bottomOuter[i]);
                Handles.DrawLine(topInner[i], topOuter[i]);
            }
        }
        
        private static void DrawRoomScaleLabels(VRInteractionZoneVisualizer visualizer, Vector3 rigPosition, float heightMin)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            
            Vector3 labelPos = rigPosition + new Vector3(0, visualizer.GetPlayerHeight() + labelOffsetAboveHead, 0);
            Handles.Label(labelPos, "[Room-Scale Mode]", style);
            
            style.fontStyle = FontStyle.Normal;
            style.normal.textColor = Color.cyan;
            Vector3 playAreaLabel = rigPosition + new Vector3(0, heightMin + playAreaLabelOffsetAboveFloor, 0);
            Handles.Label(playAreaLabel, "PLAY AREA", style);
        }
        
        #endregion
        
        #region Helper Methods
        
        private static void DrawArc(VRInteractionZoneVisualizer visualizer, Vector3 center,
            Vector3 forward, float minRadius, float maxRadius, float angleStep, float arcAngle)
        {
            // Inner and outer arcs
            for (int i = 0; i < visualizer.ArcSegments; i++)
            {
                float angle1 = -arcAngle + (angleStep * i);
                float angle2 = -arcAngle + (angleStep * (i + 1));
                
                Vector3 dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * forward;
                Vector3 dir2 = Quaternion.AngleAxis(angle2, Vector3.up) * forward;
                
                Handles.DrawLine(center + dir1 * minRadius, center + dir2 * minRadius);
                Handles.DrawLine(center + dir1 * maxRadius, center + dir2 * maxRadius);
            }
            
            // Radial lines
            for (int i = 0; i <= visualizer.ArcSegments; i += Mathf.Max(1, visualizer.ArcSegments / 4))
            {
                float angle = -arcAngle + (angleStep * i);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                Handles.DrawLine(center + direction * minRadius, center + direction * maxRadius);
            }
        }
        
        #endregion
    }
}