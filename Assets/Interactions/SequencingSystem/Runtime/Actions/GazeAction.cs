using UniRx;
using UnityEngine;

namespace Kandooz.Kuest
{
    [AddComponentMenu("Kandooz/SequenceSystem/Actions/Gaze Action")]

    [RequireComponent(typeof(StepEvenListener))]
    public class GazeAction : MonoBehaviour
    {
        private StepEvenListener listener;
        private bool started;
        private Transform player;

        private void Awake()
        {
            player = Camera.main.transform;
            listener = GetComponent<StepEvenListener>();
            listener.OnStarted.Do((_) => started = true).Subscribe().AddTo(this);
            listener.OnFinished.Do((_) => started = false).Subscribe().AddTo(this);
        }

        private void Update()
        {
            if (!started) return;
            var direction = (transform.position - player.position).normalized;
            if (Vector3.Dot(direction, player.forward) > .7f)
            {
                listener.OnActionCompleted();
            }
        }
    }
}