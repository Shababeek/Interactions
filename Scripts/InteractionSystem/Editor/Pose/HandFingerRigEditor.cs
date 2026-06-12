using Shababeek.Interactions.Animations;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(HandFingerRig))]
    public class HandFingerRigEditor : Editor
    {
        private static readonly HumanBodyBones[] LeftDistals =
        {
            HumanBodyBones.LeftThumbDistal, HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftMiddleDistal,
            HumanBodyBones.LeftRingDistal, HumanBodyBones.LeftLittleDistal
        };

        private static readonly HumanBodyBones[] LeftIntermediates =
        {
            HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftLittleIntermediate
        };

        private static readonly HumanBodyBones[] RightDistals =
        {
            HumanBodyBones.RightThumbDistal, HumanBodyBones.RightIndexDistal, HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.RightRingDistal, HumanBodyBones.RightLittleDistal
        };

        private static readonly HumanBodyBones[] RightIntermediates =
        {
            HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightLittleIntermediate
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var rig = (HandFingerRig)target;

            EditorGUILayout.Space();
            if (!rig.IsValid)
            {
                EditorGUILayout.HelpBox(
                    "Finger references incomplete. Auto-populate from the humanoid avatar, or assign tips manually.",
                    MessageType.Warning);
            }

            if (GUILayout.Button("Auto-Populate From Avatar"))
            {
                AutoPopulate(rig);
            }
        }

        private void AutoPopulate(HandFingerRig rig)
        {
            var animator = rig.GetComponentInChildren<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
            {
                EditorUtility.DisplayDialog("Auto-Populate Failed",
                    "No Animator with a humanoid avatar found on this object or its children. Assign finger tips manually.",
                    "OK");
                return;
            }

            var side = HandIdentifier.Left;
            var poseController = rig.GetComponentInParent<HandPoseController>();
            if (poseController != null && poseController.Hand == HandIdentifier.Right)
            {
                side = HandIdentifier.Right;
            }

            var distals = side == HandIdentifier.Right ? RightDistals : LeftDistals;
            var intermediates = side == HandIdentifier.Right ? RightIntermediates : LeftIntermediates;

            var fingers = new HandFingerRig.FingerBones[5];
            int resolved = 0;
            for (int f = 0; f < 5; f++)
            {
                var distal = animator.GetBoneTransform(distals[f]);
                var mid = animator.GetBoneTransform(intermediates[f]);

                fingers[f] = new HandFingerRig.FingerBones
                {
                    // Prefer an explicit tip child of the distal bone; fall back to the distal joint.
                    tip = distal != null && distal.childCount > 0 ? distal.GetChild(0) : distal,
                    mid = mid,
                    radius = 0.008f
                };

                if (fingers[f].tip != null) resolved++;
            }

            Undo.RecordObject(rig, "Auto-Populate Finger Rig");
            rig.SetFingers(fingers);
            EditorUtility.SetDirty(rig);

            Debug.Log($"[HandFingerRig] Auto-populated {resolved}/5 finger tips from the {side} humanoid hand.", rig);
        }
    }
}
