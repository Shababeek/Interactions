using UniRx;
using UnityEngine;

namespace Kandooz.Kuest
{
    [RequireComponent(typeof(StepEvenListener))]
    [AddComponentMenu("Kandooz/SequenceSystem/Actions/VoiceOverAction")]

    public class VoiceOverAction : MonoBehaviour
    {
        [SerializeField] private AudioClip clip;
        private AudioSource source;
        private bool started = false;
        private StepEvenListener listener;

        private void OnEnable()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.clip = clip;
            listener = GetComponent<StepEvenListener>();
            listener.OnStarted.Do(_ => source.Play())
                .Do(_ => started = true)
                .Subscribe()
                .AddTo(this);
            listener.OnFinished.Do(_ => source.Stop()).Subscribe().AddTo(this);
        }

        private void Update()
        {
            if (!started || source.isPlaying) return;
            listener.OnActionCompleted();
            started = false;
        }
    }
}