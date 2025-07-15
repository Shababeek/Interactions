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
        [SerializeField] private ScriptableVariable variable;
        [SerializeField] private VariableReference<int> v;
        [SerializeField] private TextMeshProUGUI text;
        private CompositeDisposable _disposable;


        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            text.text = variable.ToString();
            variable.Do(_ => UpdateText()).Subscribe().AddTo(this);
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
