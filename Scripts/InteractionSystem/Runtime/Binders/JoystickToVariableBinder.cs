using Shababeek.Utilities;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a JoystickInteractable's output to scriptable variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Joystick To Variable Binder")]
    public class JoystickToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private JoystickInteractable joystick;

        [Header("Output Variables")]
        [SerializeField] private Vector2Variable vector2Output;
        [SerializeField] private FloatVariable xOutput;
        [SerializeField] private FloatVariable yOutput;

        [Header("Settings")]
        [SerializeField] private bool invertX = false;
        [SerializeField] private bool invertY = false;
        [SerializeField] private float deadzone = 0.1f;
        [SerializeField] private float outputMultiplier = 1f;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (joystick == null) joystick = GetComponent<JoystickInteractable>();
            if (joystick == null) return;

            _disposable = new CompositeDisposable();

            joystick.OnRotationChanged
                .Subscribe(OnRotationChanged)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnRotationChanged(Vector2 rotation)
        {
            // Apply deadzone
            if (rotation.magnitude < deadzone)
                rotation = Vector2.zero;

            // Apply inversion
            float x = invertX ? -rotation.x : rotation.x;
            float y = invertY ? -rotation.y : rotation.y;

            // Apply multiplier
            x *= outputMultiplier;
            y *= outputMultiplier;

            if (vector2Output != null)
                vector2Output.Value = new Vector2(x, y);

            if (xOutput != null)
                xOutput.Value = x;

            if (yOutput != null)
                yOutput.Value = y;
        }
    }
}
