using TMPro;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// Shows the step a dial or slider currently rests on in a single text, e.g. a readout
    /// window above a selector knob. Use <see cref="StepLabelHighlighter"/> instead when every
    /// step has its own label. In edit mode the text previews the source's current (starting) step.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Feedback/Step Value Display")]
    [DefaultExecutionOrder(100)] // After the interactable's Start sets its starting step.
    [ExecuteAlways]
    public class StepValueDisplay : MonoBehaviour
    {
        [Tooltip("Dial or slider to follow. Must implement IStepInteractable. " +
                 "If empty, searches this object and its parents.")]
        [SerializeField] private InteractableBase source;

        [Tooltip("Text that shows the current step.")]
        [SerializeField] private TMP_Text text;

        [Tooltip("Text follows the step live while dragging. " +
                 "Off: only updates once the step is confirmed (after the snap).")]
        [SerializeField] private bool updateWhileMoving = true;

        [Tooltip("Number shown for the first step: 1 → 1..N, 0 → 0..N-1.")]
        [SerializeField] private int firstNumber = 1;

        [Tooltip("Format for the number. {0} is replaced by it, e.g. \"CH {0}\" or \"{0:00}\".")]
        [SerializeField] private string format = "{0}";

        [Tooltip("Optional text per step (Low / Mid / High). Overrides the number for steps that have an entry.")]
        [SerializeField] private string[] stepTexts = new string[0];

        [Tooltip("Edit mode: show the source's current step on the text.")]
        [SerializeField] private bool previewInEditMode = true;

        private IStepInteractable _stepper;

        /// <summary>The dial or slider this component follows (resolved from parents if unassigned).</summary>
        public IStepInteractable Stepper =>
            (source != null ? source : GetComponentInParent<InteractableBase>()) as IStepInteractable;

        private void Reset()
        {
            text = GetComponentInChildren<TMP_Text>();
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;

            _stepper = Stepper;
            if (_stepper == null)
            {
                Debug.LogError($"{nameof(StepValueDisplay)} on {name}: source must be a Dial or Slider (IStepInteractable).", this);
                enabled = false;
                return;
            }

            if (text == null)
            {
                Debug.LogError($"{nameof(StepValueDisplay)} on {name}: no text assigned.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (!Application.isPlaying || _stepper == null || text == null) return;

            var stream = updateWhileMoving ? _stepper.OnStepChanged : _stepper.OnStepConfirmed;
            stream.Subscribe(Show).AddTo(this);
            Show(_stepper.CurrentStep);
        }

        /// <summary>Writes <paramref name="step"/> into the text.</summary>
        public void Show(int step)
        {
            if (text == null) return;
            text.text = TextForStep(step);
        }

        /// <summary>What the display shows for <paramref name="step"/>: its custom text if it has one, else the formatted number.</summary>
        public string TextForStep(int step)
        {
            if (step >= 0 && step < stepTexts.Length && !string.IsNullOrEmpty(stepTexts[step]))
                return stepTexts[step];

            int number = firstNumber + step;
            try { return string.Format(format, number); }
            catch (System.FormatException) { return number.ToString(); }
        }

#if UNITY_EDITOR
        private int _lastPreviewedStep = int.MinValue;

        // Edit-mode Update runs whenever the scene changes, so this follows the source's step
        // as it is edited elsewhere (starting step, dial preview, a component driving it).
        private void Update()
        {
            if (Application.isPlaying) return;
            RefreshPreview(false);
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            // Touching other objects inside OnValidate is unsafe; defer to the next editor tick.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) RefreshPreview(true);
            };
        }

        /// <summary>Edit mode: re-applies the preview for the source's current step.</summary>
        public void RefreshPreview(bool force)
        {
            if (Application.isPlaying || !previewInEditMode || text == null) return;

            var stepper = Stepper;
            if (stepper == null) return;

            int step = stepper.CurrentStep;
            if (!force && step == _lastPreviewedStep) return;
            _lastPreviewedStep = step;

            var shown = TextForStep(step);
            if (text.text == shown) return;

            text.text = shown;
            UnityEditor.EditorUtility.SetDirty(text);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(text);
        }
#endif
    }
}
