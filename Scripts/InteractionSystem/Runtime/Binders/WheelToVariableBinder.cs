using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds a WheelInteractable's output to scriptable variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Wheel To Variable Binder")]
    public class WheelToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private WheelInteractable wheel;

        [Header("Output Variables")]
        [SerializeField] private FloatVariable normalizedOutput;
        [SerializeField] private FloatVariable angleOutput;

        [Header("Settings")]
        [SerializeField] private bool invertOutput = false;
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
