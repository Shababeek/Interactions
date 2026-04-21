using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Writes a DrawerInteractable's output to scriptable variables and events.</summary>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Drawer To Variable Driver")]
    public class DrawerToVariableDriver : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("Source drawer interactable.")]
        [SerializeField] private DrawerInteractable drawer;

        [Header("Output Variables")]
        [Tooltip("Float variable to receive the normalized drawer position (0-1).")]
        [SerializeField] private FloatVariable positionOutput;
        [Tooltip("Bool variable set to true when drawer is open, false when closed.")]
        [SerializeField] private BoolVariable isOpenOutput;

        [Header("Output Events")]
        [Tooltip("GameEvent raised when the drawer is fully opened.")]
        [SerializeField] private GameEvent onOpenedEvent;
        [Tooltip("GameEvent raised when the drawer is fully closed.")]
        [SerializeField] private GameEvent onClosedEvent;

        [Header("Settings")]
        [Tooltip("Invert the output values.")]
        [SerializeField] private bool invertOutput = false;
        [Tooltip("Position threshold above which the drawer is considered open (0-1).")]
        [SerializeField] private float openThreshold = 0.9f;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (drawer == null) drawer = GetComponent<DrawerInteractable>();
            if (drawer == null) return;

            _disposable = new CompositeDisposable();

            drawer.OnMoved
                .Subscribe(OnMoved)
                .AddTo(_disposable);

            if (onOpenedEvent != null)
                drawer.OnOpened
                    .Subscribe(_ => onOpenedEvent.Raise())
                    .AddTo(_disposable);

            if (onClosedEvent != null)
                drawer.OnClosed
                    .Subscribe(_ => onClosedEvent.Raise())
                    .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnMoved(float normalizedPosition)
        {
            float value = invertOutput ? (1f - normalizedPosition) : normalizedPosition;

            if (positionOutput != null) positionOutput.Value = value;
            if (isOpenOutput != null) isOpenOutput.Value = value >= openThreshold;
        }
    }
}
