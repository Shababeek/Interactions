using Shababeek.Utilities;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a LeverInteractable's output to scriptable variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Lever To Variable Binder")]
    public class LeverToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private LeverInteractable lever;

        [Header("Output Variables")]
        [SerializeField] private FloatVariable normalizedOutput;
        [SerializeField] private FloatVariable angleOutput;

        [Header("Settings")]
        [SerializeField] private bool invertOutput = false;
        [SerializeField] private float outputMultiplier = 1f;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (lever == null) lever = GetComponent<LeverInteractable>();
            if (lever == null) return;

            _disposable = new CompositeDisposable();

            lever.OnLeverChanged
                .Subscribe(OnLeverChanged)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnLeverChanged(float normalizedValue)
        {
            float value = invertOutput ? (1f - normalizedValue) : normalizedValue;
            value *= outputMultiplier;

            if (normalizedOutput != null)
                normalizedOutput.Value = value;

            if (angleOutput != null)
                angleOutput.Value = lever.CurrentAngle * (invertOutput ? -1f : 1f);
        }
    }
}
