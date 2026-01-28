using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds GameEvents to Animator triggers, bools, floats, or integers.
    /// </summary>
    /// <remarks>
    /// Allows decoupled animation control through the scriptable event system.
    ///
    /// Common use cases include:
    /// - Triggering animations from game events
    /// - Controlling animation states from variables
    /// - Playing animations when interactables are used
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Event Animator Binder")]
    public class EventAnimatorBinder : MonoBehaviour
    {
        [Tooltip("The Animator component. Uses this object's Animator if not set.")]
        [SerializeField] private Animator animator;

        [Header("Trigger Events")]
        [Tooltip("Events that trigger animator parameters.")]
        [SerializeField] private TriggerBinding[] triggerBindings;

        [Header("Bool Events")]
        [Tooltip("Events that set animator bool parameters.")]
        [SerializeField] private BoolBinding[] boolBindings;

        [Header("Numerical Bindings")]
        [Tooltip("Numerical variables bound to animator float parameters.")]
        [SerializeField] private FloatBinding[] floatBindings;

        [Tooltip("Numerical variables bound to animator integer parameters.")]
        [SerializeField] private IntBinding[] intBindings;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogWarning($"No Animator found on {gameObject.name}", this);
                return;
            }

            // Subscribe to trigger events
            if (triggerBindings != null)
            {
                foreach (var binding in triggerBindings)
                {
                    if (binding.gameEvent == null) continue;

                    var paramId = Animator.StringToHash(binding.parameterName);
                    binding.gameEvent.OnRaised
                        .Subscribe(_ => animator.SetTrigger(paramId))
                        .AddTo(_disposable);
                }
            }

            // Subscribe to bool events
            if (boolBindings != null)
            {
                foreach (var binding in boolBindings)
                {
                    if (binding.setTrueEvent == null && binding.setFalseEvent == null && binding.toggleEvent == null)
                        continue;

                    var paramId = Animator.StringToHash(binding.parameterName);

                    if (binding.setTrueEvent != null)
                    {
                        binding.setTrueEvent.OnRaised
                            .Subscribe(_ => animator.SetBool(paramId, true))
                            .AddTo(_disposable);
                    }

                    if (binding.setFalseEvent != null)
                    {
                        binding.setFalseEvent.OnRaised
                            .Subscribe(_ => animator.SetBool(paramId, false))
                            .AddTo(_disposable);
                    }

                    if (binding.toggleEvent != null)
                    {
                        binding.toggleEvent.OnRaised
                            .Subscribe(_ => animator.SetBool(paramId, !animator.GetBool(paramId)))
                            .AddTo(_disposable);
                    }
                }
            }

            // Subscribe to float variables
            if (floatBindings != null)
            {
                foreach (var binding in floatBindings)
                {
                    if (binding.variable == null) continue;

                    var numVar = binding.variable as INumericalVariable;
                    if (numVar == null) continue;

                    var paramId = Animator.StringToHash(binding.parameterName);

                    // Set initial value
                    animator.SetFloat(paramId, numVar.AsFloat * binding.multiplier);

                    // Subscribe to changes
                    binding.variable.OnRaised
                        .Subscribe(_ => animator.SetFloat(paramId, numVar.AsFloat * binding.multiplier))
                        .AddTo(_disposable);
                }
            }

            // Subscribe to int variables
            if (intBindings != null)
            {
                foreach (var binding in intBindings)
                {
                    if (binding.variable == null) continue;

                    var numVar = binding.variable as INumericalVariable;
                    if (numVar == null) continue;

                    var paramId = Animator.StringToHash(binding.parameterName);

                    // Set initial value
                    animator.SetInteger(paramId, numVar.AsInt);

                    // Subscribe to changes
                    binding.variable.OnRaised
                        .Subscribe(_ => animator.SetInteger(paramId, numVar.AsInt))
                        .AddTo(_disposable);
                }
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        /// <summary>
        /// Manually triggers an animator parameter by name.
        /// </summary>
        public void TriggerParameter(string paramName)
        {
            if (animator != null)
                animator.SetTrigger(paramName);
        }

        /// <summary>
        /// Manually sets an animator bool parameter.
        /// </summary>
        public void SetBoolParameter(string paramName, bool value)
        {
            if (animator != null)
                animator.SetBool(paramName, value);
        }

        [System.Serializable]
        public class TriggerBinding
        {
            [Tooltip("Event that triggers the animation.")]
            public GameEvent gameEvent;

            [Tooltip("Name of the animator trigger parameter.")]
            public string parameterName;
        }

        [System.Serializable]
        public class BoolBinding
        {
            [Tooltip("Name of the animator bool parameter.")]
            public string parameterName;

            [Tooltip("Event that sets the bool to true.")]
            public GameEvent setTrueEvent;

            [Tooltip("Event that sets the bool to false.")]
            public GameEvent setFalseEvent;

            [Tooltip("Event that toggles the bool.")]
            public GameEvent toggleEvent;
        }

        [System.Serializable]
        public class FloatBinding
        {
            [Tooltip("Numerical variable to bind.")]
            public ScriptableVariable variable;

            [Tooltip("Name of the animator float parameter.")]
            public string parameterName;

            [Tooltip("Multiplier applied to the value.")]
            public float multiplier = 1f;
        }

        [System.Serializable]
        public class IntBinding
        {
            [Tooltip("Numerical variable to bind.")]
            public ScriptableVariable variable;

            [Tooltip("Name of the animator integer parameter.")]
            public string parameterName;
        }
    }
}
