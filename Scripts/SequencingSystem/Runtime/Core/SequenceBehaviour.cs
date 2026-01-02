using System;
using System.Collections.Generic;
using Shababeek.Utilities;
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
        [HideInInspector] [SerializeField] private bool startOnSpace;
        [ReadOnly] [SerializeField] private bool started;
        [SerializeField] private bool starOnAwake = false;
        [HideInInspector] [SerializeField] private float delay = 1;
        [SerializeField] private UnityEvent onSequenceStarted;
        [SerializeField] private UnityEvent onSequenceCompleted;
        [HideInInspector] [SerializeField] internal List<StepEventPair> steps;
        [HideInInspector] [SerializeField] internal List<StepEventListener> stepListeners;
        public bool listner;
        

        public bool StarOnAwake => starOnAwake;
        private float _time = 0;

        private void Awake()
        {
            if (!listner) return;
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
            sequence.Steps[^1].OnRaisedData.Do(_ =>
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

        private  void OnEnable()
        {
            sequence.OnRaisedData.Where(status => status == SequenceStatus.Started).Do(_ => onSequenceStarted.Invoke()).Subscribe().AddTo(this);
            sequence.OnRaisedData.Where(status => status == SequenceStatus.Started).Do(_ => onSequenceCompleted.Invoke()).Subscribe().AddTo(this);
            if (StarOnAwake)   
                StartQuest();
        }


        private async void StartQuest()
        {
            await Awaitable.NextFrameAsync();
            Awaitable.WaitForSecondsAsync(delay);
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