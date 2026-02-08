using UnityEngine;
using TMPro;
using UniRx;
using Shababeek.Utilities;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds an IntVariable to a TMP_Dropdown with two-way synchronization.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Dropdown Binder")]
    public class DropdownBinder : MonoBehaviour
    {
        [Header("Variable")]
        [SerializeField]
        [Tooltip("The IntVariable that stores the dropdown selection index.")]
        private IntVariable dropdownValue;

        [Header("UI Component")]
        [SerializeField]
        [Tooltip("The TMP_Dropdown component to bind.")]
        private TMP_Dropdown dropdown;

        private CompositeDisposable disposables;

        private void OnEnable()
        {
            if (dropdownValue == null || dropdown == null)
                return;

            disposables = new CompositeDisposable();

            // Dropdown → Variable
            dropdown.onValueChanged
                .AsObservable()
                .Subscribe(value => dropdownValue.Value = value)
                .AddTo(disposables);

            // Variable → Dropdown
            dropdownValue.OnValueChanged
                .Subscribe(value => dropdown.value = value)
                .AddTo(disposables);

            // Sync initial state
            dropdown.value = dropdownValue.Value;
        }

        private void OnDisable()
        {
            disposables?.Dispose();
        }
    }
}
