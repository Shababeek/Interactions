using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Writes a LeverInteractable's output to scriptable variables.</summary>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Lever To Variable Driver")]
    public class LeverToVariableDriver : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("Source lever interactable.")]
        [SerializeField] private LeverInteractable lever;

        [Header("Output Variables")]
        [Tooltip("Float variable to receive the normalized lever position (0-1).")]
        [SerializeField] private FloatVariable normalizedOutput;
        [Tooltip("Float variable to receive the lever angle in degrees.")]
        [SerializeField] private FloatVariable angleOutput;

        [Header("Settings")]
        [Tooltip("Invert the output values.")]
        [SerializeField] private bool invertOutput = false;
        [Tooltip("Multiplier applied to the output values.")]
        [SerializeField] private float outputMultiplier = 1f;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (lever == null) lever = GetComponent<LeverInteractable>();
            if (lever == null) return;

            _disposable = new CompositeDisposable();

            OnLeverChanged(lever.CurrentNormalizedAngle);

            lever.OnLeverChanged
                .Subscribe(OnLeverChanged)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnLeverChanged(float normalizedValue)
        {
            float sign = invertOutput ? -1f : 1f;

            if (normalizedOutput != null)
                normalizedOutput.Value = (invertOutput ? (1f - normalizedValue) : normalizedValue) * outputMultiplier;

            if (angleOutput != null)
                angleOutput.Value = lever.CurrentAngle * sign * outputMultiplier;
        }
    }
}
