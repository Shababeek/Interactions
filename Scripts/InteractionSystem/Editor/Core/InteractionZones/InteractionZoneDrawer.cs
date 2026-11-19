using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shababeek.Interactions.Core.Editors
{
    /// <summary>Draws the interaction zones of a rig in the Scene view.</summary>
    internal static class InteractionZoneDrawer
    {
        private const float FloorLift = 0.004f;
        private const float FillAlpha = 0.16f;
        private const float LineAlpha = 0.95f;
        private const float UnselectedAlpha = 0.4f;
        private const float OccludedAlpha = 0.4f;
        private const float LineWidth = 2f;
        private const int CornerSegments = 6;
        private const float DegreesPerArcSegment = 6f;
        private const float HeadRadius = 0.1f;
        private const float ShoulderDrop = 0.22f;
        private const float ArrowLength = 0.3f;
        private const float GaugeGap = 0.1f;
        private const float GaugeTick = 0.06f;
        private const float LabelMinWorldSize = 0.2f;
        private const float LabelMinScreenRatio = 0.2f;

        private static readonly Vector3[] QuadBuffer = new Vector3[4];
        private static float _alpha = 1f;
        private static bool _drawFills = true;
        private static bool _drawLabels;
        private static bool _selected;
        private static GUIStyle _labelStyle;
        private static Texture2D _labelBackground;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawZones(VRInteractionZoneVisualizer visualizer, GizmoType gizmoType)
        {
            _selected = IsRigSelected(visualizer);
            if (!_selected && !InteractionZoneViewSettings.ShowWhenUnselected) return;

            var rig = visualizer.transform;
            var rigSpace = Matrix4x4.TRS(rig.position, Quaternion.Euler(0f, rig.eulerAngles.y, 0f), Vector3.one);
            var head = rigSpace.inverse.MultiplyPoint3x4(visualizer.GetHeadPosition());
            var previousZTest = Handles.zTest;

            using (new Handles.DrawingScope(rigSpace))
            {
                Handles.zTest = CompareFunction.LessEqual;
                _alpha = _selected ? 1f : UnselectedAlpha;
                _drawFills = true;
                _drawLabels = _selected && InteractionZoneViewSettings.IsVisible(InteractionZoneLayers.Labels);
                DrawAll(visualizer, head);

                if (_selected)
                {
                    Handles.zTest = CompareFunction.Greater;
                    _alpha = OccludedAlpha;
                    _drawLabels = false;
                    DrawAll(visualizer, head);
                }
            }

            Handles.zTest = previousZTest;
        }

        private static bool IsRigSelected(VRInteractionZoneVisualizer visualizer)
        {
            var active = Selection.activeTransform;
            return active && active.IsChildOf(visualizer.transform);
        }

        private static void DrawAll(VRInteractionZoneVisualizer visualizer, Vector3 head)
        {
            if (visualizer.VrMode == VRMode.Seated) DrawSeatedZones(visualizer, head);
            else DrawRoomScaleZones(visualizer, head);

            if (IsVisible(InteractionZoneLayers.Player)) DrawPlayer(visualizer, head);
            if (!_selected) return;

            if (IsVisible(InteractionZoneLayers.DeadZone)) DrawDeadZone(visualizer, head);
            if (IsVisible(InteractionZoneLayers.Heights)) DrawHeightGauge(visualizer, head);
        }

        #region Seated

        private static void DrawSeatedZones(VRInteractionZoneVisualizer visualizer, Vector3 head)
        {
            var settings = visualizer.Seated;
            var center = new Vector3(head.x, FloorLift, head.z);
            float reachMin = head.y + visualizer.PercentToMetres(settings.reachHeightPercent.x);
            float reachMax = head.y + visualizer.PercentToMetres(settings.reachHeightPercent.y);
            float optimalMin = head.y + visualizer.PercentToMetres(settings.optimalHeightPercent.x);
            float optimalMax = head.y + visualizer.PercentToMetres(settings.optimalHeightPercent.y);

            DrawSector(InteractionZoneLayers.Maximum, center, settings.extendedDistance, settings.maximumDistance,
                settings.arcAngle, reachMin, reachMax, false);
            DrawSector(InteractionZoneLayers.Extended, center, settings.optimalMax, settings.extendedDistance,
                settings.arcAngle, reachMin, reachMax, false);
            DrawSector(InteractionZoneLayers.Optimal, center, settings.optimalMin, settings.optimalMax,
                settings.arcAngle, optimalMin, optimalMax, true);
        }

        private static void DrawSector(InteractionZoneLayers layer, Vector3 center, float inner, float outer,
            float arcAngle, float yMin, float yMax, bool outlineInner)
        {
            if (!IsVisible(layer) || outer - inner < 0.001f) return;

            var color = InteractionZoneViewSettings.GetColor(layer);
            bool closed = arcAngle >= 179.9f;
            int segments = Mathf.Max(8, Mathf.CeilToInt(arcAngle * 2f / DegreesPerArcSegment));
            var innerPoints = new Vector3[segments + 1];
            var outerPoints = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-arcAngle, arcAngle, i / (float)segments) * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                innerPoints[i] = center + direction * inner;
                outerPoints[i] = center + direction * outer;
            }

            FillBand(innerPoints, outerPoints, color);
            DrawSectorOutline(innerPoints, outerPoints, color, closed, outlineInner);

            if (_selected && IsVisible(InteractionZoneLayers.Heights))
            {
                var bottomInner = AtHeight(innerPoints, yMin);
                var bottomOuter = AtHeight(outerPoints, yMin);
                var topInner = AtHeight(innerPoints, yMax);
                var topOuter = AtHeight(outerPoints, yMax);
                if (layer == InteractionZoneLayers.Optimal)
                {
                    FillBand(bottomInner, bottomOuter, color);
                    FillBand(topInner, topOuter, color);
                }

                DrawSectorOutline(bottomInner, bottomOuter, color, closed, outlineInner);
                DrawSectorOutline(topInner, topOuter, color, closed, outlineInner);

                int middle = segments / 2;
                DrawLine(bottomOuter[middle], topOuter[middle], color);
                if (!closed)
                {
                    DrawLine(bottomOuter[0], topOuter[0], color);
                    DrawLine(bottomOuter[segments], topOuter[segments], color);
                }

                if (outlineInner)
                {
                    DrawLine(bottomInner[middle], topInner[middle], color);
                    if (!closed)
                    {
                        DrawLine(bottomInner[0], topInner[0], color);
                        DrawLine(bottomInner[segments], topInner[segments], color);
                    }
                }
            }

            var labelPosition = center + Vector3.forward * ((inner + outer) * 0.5f);
            DrawLabel(labelPosition, $"{InteractionZoneViewSettings.GetLabel(layer)}  {inner:0.##}-{outer:0.##} m", color);
        }

        private static void DrawSectorOutline(Vector3[] inner, Vector3[] outer, Color color, bool closed, bool outlineInner)
        {
            DrawPolyLine(outer, color);
            if (outlineInner) DrawPolyLine(inner, color);
            if (closed) return;

            int last = inner.Length - 1;
            DrawLine(inner[0], outer[0], color);
            DrawLine(inner[last], outer[last], color);
        }

        #endregion

        #region Room Scale

        private static void DrawRoomScaleZones(VRInteractionZoneVisualizer visualizer, Vector3 head)
        {
            var settings = visualizer.RoomScale;
            float halfWidth = settings.playAreaWidth * 0.5f;
            float halfDepth = settings.playAreaDepth * 0.5f;
            float reachMin = head.y + visualizer.PercentToMetres(settings.reachHeightPercent.x);
            float reachMax = head.y + visualizer.PercentToMetres(settings.reachHeightPercent.y);
            float optimalMin = head.y + visualizer.PercentToMetres(settings.optimalHeightPercent.x);
            float optimalMax = head.y + visualizer.PercentToMetres(settings.optimalHeightPercent.y);

            DrawBand(InteractionZoneLayers.Maximum, halfWidth, halfDepth, settings.extendedDistance,
                settings.maximumDistance, reachMin, reachMax);
            DrawBand(InteractionZoneLayers.Extended, halfWidth, halfDepth, settings.optimalDistance,
                settings.extendedDistance, reachMin, reachMax);
            DrawBand(InteractionZoneLayers.Optimal, halfWidth, halfDepth, 0f,
                settings.optimalDistance, optimalMin, optimalMax);

            if (!IsVisible(InteractionZoneLayers.PlayArea)) return;

            var color = InteractionZoneViewSettings.GetColor(InteractionZoneLayers.PlayArea);
            var corners = new[]
            {
                new Vector3(-halfWidth, FloorLift, -halfDepth),
                new Vector3(halfWidth, FloorLift, -halfDepth),
                new Vector3(halfWidth, FloorLift, halfDepth),
                new Vector3(-halfWidth, FloorLift, halfDepth),
                new Vector3(-halfWidth, FloorLift, -halfDepth)
            };
            if (_drawFills)
            {
                QuadBuffer[0] = corners[0];
                QuadBuffer[1] = corners[1];
                QuadBuffer[2] = corners[2];
                QuadBuffer[3] = corners[3];
                Handles.color = Color.white;
                Handles.DrawSolidRectangleWithOutline(QuadBuffer, WithAlpha(color, FillAlpha), Color.clear);
            }

            DrawPolyLine(corners, color);
            DrawLabel(new Vector3(0f, FloorLift, -halfDepth * 0.6f),
                $"Play Area  {settings.playAreaWidth:0.##} x {settings.playAreaDepth:0.##} m", color);
        }

        private static void DrawBand(InteractionZoneLayers layer, float halfWidth, float halfDepth, float inner,
            float outer, float yMin, float yMax)
        {
            if (!IsVisible(layer) || outer - inner < 0.001f) return;

            var color = InteractionZoneViewSettings.GetColor(layer);
            var innerRing = BuildRing(halfWidth, halfDepth, inner, FloorLift);
            var outerRing = BuildRing(halfWidth, halfDepth, outer, FloorLift);

            FillBand(innerRing, outerRing, color);
            DrawPolyLine(outerRing, color);

            if (_selected && IsVisible(InteractionZoneLayers.Heights))
            {
                var bottom = AtHeight(outerRing, yMin);
                var top = AtHeight(outerRing, yMax);
                if (layer == InteractionZoneLayers.Optimal)
                {
                    FillBand(AtHeight(innerRing, yMin), bottom, color);
                    FillBand(AtHeight(innerRing, yMax), top, color);
                }

                DrawPolyLine(bottom, color);
                DrawPolyLine(top, color);
                for (int corner = 0; corner < 4; corner++)
                {
                    int index = corner * (CornerSegments + 1) + CornerSegments / 2;
                    DrawLine(bottom[index], top[index], color);
                }
            }

            var labelPosition = new Vector3(0f, FloorLift, halfDepth + (inner + outer) * 0.5f);
            DrawLabel(labelPosition, $"{InteractionZoneViewSettings.GetLabel(layer)}  {inner:0.##}-{outer:0.##} m", color);
        }

        private static Vector3[] BuildRing(float halfWidth, float halfDepth, float offset, float y)
        {
            int perCorner = CornerSegments + 1;
            var points = new Vector3[perCorner * 4 + 1];
            for (int corner = 0; corner < 4; corner++)
            {
                float signX = corner == 0 || corner == 3 ? 1f : -1f;
                float signZ = corner < 2 ? 1f : -1f;
                var cornerPoint = new Vector3(signX * halfWidth, y, signZ * halfDepth);
                for (int i = 0; i < perCorner; i++)
                {
                    float angle = (corner * 90f + 90f * i / CornerSegments) * Mathf.Deg2Rad;
                    points[corner * perCorner + i] =
                        cornerPoint + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * offset;
                }
            }

            points[points.Length - 1] = points[0];
            return points;
        }

        #endregion

        #region Player

        private static void DrawPlayer(VRInteractionZoneVisualizer visualizer, Vector3 head)
        {
            var color = InteractionZoneViewSettings.GetColor(InteractionZoneLayers.Player);
            float radius = visualizer.PlayerRadius;
            var feet = new Vector3(head.x, FloorLift, head.z);
            var shoulders = new Vector3(head.x, Mathf.Max(FloorLift, head.y - ShoulderDrop), head.z);

            if (_drawFills)
            {
                Handles.color = WithAlpha(color, FillAlpha);
                Handles.DrawSolidDisc(feet, Vector3.up, radius);
            }

            Handles.color = WithAlpha(color, LineAlpha * 0.7f);
            Handles.DrawWireDisc(feet, Vector3.up, radius);
            Handles.DrawWireDisc(shoulders, Vector3.up, radius);
            for (int i = 0; i < 4; i++)
            {
                var offset = Quaternion.Euler(0f, 45f + i * 90f, 0f) * Vector3.forward * radius;
                Handles.DrawLine(feet + offset, shoulders + offset);
            }

            Handles.DrawWireDisc(head, Vector3.up, HeadRadius);
            Handles.DrawWireDisc(head, Vector3.right, HeadRadius);
            Handles.DrawWireDisc(head, Vector3.forward, HeadRadius);

            var arrowStart = feet + Vector3.forward * radius;
            var arrowEnd = arrowStart + Vector3.forward * ArrowLength;
            Handles.color = WithAlpha(color, LineAlpha);
            Handles.DrawAAPolyLine(LineWidth * 2f, arrowStart, arrowEnd);
            Handles.DrawAAConvexPolygon(
                arrowEnd + Vector3.forward * 0.08f,
                arrowEnd + Vector3.right * 0.05f,
                arrowEnd - Vector3.right * 0.05f);
        }

        private static void DrawDeadZone(VRInteractionZoneVisualizer visualizer, Vector3 head)
        {
            float radius = visualizer.DeadZoneRadius;
            if (radius <= 0f) return;

            Handles.color = WithAlpha(InteractionZoneViewSettings.GetColor(InteractionZoneLayers.DeadZone), LineAlpha * 0.8f);
            Handles.DrawWireDisc(head, Vector3.up, radius);
            Handles.DrawWireDisc(head, Vector3.right, radius);
            Handles.DrawWireDisc(head, Vector3.forward, radius);
        }

        private static void DrawHeightGauge(VRInteractionZoneVisualizer visualizer, Vector3 head)
        {
            bool seated = visualizer.VrMode == VRMode.Seated;
            var reach = seated ? visualizer.Seated.reachHeightPercent : visualizer.RoomScale.reachHeightPercent;
            var optimal = seated ? visualizer.Seated.optimalHeightPercent : visualizer.RoomScale.optimalHeightPercent;
            float reachMin = head.y + visualizer.PercentToMetres(reach.x);
            float reachMax = head.y + visualizer.PercentToMetres(reach.y);
            float optimalMin = head.y + visualizer.PercentToMetres(optimal.x);
            float optimalMax = head.y + visualizer.PercentToMetres(optimal.y);

            var foot = new Vector3(head.x + visualizer.PlayerRadius + GaugeGap, 0f, head.z);
            var neutral = InteractionZoneViewSettings.GetColor(InteractionZoneLayers.Heights);
            var reachColor = InteractionZoneViewSettings.GetColor(InteractionZoneLayers.Extended);
            var optimalColor = InteractionZoneViewSettings.GetColor(InteractionZoneLayers.Optimal);

            Handles.color = WithAlpha(neutral, LineAlpha * 0.5f);
            Handles.DrawDottedLine(foot, foot + Vector3.up * reachMin, 3f);

            Handles.color = WithAlpha(reachColor, LineAlpha);
            Handles.DrawAAPolyLine(LineWidth * 2f, foot + Vector3.up * reachMin, foot + Vector3.up * reachMax);
            Handles.color = WithAlpha(optimalColor, LineAlpha);
            Handles.DrawAAPolyLine(LineWidth * 4f, foot + Vector3.up * optimalMin, foot + Vector3.up * optimalMax);

            DrawGaugeTick(foot, reachMin, reachColor);
            DrawGaugeTick(foot, reachMax, reachColor);
            DrawGaugeTick(foot, optimalMin, optimalColor);
            DrawGaugeTick(foot, optimalMax, optimalColor);
        }

        private static void DrawGaugeTick(Vector3 foot, float height, Color color)
        {
            var start = foot + Vector3.up * height;
            var end = start + Vector3.right * GaugeTick;
            DrawLine(start, end, color);
            DrawLabel(end + Vector3.right * 0.02f, $"{height:0.00} m", color);
        }

        #endregion

        #region Primitives

        private static bool IsVisible(InteractionZoneLayers layer) => InteractionZoneViewSettings.IsVisible(layer);

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha * _alpha;
            return color;
        }

        private static Vector3[] AtHeight(Vector3[] points, float y)
        {
            var lifted = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
                lifted[i] = new Vector3(points[i].x, y, points[i].z);
            return lifted;
        }

        private static void FillBand(Vector3[] inner, Vector3[] outer, Color color)
        {
            if (!_drawFills) return;

            var face = WithAlpha(color, FillAlpha);
            Handles.color = Color.white;
            for (int i = 0; i < inner.Length - 1; i++)
            {
                QuadBuffer[0] = inner[i];
                QuadBuffer[1] = outer[i];
                QuadBuffer[2] = outer[i + 1];
                QuadBuffer[3] = inner[i + 1];
                Handles.DrawSolidRectangleWithOutline(QuadBuffer, face, Color.clear);
            }
        }

        private static void DrawPolyLine(Vector3[] points, Color color)
        {
            Handles.color = WithAlpha(color, LineAlpha);
            Handles.DrawAAPolyLine(LineWidth, points);
        }

        private static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            Handles.color = WithAlpha(color, LineAlpha);
            Handles.DrawAAPolyLine(LineWidth, from, to);
        }

        private static void DrawLabel(Vector3 position, string text, Color color, float worldSize = LabelMinWorldSize)
        {
            if (!_drawLabels) return;
            if (worldSize < HandleUtility.GetHandleSize(position) * LabelMinScreenRatio) return;

            if (_labelBackground == null)
            {
                _labelBackground = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                _labelBackground.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.65f));
                _labelBackground.Apply();
                _labelStyle = null;
            }

            _labelStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 2, 2),
                normal = { background = _labelBackground }
            };
            _labelStyle.normal.textColor = Color.Lerp(color, Color.white, 0.35f);
            Handles.Label(position, text, _labelStyle);
        }

        #endregion
    }
}
