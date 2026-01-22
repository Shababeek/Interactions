using TMPro;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds a ColorVariable to TextMeshPro text color.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Color TextMeshPro Binder")]
    public class ColorTextMeshProBinder : MonoBehaviour
    {
        [SerializeField] private ColorVariable colorVariable;
        [SerializeField] private TMP_Text textComponent;

        [Header("Settings")]
        [SerializeField] private bool smooth;
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool includeAlpha = true;

        private CompositeDisposable _disposable;
        private Color _targetColor;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (textComponent == null) textComponent = GetComponent<TMP_Text>();
            if (colorVariable == null || textComponent == null) return;

            _targetColor = colorVariable.Value;
            ApplyColor(_targetColor);

            colorVariable.OnValueChanged.Subscribe(UpdateColor).AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void Update()
        {
            if (!smooth) return;

            var current = textComponent.color;
            var next = Color.Lerp(current, _targetColor, speed * Time.deltaTime);
            if (!includeAlpha) next.a = current.a;
            textComponent.color = next;
        }

        private void UpdateColor(Color color)
        {
            _targetColor = color;
            if (!smooth) ApplyColor(color);
        }

        private void ApplyColor(Color color)
        {
            if (!includeAlpha) color.a = textComponent.color.a;
            textComponent.color = color;
        }
    }
}
