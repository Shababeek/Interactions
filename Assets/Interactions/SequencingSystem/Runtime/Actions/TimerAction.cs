
using Kandooz.InteractionSystem.Core;
using UnityEngine;

namespace Kandooz.Kuest
{
    [RequireComponent(typeof(StepEvenListener))]
    [AddComponentMenu("Kandooz/SequenceSystem/Actions/TimerAction")]

    public class TimerAction : MonoBehaviour
    {
        [SerializeField] private float time;
        [ReadOnly] private float elapsed=0;
        private StepEvenListener listener;

        private void Awake()
        {
            listener = GetComponent<StepEvenListener>();
        }

        private void Update()
        {
            if(!listener.Current) return;
            elapsed += Time.deltaTime;
            if (elapsed >= time)
            {
                elapsed = 0;
                listener.OnActionCompleted();
            }
        }
    }
}