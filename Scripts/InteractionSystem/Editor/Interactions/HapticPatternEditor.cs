using System.Threading.Tasks;
using Shababeek.Interactions;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(HapticPattern))]
    public class HapticPatternEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the pattern shape flat, skipping the base ScriptableVariable's
            // "Variable Value" header and struct foldout.
            serializedObject.Update();
            var valueProp = serializedObject.FindProperty("value");
            EditorGUILayout.LabelField("Pattern", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("amplitude"));
            EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("duration"));
            EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("strength"));
            serializedObject.ApplyModifiedProperties();

            var pattern = (HapticPattern)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Click")) ApplyPreset(pattern, PresetClick());
            if (GUILayout.Button("Detent")) ApplyPreset(pattern, PresetDetent());
            if (GUILayout.Button("Thud")) ApplyPreset(pattern, PresetThud());
            if (GUILayout.Button("Buzz")) ApplyPreset(pattern, PresetBuzz());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test (requires connected controllers)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Left")) _ = TestOnNode(pattern, XRNode.LeftHand);
            if (GUILayout.Button("Test Right")) _ = TestOnNode(pattern, XRNode.RightHand);
            EditorGUILayout.EndHorizontal();
        }

        private void ApplyPreset(HapticPattern pattern, (AnimationCurve curve, float duration, float strength) preset)
        {
            Undo.RecordObject(pattern, "Apply Haptic Preset");
            pattern.SetShape(preset.curve, preset.duration, preset.strength);
            EditorUtility.SetDirty(pattern);
        }

        // Sharp, short tap — buttons, UI confirms.
        private static (AnimationCurve, float, float) PresetClick() =>
            (AnimationCurve.Constant(0f, 1f, 1f), 0.03f, 0.8f);

        // Quick ramp-down — dial/slider step boundaries.
        private static (AnimationCurve, float, float) PresetDetent() =>
            (AnimationCurve.EaseInOut(0f, 1f, 1f, 0f), 0.05f, 0.5f);

        // Strong hit fading out — impacts, socket inserts.
        private static (AnimationCurve, float, float) PresetThud() =>
            (new AnimationCurve(
                new Keyframe(0f, 1f, 0f, -2f),
                new Keyframe(1f, 0f, -0.5f, 0f)), 0.15f, 1f);

        // Oscillating rumble — rejections, warnings.
        private static (AnimationCurve, float, float) PresetBuzz() =>
            (new AnimationCurve(
                new Keyframe(0f, 0.7f),
                new Keyframe(0.17f, 0.2f),
                new Keyframe(0.33f, 0.7f),
                new Keyframe(0.5f, 0.2f),
                new Keyframe(0.67f, 0.7f),
                new Keyframe(0.83f, 0.2f),
                new Keyframe(1f, 0.7f)), 0.3f, 0.6f);

        private static async Task TestOnNode(HapticPattern pattern, XRNode node)
        {
            // Editor-time playback: Task.Delay-driven sampling (Awaitable needs the player loop).
            const int stepMs = 20;
            float duration = pattern.Duration;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                var device = InputDevices.GetDeviceAtXRNode(node);
                if (!device.isValid) return;
                float amp = pattern.Evaluate(elapsed / duration);
                if (amp > 0.001f) device.SendHapticImpulse(0, amp, stepMs / 1000f * 1.5f);
                await Task.Delay(stepMs);
                elapsed += stepMs / 1000f;
            }
        }
    }
}
