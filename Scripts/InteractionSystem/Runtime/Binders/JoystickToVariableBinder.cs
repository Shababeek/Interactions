using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Binds a JoystickInteractable's output to scriptable variables.</summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Joystick To Variable Binder")]
    public class JoystickToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("The joystick interactable to bind from.")]
        [SerializeField] private JoystickInteractable joystick;

        [Header("Output Variables")]
        [Tooltip("Vector2 variable to receive the joystick rotation.")]
        [SerializeField] private Vector2Variable vector2Output;
        [Tooltip("Float variable to receive the X axis value.")]
        [SerializeField] private FloatVariable xOutput;
        [Tooltip("Float variable to receive the Y axis value.")]
        [SerializeField] private FloatVariable yOutput;

        [Header("Settings")]
        [Tooltip("Invert the X axis output.")]
        [SerializeField] private bool invertX = false;
        [Tooltip("Invert the Y axis output.")]
        [SerializeField] private bool invertY = false;
        [Tooltip("Deadzone threshold before output is registered.")]
        [SerializeField] private float deadzone = 0.1f;
        [Tooltip("Multiplier applied to the output values.")]
        [SerializeField] private float outputMultiplier = 1f;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (joystick == null) joystick = GetComponent<JoystickInteractable>();
            if (joystick == null) return;

            _disposable = new CompositeDisposable();
            OnRotationChanged(joystick.NormalizedRotation);
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
