using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds numeric and bool variables to a CanvasGroup's properties.
    /// Control alpha, interactability, and blocking raycasts through variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Binders/Canvas Group Binder")]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupBinder : MonoBehaviour
    {
        [Header("Alpha Binding")]
        [Tooltip("Numeric variable to control alpha (0-1).")]
        [SerializeField] private ScriptableVariable alphaVariable;

        [Tooltip("Map variable range to alpha 0-1.")]
        [SerializeField] private bool useAlphaMapping = false;

        [Tooltip("Variable value for alpha = 0.")]
        [SerializeField] private float minAlphaValue = 0f;

        [Tooltip("Variable value for alpha = 1.")]
        [SerializeField] private float maxAlphaValue = 1f;

        [Header("Alpha Animation")]
        [Tooltip("Smoothly animate alpha changes.")]
        [SerializeField] private bool smoothAlpha = false;

        [Tooltip("Alpha animation speed.")]
        [SerializeField] private float alphaSpeed = 5f;

        [Header("Interactable Binding")]
        [Tooltip("Bool variable to control interactability.")]
        [SerializeField] private BoolVariable interactableVariable;

        [Tooltip("Invert the interactable logic.")]
        [SerializeField] private bool invertInteractable = false;

        [Header("Blocks Raycasts Binding")]
        [Tooltip("Bool variable to control blocking raycasts.")]
        [SerializeField] private BoolVariable blocksRaycastsVariable;

        [Tooltip("Invert the blocks raycasts logic.")]
        [SerializeField] private bool invertBlocksRaycasts = false;

        [Header("Ignore Parent Groups Binding")]
        [Tooltip("Bool variable to control ignoring parent groups.")]
        [SerializeField] private BoolVariable ignoreParentGroupsVariable;

        private CanvasGroup _canvasGroup;
        private CompositeDisposable _disposable;
        private float _targetAlpha;
        private INumericalVariable _alphaNumerical;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            _targetAlpha = _canvasGroup.alpha;

            // Alpha binding
            if (alphaVariable != null)
            {
                _alphaNumerical = alphaVariable as INumericalVariable;
                if (_alphaNumerical != null)
                {
                    UpdateAlpha(_alphaNumerical.AsFloat);

                    alphaVariable.OnRaised
                        .Subscribe(_ => UpdateAlpha(_alphaNumerical.AsFloat))
                        .AddTo(_disposable);
                }
                else
                {
                    Debug.LogWarning($"Alpha variable on {gameObject.name} is not a numerical variable", this);
                }
            }

            // Interactable binding
            if (interactableVariable != null)
            {
                UpdateInteractable(interactableVariable.Value);

                interactableVariable.OnRaised
                    .Subscribe(_ => UpdateInteractable(interactableVariable.Value))
                    .AddTo(_disposable);
            }

            // Blocks Raycasts binding
            if (blocksRaycastsVariable != null)
            {
                UpdateBlocksRaycasts(blocksRaycastsVariable.Value);

                blocksRaycastsVariable.OnRaised
                    .Subscribe(_ => UpdateBlocksRaycasts(blocksRaycastsVariable.Value))
                    .AddTo(_disposable);
            }

            // Ignore Parent Groups binding
            if (ignoreParentGroupsVariable != null)
            {
                UpdateIgnoreParentGroups(ignoreParentGroupsVariable.Value);

                ignoreParentGroupsVariable.OnRaised
                    .Subscribe(_ => UpdateIgnoreParentGroups(ignoreParentGroupsVariable.Value))
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (!smoothAlpha) return;

            if (!Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, alphaSpeed * Time.deltaTime);
            }
        }

        private void UpdateAlpha(float value)
        {
            float alpha;

            if (useAlphaMapping)
            {
                float t = Mathf.InverseLerp(minAlphaValue, maxAlphaValue, value);
                alpha = Mathf.Clamp01(t);
            }
            else
            {
                alpha = Mathf.Clamp01(value);
            }

            _targetAlpha = alpha;

            if (!smoothAlpha)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        private void UpdateInteractable(bool value)
        {
            _canvasGroup.interactable = invertInteractable ? !value : value;
        }

        private void UpdateBlocksRaycasts(bool value)
        {
            _canvasGroup.blocksRaycasts = invertBlocksRaycasts ? !value : value;
        }

        private void UpdateIgnoreParentGroups(bool value)
        {
            _canvasGroup.ignoreParentGroups = value;
        }

        /// <summary>
        /// Sets alpha immediately without animation.
        /// </summary>
        public void SetAlphaImmediate(float alpha)
        {
            _targetAlpha = alpha;
            _canvasGroup.alpha = alpha;
        }

        /// <summary>
        /// Fades to the specified alpha over time.
        /// </summary>
        public void FadeTo(float alpha)
        {
            _targetAlpha = Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// Fades to full visibility (alpha = 1).
        /// </summary>
        public void FadeIn()
        {
            _targetAlpha = 1f;
        }

        /// <summary>
        /// Fades to invisible (alpha = 0).
        /// </summary>
        public void FadeOut()
        {
            _targetAlpha = 0f;
        }

        /// <summary>
        /// Shows the canvas group (alpha = 1, interactable, blocks raycasts).
        /// </summary>
        [ContextMenu("Show")]
        public void Show()
        {
            _targetAlpha = 1f;
            if (!smoothAlpha) _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Hides the canvas group (alpha = 0, not interactable, doesn't block raycasts).
        /// </summary>
        [ContextMenu("Hide")]
        public void Hide()
        {
            _targetAlpha = 0f;
            if (!smoothAlpha) _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
