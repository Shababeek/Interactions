using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Writes a Switch's on/off state to a scriptable variable and raises events.</summary>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Switch To Variable Driver")]
    [RequireComponent(typeof(Switch))]
    public class SwitchToVariableDriver : MonoBehaviour
    {
        [Header("Variable Output")]
        [Tooltip("BoolVariable that receives the switch state (true = on).")]
        [SerializeField] private BoolVariable stateVariable;

        [Header("Event Outputs")]
        [Tooltip("GameEvent raised when the switch turns on.")]
        [SerializeField] private GameEvent onTurnedOnEvent;

        [Tooltip("GameEvent raised when the switch turns off.")]
        [SerializeField] private GameEvent onTurnedOffEvent;

        private Switch _switch;
        private CompositeDisposable _disposable;

        private void Awake() => _switch = GetComponent<Switch>();

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            _switch.OnStateChanged
                .Subscribe(OnStateChanged)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnStateChanged(bool on)
        {
            if (stateVariable) stateVariable.Value = on;
            if (on) onTurnedOnEvent?.Raise();
            else onTurnedOffEvent?.Raise();
        }
    }
}
