using Kandooz.InteractionSystem.Animations;
using Kandooz.InteractionSystem.Core;
using Kandooz.InteractionSystem.Interactions;
using UnityEditor;

namespace Kandooz.Interactions.Editors
{
    [CustomEditor(typeof(InteractionPoseConstrainer), true)]
    internal class PoseConstrainterEditor : AbstractPoseConstraintEditor
    {
        protected override HandIdentifier SelectedHand
        {
            set
            {
                selectedHand = value;
                DeselectHands();
                switch (selectedHand)
                {
                    case HandIdentifier.Left:
                        currentHand = CreateHandInPivot(interactable.LeftHandTransform, leftHandPrefab);
                        break;
                    case HandIdentifier.Right:
                        currentHand = CreateHandInPivot(interactable.RightHandTransform, rightHandPrefab);
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
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName).FindPropertyRelative("poseConstrains"));
            serializedObject.ApplyModifiedProperties();

        }
        protected override void DeselectHands()
        {
            {
                var rightHandInteractor = interactable.RightHandTransform.GetComponentInChildren<HandPoseController>();
                var leftHandInteractor = interactable.LeftHandTransform.GetComponentInChildren<HandPoseController>();
                if (currentHand) DestroyImmediate(currentHand.gameObject);
                if (rightHandInteractor != null) DestroyImmediate(rightHandInteractor.gameObject);
                if (leftHandInteractor != null) DestroyImmediate(leftHandInteractor.gameObject);
            }
        }
    }
}