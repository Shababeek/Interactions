using System.Collections.Generic;
using Kandooz.InteractionSystem.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kandooz.InteractionSystem.Animations
{
    /// <summary>
    /// Control the pose of the hand and it's fingers
    /// </summary>
    [RequireComponent(typeof(VariableTweener))]
    public class HandPoseController : MonoBehaviour, IPoseable
    {
        #region private variables

        [HideInInspector] [SerializeField] private HandPoseData handData;

        [Range(0, 1)] [HideInInspector] [SerializeField]
        private float[] fingers = new float[5];

        [HideInInspector] [SerializeField] private int currentPoseIndex;
        private List<IPose> poses;
        private Hand hand;
        private VariableTweener variableTweener;
        private AnimationMixerPlayable handMixer;
        private Animator animator;
        PlayableGraph graph;
        private PoseConstrains constrains = PoseConstrains.Free;

        #endregion
        public float this[FingerName index]
        {
            get => this[(int)index];
            set => this[(int)index] = value;
        }
        public float this[int index]
        {
            get => fingers[index];
            set
            {
                fingers[index] = value;
                poses[currentPoseIndex][index] = value;
            }
        }

        public int Pose
        {
            set => currentPoseIndex = value;
        }
        

        public PoseConstrains Constrains
        {
            set => constrains = value;
        }
        public int CurrentPoseIndex
        {
            get => currentPoseIndex;
            set
            {
                handMixer.SetInputWeight(currentPoseIndex, 0);
                handMixer.SetInputWeight(value, 1);
                currentPoseIndex = value;
                for (int finger = 0; finger < fingers.Length; finger++)
                {
                    poses[value][finger] = fingers[finger];
                }
            }
        }

        public HandPoseData HandData => handData;
        public PlayableGraph Graph => graph;
        public List<IPose> Poses => poses;

        public void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (!handData)
            {
                Debug.LogError("please select a hand data object");
                return;
            }

            GetDependencies();
            InitializeGraph();
        }

        private void GetDependencies()
        {
            variableTweener = GetComponent<VariableTweener>();
            animator = GetComponentInChildren<Animator>();
            hand = GetComponent<Hand>();
            if (!animator)
            {
                Debug.LogError("Please add animator to the object or it's children");
            }
        }

        private void InitializeGraph()
        {
            CreateGraphAndSetItsOutputs();
            InitializePoses();
            graph.Play();
        }

        private void CreateGraphAndSetItsOutputs()
        {
            graph = PlayableGraph.Create(this.name);
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            handMixer = AnimationMixerPlayable.Create(graph, handData.Poses.Length);
            var playableOutput = AnimationPlayableOutput.Create(graph, "Hand mixer", animator);
            playableOutput.SetSourcePlayable(handMixer);
        }

        private void InitializePoses()
        {
            poses = new List<IPose>(handData.Poses.Length + 1);
            for (int i = 0; i < handData.Poses.Length; i++)
            {
                CreateAndConnectPose(i, handData.Poses[i]);
            }
        }

        private void CreateAndConnectPose(int poseID, PoseData data)
        {
            IPose pose = data.Type == PoseData.PoseType.Dynamic ? CreateDynamicPose(poseID, data) : CreateStaticPose(poseID, data);
            poses.Add(pose);
        }

        private IPose CreateStaticPose(int poseID, PoseData data)
        {
            var pose = new StaticPose(graph, data);
            graph.Connect(pose.Mixer, 0, handMixer, poseID);
            return pose;
        }

        private IPose CreateDynamicPose(int poseID, PoseData data)
        {
            var pose = new DynamicPose(graph, data, handData, variableTweener);
            graph.Connect(pose.PoseMixer, 0, handMixer, poseID);
            pose.PoseMixer.SetInputWeight(0, 1);
            return pose;
        }

        private void Update()
        {
            UpdateGraphVariables();
            UpdateFingersFromHand();
        }

        public void UpdateGraphVariables()
        {
            if (!graph.IsValid())
            {
                InitializeGraph();
            }

            for (int i = 0; i < fingers.Length; i++)
            {
                this[i] = fingers[i];
            }

            CurrentPoseIndex = currentPoseIndex;
            graph.Evaluate();
        }

        private void UpdateFingersFromHand()
        {
            for (int i = 0; i < 5; i++)
            {
                this[i] = constrains[i].GetConstrainedValue(hand[i]);
            }
        }

    }
}