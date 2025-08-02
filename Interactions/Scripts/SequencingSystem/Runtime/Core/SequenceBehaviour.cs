using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shababeek.Core;
using UniRx;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Manages the execution of sequences in the sequencing system.
    /// Handles sequence lifecycle, timing, and analytics tracking.
    /// </summary>
    public class SequenceBehaviour : MonoBehaviour
    {
        [Tooltip("The sequence to be executed by this behaviour.")]
        [SerializeField] public Sequence sequence;
        
        [Tooltip("Whether to start the sequence when spacebar is pressed.")]
        [HideInInspector] [SerializeField] private bool startOnSpace;
        
        [Tooltip("Indicates whether the sequence has been started.")]
        [ReadOnly] [SerializeField] private bool started;

        [Tooltip("Whether to automatically start the sequence on awake.")]
        [SerializeField] private bool starOnAwake = false;
        
        [Tooltip("Delay before starting the sequence (in seconds).")]
        [HideInInspector] [SerializeField] private float delay = 0;
        
        [Tooltip("Event raised when the sequence starts.")]
        [SerializeField] private UnityEvent onSequenceStarted;
        
        [Tooltip("Event raised when the sequence completes.")]
        [SerializeField] private UnityEvent onSequenceCompleted;

        [Tooltip("Internal list of step event pairs for the sequence.")]
        [HideInInspector] [SerializeField] internal List<StepEventPair> steps;
        
        [Tooltip("Internal list of step event listeners for the sequence.")]
        [HideInInspector] [SerializeField] internal List<StepEventListener> stepListeners;
        
        [Tooltip("Whether to enable analytics tracking for this sequence.")]
        public bool listner;
        
        /// <summary>
        /// Indicates whether the sequence should start automatically on awake.
        /// </summary>
        /// <value>True if the sequence starts on awake, false otherwise.</value>
        public bool StarOnAwake => starOnAwake;
        private float _time = 0;

        private void Awake()
        {
            if (listner)
            {
                sequence.Steps[0].OnRaisedData.Do(_ =>
                {
                    try
                    {
                        var data = new Dictionary<string, object>();
                        _time = 0;
                        data.Add("time", Time.realtimeSinceStartup);
                        data.Add("type", true);
                        data.Add("name", sequence.name);
                        var result = Analytics.CustomEvent("started", data);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        throw;
                    }
                }).Subscribe().AddTo(this);
                sequence.Steps[sequence.Steps.Count - 1].OnRaisedData.Do(_ =>
                {
                    try
                    {

                        var data = new Dictionary<string, object>();
                        data.Add("time", Time.realtimeSinceStartup - _time);
                        data.Add("type", true);
                        data.Add("name", sequence.name);
                        Analytics.CustomEvent("started", data);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        throw;
                    }
                }).Subscribe().AddTo(this);
            }
        }

        private async void OnEnable()
        {
            sequence.OnRaisedData.Where(status => status == SequenceStatus.Started).Do(_ => onSequenceStarted.Invoke()).Subscribe().AddTo(this);
            sequence.OnRaisedData.Where(status => status == SequenceStatus.Started).Do(_ => onSequenceCompleted.Invoke()).Subscribe().AddTo(this);
            if (!StarOnAwake) return;
            while (delay > 0)
            {
                await Task.Yield();
                delay -= Time.deltaTime;
            }

            StartQuest();
        }

        /// <summary>
        /// Starts the sequence execution.
        /// Begins the sequence and marks it as started.
        /// </summary>
        /// <remarks>
        /// This method will begin the sequence immediately.
        /// If the sequence is already started, calling this method will have no effect.
        /// </remarks>
        public void StartQuest()
        {
            sequence.Begin();
            started = true;
        }

        private void Update()
        {
            if (startOnSpace && !started && Input.GetKeyDown(KeyCode.Space))
                StartQuest();
            else if (started && Input.GetKeyDown(KeyCode.Space)) sequence.CurrentStep.CompleteStep();
        }

        [Serializable]
        public class StepEventPair
        {
            public UnityEvent listeners;
            public Step step;

            public StepEventPair(Step step)
            {
                this.step = step;
            }
        }
    }
}