using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds a BoolVariable to enable/disable GameObjects or components.
    /// </summary>
    /// <remarks>
    /// Useful for toggling visibility, enabling/disabling features, or triggering events
    /// based on a boolean state.
    ///
    /// Common use cases include:
    /// - Toggling UI panels
    /// - Enabling/disabling game features
    /// - Showing/hiding objects based on state
    /// - Activating effects when conditions are met
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Bool Toggle Binder")]
    public class BoolToggleBinder : MonoBehaviour
    {
        [Tooltip("The bool variable to bind.")]
        [SerializeField] private BoolVariable variable;

        [Header("Toggle Targets")]
        [Tooltip("GameObjects to enable when true, disable when false.")]
        [SerializeField] private GameObject[] objectsToToggle;

        [Tooltip("Behaviours (scripts) to enable when true, disable when false.")]
        [SerializeField] private Behaviour[] behavioursToToggle;

        [Tooltip("Colliders to enable when true, disable when false.")]
        [SerializeField] private Collider[] collidersToToggle;

        [Tooltip("Renderers to enable when true, disable when false.")]
        [SerializeField] private Renderer[] renderersToToggle;

        [Header("Options")]
        [Tooltip("Invert the boolean value (true becomes false, false becomes true).")]
        [SerializeField] private bool invert = false;

        [Tooltip("Set initial state on enable based on current variable value.")]
        [SerializeField] private bool setOnEnable = true;

        [Header("Events")]
        [Tooltip("Called when the value changes to true (after invert if applicable).")]
        [SerializeField] private UnityEvent onTrue;

        [Tooltip("Called when the value changes to false (after invert if applicable).")]
        [SerializeField] private UnityEvent onFalse;

        [Tooltip("Called whenever the value changes. Parameter is the new value (after invert).")]
        [SerializeField] private UnityEvent<bool> onValueChanged;

        private CompositeDisposable _disposable;
        private bool _lastValue;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (variable == null)
            {
                Debug.LogWarning($"BoolVariable is not assigned on {gameObject.name}", this);
                return;
            }

            if (setOnEnable)
            {
                ApplyValue(GetEffectiveValue());
            }

            variable.OnRaised
                .Subscribe(_ => OnValueRaised())
                .AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void OnValueRaised()
        {
            bool effectiveValue = GetEffectiveValue();

            if (effectiveValue != _lastValue)
            {
                ApplyValue(effectiveValue);
            }
        }

        private bool GetEffectiveValue()
        {
            bool value = variable.Value;
            return invert ? !value : value;
        }

        private void ApplyValue(bool value)
        {
            _lastValue = value;

            // Toggle GameObjects
            if (objectsToToggle != null)
            {
                foreach (var obj in objectsToToggle)
                {
                    if (obj != null)
                        obj.SetActive(value);
                }
            }

            // Toggle Behaviours
            if (behavioursToToggle != null)
            {
                foreach (var behaviour in behavioursToToggle)
                {
                    if (behaviour != null)
                        behaviour.enabled = value;
                }
            }

            // Toggle Colliders
            if (collidersToToggle != null)
            {
                foreach (var col in collidersToToggle)
                {
                    if (col != null)
                        col.enabled = value;
                }
            }

            // Toggle Renderers
            if (renderersToToggle != null)
            {
                foreach (var rend in renderersToToggle)
                {
                    if (rend != null)
                        rend.enabled = value;
                }
            }

            // Fire events
            onValueChanged?.Invoke(value);

            if (value)
                onTrue?.Invoke();
            else
                onFalse?.Invoke();
        }

        /// <summary>
        /// Manually sets the toggle state (ignoring the variable).
        /// </summary>
        public void SetState(bool state)
        {
            ApplyValue(state);
        }

        /// <summary>
        /// Toggles the current state.
        /// </summary>
        public void Toggle()
        {
            ApplyValue(!_lastValue);
        }

        /// <summary>
        /// Forces a refresh from the current variable value.
        /// </summary>
        public void Refresh()
        {
            if (variable != null)
                ApplyValue(GetEffectiveValue());
        }
    }
}
