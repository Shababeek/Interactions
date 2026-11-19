using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Core.Editors
{
    /// <summary>Inspector for the Interaction Zone Visualizer: one mode at a time, plus the Scene view layer switches.</summary>
    [CustomEditor(typeof(VRInteractionZoneVisualizer))]
    public class InteractionZoneVisualizerEditor : Editor
    {
        private const float HeightSliderMin = -100f;
        private const float HeightSliderMax = 50f;
        private const float SwatchSize = 12f;

        private static readonly string[] ModeLabels = { "Room Scale", "Seated" };

        private SerializedProperty _vrMode;
        private SerializedProperty _playerRadius;
        private SerializedProperty _deadZoneRadius;
        private SerializedProperty _roomScale;
        private SerializedProperty _seated;

        private void OnEnable()
        {
            _vrMode = serializedObject.FindProperty("vrMode");
            _playerRadius = serializedObject.FindProperty("playerRadius");
            _deadZoneRadius = serializedObject.FindProperty("deadZoneRadius");
            _roomScale = serializedObject.FindProperty("roomScale");
            _seated = serializedObject.FindProperty("seated");
            InteractionZoneViewSettings.Changed += Repaint;
        }

        private void OnDisable()
        {
            InteractionZoneViewSettings.Changed -= Repaint;
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            var visualizer = (VRInteractionZoneVisualizer)target;
            serializedObject.Update();

            DrawModeSelector();
            bool seated = _vrMode.enumValueIndex == (int)VRMode.Seated;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_playerRadius);
            EditorGUILayout.PropertyField(_deadZoneRadius);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(
                    new GUIContent("Eye Height", "Taken from Camera Height on the Camera Rig."), visualizer.EyeHeight);
            }

            EditorGUILayout.Space();
            if (seated) DrawSeatedSettings(visualizer);
            else DrawRoomScaleSettings(visualizer);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawResetButton(visualizer, seated ? VRMode.Seated : VRMode.RoomScale);

            EditorGUILayout.Space();
            DrawSceneViewLayers(visualizer, seated);
        }

        private void DrawModeSelector()
        {
            EditorGUI.BeginChangeCheck();
            int mode = GUILayout.Toolbar(_vrMode.enumValueIndex, ModeLabels, GUILayout.Height(24f));
            if (EditorGUI.EndChangeCheck()) _vrMode.enumValueIndex = mode;

            EditorGUILayout.HelpBox(mode == (int)VRMode.Seated
                    ? "Seated: the player stays in one spot. Zones are distances in front of the head."
                    : "Room Scale: the player walks inside the play area. Zones are distances past its edge.",
                MessageType.None);
        }

        private void DrawRoomScaleSettings(VRInteractionZoneVisualizer visualizer)
        {
            EditorGUILayout.LabelField("Play Area", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_roomScale.FindPropertyRelative("playAreaWidth"), new GUIContent("Width"));
            EditorGUILayout.PropertyField(_roomScale.FindPropertyRelative("playAreaDepth"), new GUIContent("Depth"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reach Past The Edge (m)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_roomScale.FindPropertyRelative("optimalDistance"), new GUIContent("Optimal"));
            EditorGUILayout.PropertyField(_roomScale.FindPropertyRelative("extendedDistance"), new GUIContent("Extended"));
            EditorGUILayout.PropertyField(_roomScale.FindPropertyRelative("maximumDistance"), new GUIContent("Maximum"));

            EditorGUILayout.Space();
            DrawHeights(visualizer, _roomScale);
        }

        private void DrawSeatedSettings(VRInteractionZoneVisualizer visualizer)
        {
            EditorGUILayout.LabelField("Reach In Front Of The Head (m)", EditorStyles.boldLabel);
            DrawRange(_seated.FindPropertyRelative("optimalMin"), _seated.FindPropertyRelative("optimalMax"),
                new GUIContent("Optimal", "Closest and furthest comfortable distance."), 0f, 1.5f);
            EditorGUILayout.PropertyField(_seated.FindPropertyRelative("extendedDistance"), new GUIContent("Extended"));
            EditorGUILayout.PropertyField(_seated.FindPropertyRelative("maximumDistance"), new GUIContent("Maximum"));
            EditorGUILayout.PropertyField(_seated.FindPropertyRelative("arcAngle"), new GUIContent("Arc Half Angle"));

            EditorGUILayout.Space();
            DrawHeights(visualizer, _seated);
        }

        private void DrawHeights(VRInteractionZoneVisualizer visualizer, SerializedProperty settings)
        {
            EditorGUILayout.LabelField("Heights (% of eye height, from the head)", EditorStyles.boldLabel);
            DrawHeightRange(visualizer, settings.FindPropertyRelative("reachHeightPercent"),
                new GUIContent("Reachable", "Lowest and highest height the player can reach."));
            DrawHeightRange(visualizer, settings.FindPropertyRelative("optimalHeightPercent"),
                new GUIContent("Optimal", "Lowest and highest comfortable height."));
        }

        private void DrawHeightRange(VRInteractionZoneVisualizer visualizer, SerializedProperty range, GUIContent label)
        {
            var value = range.vector2Value;
            EditorGUI.BeginChangeCheck();
            DrawMinMax(label, ref value.x, ref value.y, HeightSliderMin, HeightSliderMax);
            if (EditorGUI.EndChangeCheck()) range.vector2Value = value;

            float eye = visualizer.EyeHeight;
            float low = eye + visualizer.PercentToMetres(value.x);
            float high = eye + visualizer.PercentToMetres(value.y);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField(" ", $"{low:0.00} m to {high:0.00} m above the floor", EditorStyles.miniLabel);
            }
        }

        private static void DrawRange(SerializedProperty min, SerializedProperty max, GUIContent label, float low, float high)
        {
            float minValue = min.floatValue;
            float maxValue = max.floatValue;
            EditorGUI.BeginChangeCheck();
            DrawMinMax(label, ref minValue, ref maxValue, low, high);
            if (!EditorGUI.EndChangeCheck()) return;

            min.floatValue = minValue;
            max.floatValue = maxValue;
        }

        private static void DrawMinMax(GUIContent label, ref float min, ref float max, float low, float high)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                min = EditorGUILayout.DelayedFloatField(min, GUILayout.Width(48f));
                EditorGUILayout.MinMaxSlider(ref min, ref max, low, high);
                max = EditorGUILayout.DelayedFloatField(max, GUILayout.Width(48f));
            }

            min = Mathf.Round(min * 100f) / 100f;
            max = Mathf.Max(min, Mathf.Round(max * 100f) / 100f);
        }

        private static void DrawResetButton(VRInteractionZoneVisualizer visualizer, VRMode mode)
        {
            string modeName = ModeLabels[(int)mode];
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (!GUILayout.Button(new GUIContent($"Reset {modeName} Values",
                        $"Restores the default {modeName} zones. The other mode is not touched."),
                        GUILayout.Width(190f))) return;
            }

            Undo.RecordObject(visualizer, $"Reset {modeName} Zones");
            visualizer.ResetToDefaults(mode);
            PrefabUtility.RecordPrefabInstancePropertyModifications(visualizer);
            EditorUtility.SetDirty(visualizer);
        }

        private static void DrawSceneViewLayers(VRInteractionZoneVisualizer visualizer, bool seated)
        {
            EditorGUILayout.LabelField("Scene View", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var room = visualizer.RoomScale;
                var seat = visualizer.Seated;
                if (seated)
                {
                    DrawLayerRow(InteractionZoneLayers.Optimal, $"{seat.optimalMin:0.##} to {seat.optimalMax:0.##} m");
                    DrawLayerRow(InteractionZoneLayers.Extended, $"{seat.optimalMax:0.##} to {seat.extendedDistance:0.##} m");
                    DrawLayerRow(InteractionZoneLayers.Maximum, $"{seat.extendedDistance:0.##} to {seat.maximumDistance:0.##} m");
                }
                else
                {
                    DrawLayerRow(InteractionZoneLayers.PlayArea, $"{room.playAreaWidth:0.##} x {room.playAreaDepth:0.##} m");
                    DrawLayerRow(InteractionZoneLayers.Optimal, $"edge to {room.optimalDistance:0.##} m");
                    DrawLayerRow(InteractionZoneLayers.Extended, $"{room.optimalDistance:0.##} to {room.extendedDistance:0.##} m");
                    DrawLayerRow(InteractionZoneLayers.Maximum, $"{room.extendedDistance:0.##} to {room.maximumDistance:0.##} m");
                }

                DrawLayerRow(InteractionZoneLayers.DeadZone, $"within {visualizer.DeadZoneRadius:0.##} m of the head");
                DrawLayerRow(InteractionZoneLayers.Player, string.Empty);
                DrawLayerRow(InteractionZoneLayers.Heights, "volumes and the height gauge");
                DrawLayerRow(InteractionZoneLayers.Labels, string.Empty);

                EditorGUILayout.Space(2f);
                InteractionZoneViewSettings.ShowWhenUnselected = EditorGUILayout.ToggleLeft(
                    new GUIContent("Show When Not Selected", "Draw a faint footprint while the rig is not selected."),
                    InteractionZoneViewSettings.ShowWhenUnselected);
            }

            EditorGUILayout.LabelField("These switches are your own editor preference. They are not saved in the scene or prefab.",
                EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawLayerRow(InteractionZoneLayers layer, string detail)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var swatch = GUILayoutUtility.GetRect(SwatchSize, SwatchSize, GUILayout.Width(SwatchSize), GUILayout.Height(SwatchSize));
                swatch.y += 3f;
                EditorGUI.DrawRect(swatch, InteractionZoneViewSettings.GetColor(layer));

                bool visible = InteractionZoneViewSettings.IsVisible(layer);
                bool toggled = EditorGUILayout.ToggleLeft(InteractionZoneViewSettings.GetLabel(layer), visible, GUILayout.Width(130f));
                if (toggled != visible) InteractionZoneViewSettings.SetVisible(layer, toggled);

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(detail, EditorStyles.miniLabel, GUILayout.MinWidth(40f));
            }
        }
    }
}
