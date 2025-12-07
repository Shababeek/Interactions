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

        private void UpdateText()
        {
            if(_textUI)_textUI.text = variable.ToString();
            if(_text3D) _text3D.text = variable.ToString();
        }

        private void OnDisable()
        {
            _disposable.Dispose();
        }
    }
}
