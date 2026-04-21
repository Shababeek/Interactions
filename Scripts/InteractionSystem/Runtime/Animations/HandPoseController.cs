using System.Collections.Generic;
using Shababeek.ReactiveVars;
using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Drives hand pose and finger curls. In play mode it polls the sibling Hand; in edit mode it
    /// reads the serialized fingers array. Muscle-based mode drives both hands' finger muscles off
    /// the same weights and pins the chain from hips to the selected Hand bone at the animator root.
    /// </summary>
    [RequireComponent(typeof(VariableTweener))]
    [AddComponentMenu("Shababeek/Interactions/Animations/Hand Pose Controller")]
    [ExecuteAlways]
    public class HandPoseController : MonoBehaviour, IPoseable
    {
        [Header("Hand Configuration")]
        [HideInInspector] [SerializeField] [Tooltip("The HandData asset containing all pose definitions and finger configurations.")]
        private HandData handData;

        [Tooltip("Which hand this controller represents. Used only to pick the pin bone in muscle-based mode; finger muscles are driven on both hands regardless.")]
        [SerializeField] private HandIdentifier hand = HandIdentifier.Left;

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
        private HumanPoseHandler _humanPoseHandler;
        private HumanPose _humanPose;
        private Transform[] _pinChainTopDown;
        private HandPoseSystem _activePoseSystem = HandPoseSystem.LegacyBoneBased;

        /// <summary>Finger curl value (0 = extended, 1 = curled) by finger name.</summary>
        public float this[FingerName index]
        {
            get => this[(int)index];
            set => this[(int)index] = value;
        }

        /// <summary>Finger curl value (0 = extended, 1 = curled) by index (0=Thumb..4=Pinky).</summary>
        public float this[int index]
        {
            get => fingers[index];
            set
            {
                fingers[index] = value;
                if (_poses == null || currentPoseIndex < 0 || currentPoseIndex >= _poses.Count) return;
                _poses[currentPoseIndex][index] = value;
            }
        }

        /// <summary>Sets the active pose index, clamped to the HandData pose range.</summary>
        public int Pose
        {
            set
            {
                if (handData == null || handData.Poses == null || handData.Poses.Length == 0)
                {
                    Debug.LogWarning($"[HandPoseController] Cannot set pose on {gameObject.name}: HandData or Poses is null/empty", this);
                    currentPoseIndex = 0;
                    return;
                }

                if (value < 0 || value >= handData.Poses.Length)
                {
                    Debug.LogWarning($"[HandPoseController] Pose index {value} is out of bounds on {gameObject.name}. Valid range: 0-{handData.Poses.Length - 1}. Defaulting to 0.", this);
                    currentPoseIndex = 0;
                }
                else
                {
                    currentPoseIndex = value;
                }
            }
        }

        /// <summary>Pose constraints applied to the polled Hand each Update in play mode.</summary>
        public PoseConstrains Constrains
        {
            set => _constrains = value;
        }

        /// <summary>Active pose index; setter blends mixer weights and pushes finger values.</summary>
        public int CurrentPoseIndex
        {
            get => currentPoseIndex;
            set
            {
                if (_handMixer.IsValid())
                {
                    _handMixer.SetInputWeight(currentPoseIndex, 0);
                    _handMixer.SetInputWeight(value, 1);
                }
                currentPoseIndex = value;
                if (_poses == null || value < 0 || value >= _poses.Count) return;
                for (int finger = 0; finger < fingers.Length; finger++)
                {
                    _poses[value][finger] = fingers[finger];
                }
            }
        }

        /// <summary>HandData asset driving this controller.</summary>
        public HandData HandData
        {
            get => handData;
            set => handData = value;
        }

        /// <summary>Which hand this controller represents; picks the pin bone in muscle-based mode.</summary>
        public HandIdentifier Hand
        {
            get => hand;
            set => hand = value;
        }

        /// <summary>Underlying Playable Graph (legacy mode) or sentinel graph (muscle mode).</summary>
        public PlayableGraph Graph => _graph;

        /// <summary>All poses built from HandData.</summary>
        public List<IPose> Poses => _poses;

        public void Start()
        {
            Initialize();
        }

        /// <summary>Builds the pose graph and wires it to the hand's Animator. Safe to call repeatedly.</summary>
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
            _activePoseSystem = handData.PoseSystem;
            if (_activePoseSystem == HandPoseSystem.MuscleBased)
            {
                InitializeMuscleBasedSystem();
            }
            else
            {
                CreateGraphAndSetItsOutputs();
                InitializePoses();
            }
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

        private void InitializeMuscleBasedSystem()
        {
            if (_animator == null || _animator.avatar == null || !_animator.avatar.isHuman)
            {
                Debug.LogError($"[HandPoseController] MuscleBased pose system on {gameObject.name} requires a Humanoid Avatar on the Animator.", this);
                _graph = PlayableGraph.Create(this.name);
                _poses = new List<IPose>();
                return;
            }

            _graph = PlayableGraph.Create(this.name);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            DisposeHumanPoseHandler();
            _humanPoseHandler = new HumanPoseHandler(_animator.avatar, _animator.transform);
            _humanPose = new HumanPose();

            var pinSide = hand == HandIdentifier.None ? HandIdentifier.Left : hand;
            var handHumanBone = pinSide == HandIdentifier.Right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            var handBone = _animator.GetBoneTransform(handHumanBone);
            if (handBone == null)
            {
                Debug.LogWarning($"[HandPoseController] Could not resolve {handHumanBone} on the humanoid avatar of {gameObject.name}.", this);
            }
            _pinChainTopDown = BuildPinChainTopDown(handBone, _animator.transform);

            _poses = new List<IPose>(handData.Poses.Length);
            for (int i = 0; i < handData.Poses.Length; i++)
            {
                var data = handData.Poses[i];
                _poses.Add(CreateMuscleBasedPose(data));
            }
        }

        private IPose CreateMuscleBasedPose(PoseData data)
        {
            bool hasOpenClip = data.OpenAnimationClip != null;
            bool hasClosedClip = data.ClosedAnimationClip != null;

            if (data.Type == PoseData.PoseType.Dynamic)
            {
                if (!hasOpenClip && !hasClosedClip)
                    return new MuscleBasedProceduralDynamicPose(data.Name);
                return new MuscleBasedDynamicPose(data, _animator);
            }

            return new MuscleBasedStaticPose(data, _animator);
        }

        private void ApplyMuscleWrites()
        {
            if (_humanPoseHandler == null || _poses == null || _poses.Count == 0) return;
            if (currentPoseIndex < 0 || currentPoseIndex >= _poses.Count) return;

            // Read the live pose so any upstream muscle writer passes through; we only overwrite
            // finger muscles. PinHandToAnimatorRoot resets the hips-through-hand chain each frame,
            // so any body-position drift from Get/Set is overwritten there.
            _humanPoseHandler.GetHumanPose(ref _humanPose);

            switch (_poses[currentPoseIndex])
            {
                case MuscleBasedDynamicPose dyn: dyn.WriteTo(ref _humanPose); break;
                case MuscleBasedProceduralDynamicPose procDyn: procDyn.WriteTo(ref _humanPose); break;
                case MuscleBasedStaticPose stat: stat.WriteTo(ref _humanPose); break;
            }

            _humanPoseHandler.SetHumanPose(ref _humanPose);
        }

        private void DisposeHumanPoseHandler()
        {
            if (_humanPoseHandler != null)
            {
                _humanPoseHandler.Dispose();
                _humanPoseHandler = null;
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
            if (Application.isPlaying) PullFingersFromHand();
        }

        private void PullFingersFromHand()
        {
            if (_hand == null || handData == null || handData.Poses == null || handData.Poses.Length == 0) return;

            Pose = _constrains[0].pose;
            for (int i = 0; i < 5; i++)
            {
                this[i] = _constrains[i].constraints.GetConstrainedValue(_hand[i]);
            }
        }

        /// <summary>Pushes inspector finger values into the active pose and evaluates the graph. Muscle writes happen in LateUpdate.</summary>
        public void UpdateGraphVariables()
        {
            if (!_graph.IsValid())
            {
                Debug.LogWarning($"[HandPoseController] Graph became invalid on {gameObject.name}, reinitializing...", this);
                DisposeGraph();
                InitializeGraph();
            }

            for (int i = 0; i < fingers.Length; i++)
            {
                this[i] = fingers[i];
            }

            CurrentPoseIndex = currentPoseIndex;
            _graph.Evaluate();
        }

        private void LateUpdate()
        {
            if (_activePoseSystem != HandPoseSystem.MuscleBased) return;
            if (!_graph.IsValid()) return;
            ApplyMuscleWrites();
            PinHandToAnimatorRoot();
        }

        private void PinHandToAnimatorRoot()
        {
            if (_animator == null || _pinChainTopDown == null || _pinChainTopDown.Length == 0) return;

            // Walk top-down so each child write computes against an already-pinned parent and the
            // whole arm chain collapses to the animator root with zero local offsets.
            var root = _animator.transform;
            for (int i = 0; i < _pinChainTopDown.Length; i++)
            {
                var bone = _pinChainTopDown[i];
                if (bone == null) continue;
                bone.SetPositionAndRotation(root.position, root.rotation);
            }
        }

        private static Transform[] BuildPinChainTopDown(Transform pinBone, Transform animatorRoot)
        {
            if (pinBone == null || animatorRoot == null) return System.Array.Empty<Transform>();

            var stack = new Stack<Transform>();
            var cursor = pinBone;
            while (cursor != null && cursor != animatorRoot)
            {
                stack.Push(cursor);
                cursor = cursor.parent;
            }
            return stack.ToArray();
        }

        private void DisposeGraph()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        private void OnDestroy()
        {
            DisposeGraph();
            DisposeHumanPoseHandler();
        }
    }
}
