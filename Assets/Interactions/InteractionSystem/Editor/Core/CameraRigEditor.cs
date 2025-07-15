using System;
using UnityEditor;

namespace Kandooz.InteractionSystem.Core.Editors
{
    [CustomEditor(typeof(CameraRig))]
    public class CameraRigEditor : Editor
    {
        private CameraRig rig;

        private void OnEnable()
        {
            rig = (CameraRig)target;
            var hands = rig.GetComponentsInChildren<Hand>();
            SetHandSourceIfEmpty(serializedObject.FindProperty("leftHand"), hands,  HandIdentifier.Left);
            SetHandSourceIfEmpty(serializedObject.FindProperty("rightHand"), hands,  HandIdentifier.Right);
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
        }

        private static void SetHandSourceIfEmpty(SerializedProperty handSerializedProperty, Hand[] hands, HandIdentifier handSource)
        {
            if (handSerializedProperty.objectReferenceValue == null)
            {
                var handObject = GetHandObject(hands, handSource);
                handSerializedProperty.objectReferenceValue = handObject;
            }
        }

        private static Hand GetHandObject(Hand[] hands,HandIdentifier handIdentifier)
        {
            Hand handObject = null;
            foreach (var hand in hands)
            {
                if (hand.HandIdentifier ==handIdentifier)
                {
                    handObject = hand;
                }
            }

            return handObject;
        }
    }
}