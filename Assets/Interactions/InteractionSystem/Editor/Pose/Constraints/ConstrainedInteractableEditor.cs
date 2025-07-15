using Kandooz.InteractionSystem.Animations;
using Kandooz.InteractionSystem.Core;
using Kandooz.InteractionSystem.Interactions;
using UnityEditor;
using UnityEngine;

namespace Kandooz.Interactions.Editors
{
    [CustomEditor(typeof(ConstrainedInteractableBase),true)]
    public class ConstrainedInteractableEditor : AbstractPoseConstraintEditor
    {
        private HandPoseController leftHandPose;
        private HandPoseController rightHandPose;
        protected override void DeselectHands()
        {
            interactable.LeftHandTransform.gameObject.SetActive(false);
            interactable.RightHandTransform.gameObject.SetActive(false);
        }
        protected override HandIdentifier SelectedHand
        {
            set
            {
                if (value == selectedHand) return;
                selectedHand = value;
                DeselectHands();

                switch (selectedHand)
                {
                    case HandIdentifier.Left:
                        interactable.LeftHandTransform.gameObject.SetActive(true);
                        currentHand = leftHandPose;
                        break;
                    case HandIdentifier.Right:
                        interactable.RightHandTransform.gameObject.SetActive(true);
                        currentHand = rightHandPose;
                        break;
                }
            }
        }
        protected override void ShowPoseInspector()
        {
            var propertyName = selectedHand switch
            {
                HandIdentifier.Left => "leftConstraints",
                HandIdentifier.Right => "rightConstraints",
                _ => ""
            };
            if (propertyName == "") return;
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName));
            serializedObject.ApplyModifiedProperties();

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            leftHandPose = interactable.LeftHandTransform.GetComponentInChildren<HandPoseController>(true)
                ? interactable.LeftHandTransform.GetComponentInChildren<HandPoseController>(true)
                : CreateHandInPivot(interactable.LeftHandTransform, leftHandPrefab);

            rightHandPose = interactable.RightHandTransform.GetComponentInChildren<HandPoseController>(true)
                ? interactable.RightHandTransform.GetComponentInChildren<HandPoseController>(true)
                : CreateHandInPivot(interactable.RightHandTransform, rightHandPrefab);
        }
    }
}