using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Binds a DrawerInteractable's output to scriptable variables and events.</summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Drawer To Variable Binder")]
    public class DrawerToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("The drawer interactable to bind from.")]
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

            drawer.OnOpened
                .Subscribe(_ => OnOpened())
                .AddTo(_disposable);

            drawer.OnClosed
                .Subscribe(_ => OnClosed())
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnMoved(float normalizedPosition)
        {
            float value = invertOutput ? (1f - normalizedPosition) : normalizedPosition;

            if (positionOutput != null)
                positionOutput.Value = value;

            if (isOpenOutput != null)
                isOpenOutput.Value = value >= openThreshold;
        }

        private void OnOpened()
        {
            onOpenedEvent?.Raise();
        }

        private void OnClosed()
        {
            onClosedEvent?.Raise();
        }
    }
}
