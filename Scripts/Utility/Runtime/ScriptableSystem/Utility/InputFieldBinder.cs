using UnityEngine;
using TMPro;
using UniRx;
using Shababeek.Utilities;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a TextVariable to a TMP_InputField with two-way synchronization.
    /// </summary>
    [AddComponentMenu("Shababeek/ScriptableSystem/Binders/Input Field Binder")]
    public class InputFieldBinder : MonoBehaviour
    {
        [Header("Variable")]
        [SerializeField]
        [Tooltip("The TextVariable that stores the input field text.")]
        private TextVariable textVariable;

        [Header("UI Component")]
        [SerializeField]
        [Tooltip("The TMP_InputField component to bind.")]
        private TMP_InputField inputField;

        private CompositeDisposable disposables;
        private bool isUpdating;

        private void OnEnable()
        {
            if (textVariable is null || inputField == null)
                return;

            disposables = new CompositeDisposable();
            isUpdating = false;

            // InputField → Variable
            inputField.onValueChanged
                .AsObservable()
                .Subscribe(value =>
                {
                    if (!isUpdating)
                    {
                        isUpdating = true;
                        textVariable.Value = value;
                        isUpdating = false;
                    }
                })
                .AddTo(disposables);

            // Variable → InputField
            textVariable.OnValueChanged
                .Subscribe(value =>
                {
                    if (!isUpdating)
                    {
                        isUpdating = true;
                        inputField.text = value;
                        isUpdating = false;
                    }
                })
                .AddTo(disposables);

            // Sync initial state
            isUpdating = true;
            inputField.text = textVariable.Value;
            isUpdating = false;
        }

        private void OnDisable()
        {
            disposables?.Dispose();
        }
    }
}
