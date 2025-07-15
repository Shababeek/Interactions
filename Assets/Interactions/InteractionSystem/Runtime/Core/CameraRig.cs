using System;
using Kandooz.InteractionSystem.Animations;
using Kandooz.InteractionSystem.Interactions;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public enum InteractionSystemType
    {
        TransformBased,
        PhysicsBased
    }

    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private Transform offsetObject;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private InteractionSystemType interactionSystemType;
        [SerializeField] private HandPoseData poseData;
        [SerializeField] private Config config;
        [SerializeField] private float playerHeight;
        [SerializeField] private bool initializeLayers;

        private HandPoseController leftPoseController, rightPoseController;
        public HandPoseController LeftHandPrefab => poseData.LeftHandPrefab;
        public HandPoseController RightHandPrefab => poseData.RightHandPrefab;
        public Config Config => config;

        private void Awake()
        {
            switch (interactionSystemType)
            {
                case InteractionSystemType.TransformBased:

                    break;
                case InteractionSystemType.PhysicsBased:
                    InitializePhysicsBasedHands();
                    break;
            }
            if(!initializeLayers)return; 
                
            ChangeLayerRecursive(transform, config.PlayerLayer);
            ChangeLayerRecursive(leftHand, config.LeftHandLayer);
            ChangeLayerRecursive(rightHand, config.RightHandLayer);
        }

        static void ChangeLayerRecursive(Transform transform, int layer)
        {

            transform.gameObject.layer = layer;
            for (int i = 0; i < transform.childCount; i++)
            {
                ChangeLayerRecursive(transform.GetChild(i), layer);
            }
        }

        private void InitializePhysicsBasedHands()
        {
            InitializeHands();
        }

        private void InitializeHands()
        {
            rightPoseController = InitializeHand(RightHandPrefab, rightHand, HandIdentifier.Right);
            leftPoseController = InitializeHand(LeftHandPrefab, leftHand, HandIdentifier.Left);
            InitializePhysics(rightPoseController.gameObject, rightHand);
            InitializePhysics(leftPoseController.gameObject, leftHand);
        }

        private static void InitializePhysics(GameObject hand, Transform target)
        {
            // TODO: the values should be moved to the config file 

            var rb = hand.GetComponent<Rigidbody>();
            if (rb == null) rb = hand.AddComponent<Rigidbody>();
            rb.mass = 40;
            rb.linearDamping = 5;
            rb.angularDamping = 1;
            var follower = hand.AddComponent<PhysicsHandFollwer>();
            follower.Target = target;
        }

        private HandPoseController InitializeHand(HandPoseController handPrefab, Transform handPivot, HandIdentifier handIdentifier)
        {
            var poseController = handPivot.GetComponentInChildren<HandPoseController>();
            if (poseController != null) return poseController;

            poseController = Instantiate(handPrefab, handPivot);
            poseController.transform.localPosition = Vector3.zero;
            poseController.transform.localRotation = Quaternion.identity;
            var hand = poseController.gameObject;
            var handController = hand.AddComponent<Hand>();
            handController.HandIdentifier = handIdentifier;
            handController.Config = config;
            hand.AddComponent<TriggerInteractor>();
            return poseController;
        }
    }
}