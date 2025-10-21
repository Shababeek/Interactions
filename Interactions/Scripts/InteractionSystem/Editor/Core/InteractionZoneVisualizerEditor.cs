using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Core.Editors
{
    [CustomEditor(typeof(VRInteractionZoneVisualizer))]
    public class VRInteractionZoneVisualizerEditor : Editor
    {
        private void OnSceneGUI()
        {
            var visualizer = (VRInteractionZoneVisualizer)target;
            
            if (visualizer.GetCameraRig() == null) return;
            
            // Get head position
            Vector3 headPosition = visualizer.GetHeadPosition();
            Vector3 forward = visualizer.transform.forward;
            Vector3 right = visualizer.transform.right;
            
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, headPosition, Quaternion.identity, 0.05f, EventType.Repaint);
            
            if (visualizer.ShowDeadZone)
            {
                Handles.color = new Color(1f, 0f, 0f, 0.7f);
                Handles.DrawWireDisc(headPosition, Vector3.up, visualizer.DeadZoneRadius);
                Handles.DrawWireDisc(headPosition, Vector3.forward, visualizer.DeadZoneRadius);
                Handles.DrawWireDisc(headPosition, Vector3.right, visualizer.DeadZoneRadius);
            }
            
            // Maximum Reach Zone (outermost)
            if (visualizer.ShowMaximumZone)
            {
                Handles.color = new Color(1f, 0.5f, 0f, 0.4f); // Orange
                DrawInteractionZone(visualizer, headPosition, forward, right, 
                    visualizer.ExtendedMaxDistance, visualizer.MaximumMaxDistance, 
                    visualizer.HeightMin, visualizer.HeightMax);
            }
            
            // Extended Reach Zone
            if (visualizer.ShowExtendedZone)
            {
                Handles.color = new Color(1f, 1f, 0f, 0.5f); // Yellow
                DrawInteractionZone(visualizer, headPosition, forward, right, 
                    visualizer.OptimalMaxDistance, visualizer.ExtendedMaxDistance, 
                    visualizer.HeightMin, visualizer.HeightMax);
            }
            
            // Optimal Grab Zone
            if (visualizer.ShowOptimalZone)
            {
                Handles.color = new Color(0f, 1f, 0f, 0.6f); // Green
                DrawInteractionZone(visualizer, headPosition, forward, right, 
                    visualizer.OptimalMinDistance, visualizer.OptimalMaxDistance, 
                    visualizer.OptimalHeightMin, visualizer.OptimalHeightMax);
            }
            
            // Draw reference lines
            Handles.color = Color.white;
            Handles.DrawLine(headPosition, headPosition + forward * visualizer.MaximumMaxDistance);
            Handles.DrawLine(headPosition + Vector3.up * visualizer.HeightMin, 
                headPosition + Vector3.up * visualizer.HeightMax);
            
            // Draw labels
            DrawLabels(visualizer, headPosition, forward);
            
            // Draw legend
            if (visualizer.ShowLegend)
            {
                DrawLegend(visualizer, headPosition);
            }
        }
        
        private void DrawInteractionZone(VRInteractionZoneVisualizer visualizer, Vector3 origin, 
            Vector3 forward, Vector3 right, float minDist, float maxDist, float minHeight, float maxHeight)
        {
            float angleStep = (visualizer.ArcAngle * 2f) / visualizer.ArcSegments;
            
            // Draw arcs at min and max height
            DrawArc(visualizer, origin + Vector3.up * minHeight, forward, minDist, maxDist, angleStep);
            DrawArc(visualizer, origin + Vector3.up * maxHeight, forward, minDist, maxDist, angleStep);
            
            // Draw vertical connecting lines at regular intervals
            for (int i = 0; i <= visualizer.ArcSegments; i++)
            {
                float angle = -visualizer.ArcAngle + (angleStep * i);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                
                Vector3 innerBottom = origin + Vector3.up * minHeight + direction * minDist;
                Vector3 innerTop = origin + Vector3.up * maxHeight + direction * minDist;
                Handles.DrawLine(innerBottom, innerTop);
                
                Vector3 outerBottom = origin + Vector3.up * minHeight + direction * maxDist;
                Vector3 outerTop = origin + Vector3.up * maxHeight + direction * maxDist;
                Handles.DrawLine(outerBottom, outerTop);
            }
        }
        
        private void DrawArc(VRInteractionZoneVisualizer visualizer, Vector3 center, 
            Vector3 forward, float minRadius, float maxRadius, float angleStep)
        {
            // Inner and outer arcs
            for (int i = 0; i < visualizer.ArcSegments; i++)
            {
                float angle1 = -visualizer.ArcAngle + (angleStep * i);
                float angle2 = -visualizer.ArcAngle + (angleStep * (i + 1));
                
                Vector3 dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * forward;
                Vector3 dir2 = Quaternion.AngleAxis(angle2, Vector3.up) * forward;
                
                Handles.DrawLine(center + dir1 * minRadius, center + dir2 * minRadius);
                Handles.DrawLine(center + dir1 * maxRadius, center + dir2 * maxRadius);
            }
            
            // Radial lines connecting inner and outer arcs
            for (int i = 0; i <= visualizer.ArcSegments; i += Mathf.Max(1, visualizer.ArcSegments / 4))
            {
                float angle = -visualizer.ArcAngle + (angleStep * i);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                Handles.DrawLine(center + direction * minRadius, center + direction * maxRadius);
            }
        }
        
        private void DrawLabels(VRInteractionZoneVisualizer visualizer, Vector3 headPosition, Vector3 forward)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            style.fontSize = 10;
            
            // Label the zones
            if (visualizer.ShowOptimalZone)
            {
                Vector3 optimalPos = headPosition + forward * ((visualizer.OptimalMinDistance + visualizer.OptimalMaxDistance) / 2f);
                Handles.Label(optimalPos, "OPTIMAL", style);
            }
            
            if (visualizer.ShowExtendedZone)
            {
                Vector3 extendedPos = headPosition + forward * ((visualizer.OptimalMaxDistance + visualizer.ExtendedMaxDistance) / 2f);
                style.normal.textColor = Color.yellow;
                Handles.Label(extendedPos, "EXTENDED", style);
            }
            
            if (visualizer.ShowMaximumZone)
            {
                Vector3 maxPos = headPosition + forward * ((visualizer.ExtendedMaxDistance + visualizer.MaximumMaxDistance) / 2f);
                style.normal.textColor = new Color(1f, 0.5f, 0f);
                Handles.Label(maxPos, "MAXIMUM", style);
            }
        }
        
        private void DrawLegend(VRInteractionZoneVisualizer visualizer, Vector3 headPosition)
        {
            // Position legend to the right and above the head position
            Vector3 legendPosition = headPosition + Vector3.right * 1.5f + Vector3.up * 0.5f;
            
            Handles.BeginGUI();
            
            // Convert 3D position to screen space
            Vector3 screenPos = HandleUtility.WorldToGUIPoint(legendPosition);
            
            // Create legend box
            float boxWidth = 180f;
            float boxHeight = 120f;
            float lineHeight = 25f;
            float padding = 10f;
            
            Rect boxRect = new Rect(screenPos.x, screenPos.y, boxWidth, boxHeight);
            
            // Draw background
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.9f));
            GUI.Box(boxRect, "", boxStyle);
            
            // Draw title
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontSize = 12;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            
            Rect titleRect = new Rect(screenPos.x + padding, screenPos.y + padding, boxWidth - padding * 2, 20f);
            GUI.Label(titleRect, "VR Interaction Zones", titleStyle);
            
            // Draw legend items
            float yOffset = screenPos.y + padding + 25f;
            
            if (visualizer.ShowOptimalZone)
            {
                DrawLegendItem(screenPos.x + padding, yOffset, "Optimal Zone", new Color(0f, 1f, 0f, 0.6f), 
                    $"{visualizer.OptimalMinDistance:F1}m - {visualizer.OptimalMaxDistance:F1}m");
                yOffset += lineHeight;
            }
            
            if (visualizer.ShowExtendedZone)
            {
                DrawLegendItem(screenPos.x + padding, yOffset, "Extended Reach", new Color(1f, 1f, 0f, 0.5f), 
                    $"{visualizer.OptimalMaxDistance:F1}m - {visualizer.ExtendedMaxDistance:F1}m");
                yOffset += lineHeight;
            }
            
            if (visualizer.ShowMaximumZone)
            {
                DrawLegendItem(screenPos.x + padding, yOffset, "Maximum Reach", new Color(1f, 0.5f, 0f, 0.4f), 
                    $"{visualizer.ExtendedMaxDistance:F1}m - {visualizer.MaximumMaxDistance:F1}m");
                yOffset += lineHeight;
            }
            
            if (visualizer.ShowDeadZone)
            {
                DrawLegendItem(screenPos.x + padding, yOffset, "Dead Zone", new Color(1f, 0f, 0f, 0.3f), 
                    $"< {visualizer.DeadZoneRadius:F2}m");
            }
            
            Handles.EndGUI();
        }
        
        private void DrawLegendItem(float x, float y, string label, Color color, string distance)
        {
            // Draw color box
            float boxSize = 15f;
            Rect colorRect = new Rect(x, y, boxSize, boxSize);
            GUIStyle colorStyle = new GUIStyle();
            colorStyle.normal.background = MakeTex(2, 2, color);
            GUI.Box(colorRect, "", colorStyle);
            
            // Draw label
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 10;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            
            Rect labelRect = new Rect(x + boxSize + 5f, y, 100f, boxSize);
            GUI.Label(labelRect, label, labelStyle);
            
            // Draw distance
            GUIStyle distanceStyle = new GUIStyle();
            distanceStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            distanceStyle.fontSize = 9;
            distanceStyle.alignment = TextAnchor.MiddleLeft;
            
            Rect distanceRect = new Rect(x + boxSize + 5f, y + 12f, 100f, boxSize);
            GUI.Label(distanceRect, distance, distanceStyle);
        }
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            
            return result;
        }
    }
}