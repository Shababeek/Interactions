using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Binds a WheelInteractable's output to scriptable variables.</summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Wheel To Variable Binder")]
    public class WheelToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("The wheel interactable to bind from.")]
        [SerializeField] private WheelInteractable wheel;

        [Header("Output Variables")]
        [Tooltip("Float variable to receive the normalized wheel rotation (0-1).")]
        [SerializeField] private FloatVariable normalizedOutput;
        [Tooltip("Float variable to receive the wheel angle in degrees.")]
        [SerializeField] private FloatVariable angleOutput;

        [Header("Settings")]
        [Tooltip("Invert the output values.")]
        [SerializeField] private bool invertOutput = false;
        [Tooltip("Multiplier applied to the output values.")]
        [SerializeField] private float outputMultiplier = 1f;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (wheel == null) wheel = GetComponent<WheelInteractable>();
            if (wheel == null) return;

            _disposable = new CompositeDisposable();

            wheel.OnNormalizedChanged
                .Subscribe(OnNormalizedChanged)
                .AddTo(_disposable);

            wheel.OnAngleChanged
                .Subscribe(OnAngleChanged)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnNormalizedChanged(float value)
        {
            if (normalizedOutput == null) return;
            float output = invertOutput ? -value : value;
            normalizedOutput.Value = output * outputMultiplier;
        }

        private void OnAngleChanged(float angle)
        {
            if (angleOutput == null) return;
            float output = invertOutput ? -angle : angle;
            angleOutput.Value = output * outputMultiplier;
        }
    }
}
