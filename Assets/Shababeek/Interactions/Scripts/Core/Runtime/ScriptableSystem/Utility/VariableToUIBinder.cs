using System;
using UniRx;
using UnityEngine;
using TMPro;

namespace Shababeek.Core
{
    /// <summary>
    /// Binds a ScriptableVariable to a UI element for live updates.
    /// </summary>
    [AddComponentMenu(menuName: "Shababeek/Scriptable System/UI Variable Updated")]
    public class VariableToUIBinder : MonoBehaviour
    {
        [Tooltip("The ScriptableVariable to bind to the UI.")]
        [SerializeField] private ScriptableVariable variable;
        [Tooltip("The TextMeshProUGUI component to update with the variable's value.")]
        [SerializeField] private TextMeshProUGUI text;
        private CompositeDisposable _disposable;


        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            text.text = variable.ToString();
            variable.OnRaised.Do(_ => UpdateText()).Subscribe().AddTo(this);
        }

        private void UpdateText()
        {
            text.text = variable.ToString();
        }

        private void OnDisable()
        {
            _disposable.Dispose();
        }
    }
}
