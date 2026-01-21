using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Completes a step when the player gazes at a collider.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/GazeAction")]
    public class GazeAction : AbstractSequenceAction
    {
        [Tooltip("The collider to check for gaze. If not set, uses the collider on this GameObject.")]
        [SerializeField] private Collider targetCollider;

        [Tooltip("The transform to use as the gaze origin. If not set, uses Camera.main.")]
        [SerializeField] private Transform gazeOrigin;

        [Tooltip("Maximum distance for gaze detection.")]
        [SerializeField] private float maxDistance = 20f;

        [Tooltip("Duration the player must gaze at the target before completing (0 = instant).")]
        [SerializeField] private float requiredGazeDuration = 0f;

        private float _currentGazeTime = 0f;

        private void Awake()
        {
            if (targetCollider == null)
                targetCollider = GetComponent<Collider>();

            if (gazeOrigin == null && Camera.main != null)
                gazeOrigin = Camera.main.transform;
        }

        private void Update()
        {
            if (!Started || gazeOrigin == null || targetCollider == null) return;

            var ray = new Ray(gazeOrigin.position, gazeOrigin.forward);
            if (targetCollider.Raycast(ray, out _, maxDistance))
            {
                _currentGazeTime += Time.deltaTime;
                if (_currentGazeTime >= requiredGazeDuration)
                {
                    CompleteStep();
                }
            }
            else
            {
                _currentGazeTime = 0f;
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _currentGazeTime = 0f;
            }
        }
    }
}