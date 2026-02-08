using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Shababeek.Utilities;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a FloatVariable to a ScrollRect's normalized scroll position with two-way synchronization.
    /// </summary>
    [AddComponentMenu("Shababeek/ScriptableSystem/Binders/ScrollRect Binder")]
    public class ScrollRectBinder : MonoBehaviour
    {
        [Header("Variable")]
        [SerializeField]
        [Tooltip("The FloatVariable that stores the normalized scroll position (0-1).")]
        private FloatVariable scrollPosition;

        [Header("UI Component")]
        [SerializeField]
        [Tooltip("The ScrollRect component to bind.")]
        private ScrollRect scrollRect;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("If true, binds to horizontal scroll position. If false, binds to vertical.")]
        private bool useHorizontal = false;

        private CompositeDisposable disposables;

        private void OnEnable()
        {
            if (scrollPosition == null || scrollRect == null)
                return;

            disposables = new CompositeDisposable();

            // ScrollRect → Variable
            scrollRect.onValueChanged
                .AsObservable()
                .Subscribe(value =>
                {
                    float normalizedPosition = useHorizontal ? value.x : value.y;
                    scrollPosition.Value = normalizedPosition;
                })
                .AddTo(disposables);

            // Variable → ScrollRect
            scrollPosition.OnValueChanged
                .Subscribe(value =>
                {
                    Vector2 normalizedPosition = scrollRect.normalizedPosition;
                    if (useHorizontal)
                        normalizedPosition.x = value;
                    else
                        normalizedPosition.y = value;
                    scrollRect.normalizedPosition = normalizedPosition;
                })
                .AddTo(disposables);

            // Sync initial state
            float initialPosition = useHorizontal ? scrollRect.normalizedPosition.x : scrollRect.normalizedPosition.y;
            scrollPosition.Value = initialPosition;
        }

        private void OnDisable()
        {
            disposables?.Dispose();
        }
    }
}
