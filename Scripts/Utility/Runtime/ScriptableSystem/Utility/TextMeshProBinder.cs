using System;
using UniRx;
using UnityEngine;
using TMPro;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds a ScriptableVariable to a UI element for live updates.
    /// </summary>
    [AddComponentMenu(menuName: "Shababeek/Scriptable System/TMPro Variable Binder")]
    public class TextMeshProBinder : MonoBehaviour
    {
        [Tooltip("Format string (e.g., 'X' for hex, 'F2' for 2 decimals, 'N0' for thousands separator)")]
        [SerializeField] private string format = "";
        [Tooltip("The ScriptableVariable to bind to the UI.")]
        [SerializeField] private ScriptableVariable variable;
        [Tooltip("The TextMeshProUGUI component to update with the variable's value.")]
        private TextMeshProUGUI _textUI;
        private TMP_Text _text3D;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _textUI = GetComponent<TextMeshProUGUI>();
            _text3D = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            if(_textUI) _textUI.text = variable.ToString();
            if(_text3D) _text3D.text = variable.ToString();
            variable.OnRaised.Do(_ => UpdateText()).Subscribe().AddTo(this);
        }
        private string FormatValue(object value)
        {
            if (value == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(format) && value is IFormattable formattable)
                return formattable.ToString(format, null);

            return value.ToString();
        }
        private void UpdateText()
        {
            string text = FormatValue(variable.GetValue());

            if (_textUI) _textUI.text = text;
            if (_text3D) _text3D.text = text;
        }

        private void OnDisable()
        {
            _disposable.Dispose();
        }
    }
}
