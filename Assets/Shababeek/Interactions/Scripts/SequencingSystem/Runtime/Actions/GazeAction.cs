using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/Actions/GazeAction")]
    public class GazeAction : AbstractSequenceAction
    {
        private StepEventListener listener;
        private Collider collider;
        private Transform player;

        private void Awake()
        {
            collider = GetComponent<Collider>();
            player = Camera.main.transform;
            listener = GetComponent<StepEventListener>();
        }

        private void Update()
        {
            if (!Started) return;
            var ray = new Ray(player.position, player.forward);
            RaycastHit hitInfo;
            if (collider.Raycast(ray, out hitInfo, 20))
            {
                listener.OnActionCompleted();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
        }
    }
}