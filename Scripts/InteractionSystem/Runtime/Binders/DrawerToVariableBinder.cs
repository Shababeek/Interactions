using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a DrawerInteractable's output to scriptable variables and events.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Drawer To Variable Binder")]
    public class DrawerToVariableBinder : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private DrawerInteractable drawer;

        [Header("Output Variables")]
        [SerializeField] private FloatVariable positionOutput;
        [SerializeField] private BoolVariable isOpenOutput;

        [Header("Output Events")]
        [SerializeField] private GameEvent onOpenedEvent;
        [SerializeField] private GameEvent onClosedEvent;

        [Header("Settings")]
        [SerializeField] private bool invertOutput = false;
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
