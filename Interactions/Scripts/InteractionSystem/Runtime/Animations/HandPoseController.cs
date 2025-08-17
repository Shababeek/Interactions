using System.Collections.Generic;
using Shababeek.Utilities;
using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Controls the pose and finger animations of a VR hand using Unity's Playable Graph system.
    /// This component manages hand pose transitions, finger curl values, and constraint application
    /// through a sophisticated animation mixing system that supports both static and dynamic poses.
    /// </summary>
    /// <remarks>
    /// The HandPoseController implements the IPoseable interface and works with HandData to manage
    /// multiple pose states. It uses Unity's Playable Graph API for efficient animation blending
    /// and supports both static poses (pre-defined animations) and dynamic poses (real-time finger control).
    /// The system automatically applies pose constraints and updates finger values based on input data.
    /// </remarks>
    [RequireComponent(typeof(VariableTweener))]
    public class HandPoseController : MonoBehaviour, IPoseable
    {

        [HideInInspector] [SerializeField] private HandData handData;

        [Range(0, 1)] [HideInInspector] [SerializeField]
        private float[] fingers = new float[5];

        [HideInInspector] [SerializeField] private int currentPoseIndex;
        private List<IPose> _poses;
        private Hand _hand;
        private VariableTweener _variableTweener;
        private AnimationMixerPlayable _handMixer;
        private Animator _animator;
        PlayableGraph _graph;
        private PoseConstrains _constrains = PoseConstrains.Free;

        /// <summary>
        /// Indexer that provides access to finger values by finger name.
        /// Allows getting and setting individual finger curl values (0 = extended, 1 = curled).
        /// </summary>
        /// <param name="index">The finger to access (Thumb, Index, Middle, Ring, Pinky).</param>
        /// <returns>The current curl value for the specified finger.</returns>
        /// <value>The curl value to set for the specified finger.</value>
        public float this[FingerName index]
        {
            get => this[(int)index];
            set => this[(int)index] = value;
        }
        
        /// <summary>
        /// Indexer that provides access to finger values by numeric index.
        /// Allows getting and setting individual finger curl values (0 = extended, 1 = curled).
        /// </summary>
        /// <param name="index">The finger index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</param>
        /// <returns>The current curl value for the specified finger.</returns>
        /// <value>The curl value to set for the specified finger.</value>
        public float this[int index]
        {
            get => fingers[index];
            set
            {
                fingers[index] = value;
                _poses[currentPoseIndex][index] = value;
            }
        }

        /// <summary>
        /// Sets the current pose index for the hand animation system.
        /// This determines which pose from the HandData is currently active.
        /// </summary>
        /// <value>The index of the pose to activate.</value>
        public int Pose
        {
            set => currentPoseIndex = value;
        }
        
        /// <summary>
        /// Sets the pose constraints that limit finger movement and curl values.
        /// Constraints are applied to each finger individually and can restrict movement
        /// based on interaction requirements or physical limitations.
        /// </summary>
        /// <value>The pose constraints to apply to the hand.</value>
        public PoseConstrains Constrains
        {
            set => _constrains = value;
        }



        /// <summary>
        /// Gets or sets the current pose index with proper animation blending.
        /// When setting a new pose, the system smoothly transitions between poses
        /// by adjusting the animation mixer weights and updating finger values.
        /// </summary>
        /// <value>The index of the currently active pose.</value>
        /// <returns>The index of the currently active pose.</returns>
        public int CurrentPoseIndex
        {
            get => currentPoseIndex;
            set
            {
                _handMixer.SetInputWeight(currentPoseIndex, 0);
                _handMixer.SetInputWeight(value, 1);
                currentPoseIndex = value;
                for (int finger = 0; finger < fingers.Length; finger++)
                {
                    _poses[value][finger] = fingers[finger];
                }
            }
        }

        /// <summary>
        /// Gets or sets the HandData object that contains all pose definitions and finger configurations.
        /// HandData defines the available poses, their types (static or dynamic), and finger joint mappings.
        /// </summary>
        /// <value>The HandData object containing pose definitions.</value>
        /// <returns>The HandData object containing pose definitions.</returns>
        public HandData HandData
        {
            get => handData;
            set => handData = value;
        }

        /// <summary>
        /// Gets the Playable Graph used for animation management.
        /// The graph contains all animation playables and manages the animation system's execution.
        /// </summary>
        /// <returns>The Playable Graph instance.</returns>
        public PlayableGraph Graph => _graph;
        
        /// <summary>
        /// Gets the list of all available poses for this hand.
        /// Each pose can be either static (pre-defined animation) or dynamic (real-time control).
        /// </summary>
        /// <returns>A list of all pose instances.</returns>
        public List<IPose> Poses => _poses;

        public void Start()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initializes the hand pose controller and sets up the animation system.
        /// This method creates the Playable Graph, initializes poses, and establishes
        /// the connection between the animation system and the hand's animator component.
        /// </summary>
        /// <remarks>
        /// The initialization process includes:
        /// 1. Validating that HandData is assigned
        /// 2. Getting required component dependencies
        /// 3. Creating the Playable Graph and animation mixer
        /// 4. Initializing all poses from HandData
        /// 5. Starting the animation graph
        /// </remarks>
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
            _variableTweener = GetComponent<VariableTweener>();
            _animator = GetComponentInChildren<Animator>();
            _hand = GetComponent<Hand>();
            if (!_animator)
            {
                Debug.LogError("Please add animator to the object or it's children");
            }
        }

        private void InitializeGraph()
        {
            CreateGraphAndSetItsOutputs();
            InitializePoses();
            _graph.Play();
        }

        private void CreateGraphAndSetItsOutputs()
        {
            _graph = PlayableGraph.Create(this.name);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _handMixer = AnimationMixerPlayable.Create(_graph, handData.Poses.Length);
            var playableOutput = AnimationPlayableOutput.Create(_graph, "Hand mixer", _animator);
            playableOutput.SetSourcePlayable(_handMixer);
        }

        private void InitializePoses()
        {
            _poses = new List<IPose>(handData.Poses.Length + 1);
            for (int i = 0; i < handData.Poses.Length; i++)
            {
                CreateAndConnectPose(i, handData.Poses[i]);
            }
        }

        private void CreateAndConnectPose(int poseID, PoseData data)
        {
            IPose pose = data.Type == PoseData.PoseType.Dynamic ? CreateDynamicPose(poseID, data) : CreateStaticPose(poseID, data);
            _poses.Add(pose);
        }

        private IPose CreateStaticPose(int poseID, PoseData data)
        {
            var pose = new StaticPose(_graph, data);
            _graph.Connect(pose.Mixer, 0, _handMixer, poseID);
            return pose;
        }

        private IPose CreateDynamicPose(int poseID, PoseData data)
        {
            var pose = new DynamicPose(_graph, data, handData, _variableTweener);
            _graph.Connect(pose.PoseMixer, 0, _handMixer, poseID);
            pose.PoseMixer.SetInputWeight(0, 1);
            return pose;
        }

        private void Update()
        {
            UpdateGraphVariables();
            UpdateFingersFromHand();
        }

        /// <summary>
        /// Updates the animation graph variables and evaluates the current pose.
        /// This method ensures the Playable Graph is valid, updates finger values,
        /// and evaluates the animation system to apply the current pose.
        /// </summary>
        /// <remarks>
        /// If the graph becomes invalid (e.g., after domain reload), it automatically reinitializes.
        /// This method also ensures that finger values are properly synchronized with the current pose.
        /// </remarks>
        public void UpdateGraphVariables()
        {
            if (!_graph.IsValid())
            {
                InitializeGraph();
            }

            for (int i = 0; i < fingers.Length; i++)
            {
                this[i] = fingers[i];
            }

            CurrentPoseIndex = currentPoseIndex;
            _graph.Evaluate();
        }

        /// <summary>
        /// Updates finger values from the hand input system and applies constraints.
        /// This method reads finger values from the Hand component and applies any
        /// pose constraints to limit the finger movement based on interaction requirements.
        /// </summary>
        /// <remarks>
        /// The method iterates through all five fingers and applies constraints to each finger
        /// individually, ensuring that finger movement is properly limited when constraints are active.
        /// </remarks>
        private void UpdateFingersFromHand()
        {
            var pose = _constrains[0].pose;
            if (pose < 0 || pose >= handData.Poses.Length)
            {
                pose = 0;
            }

            Pose = pose;
            for (var i = 0; i < 5; i++)
            {
                this[i] = _constrains[i].constraints.GetConstrainedValue(_hand[i]);
            }
        }

    }
}