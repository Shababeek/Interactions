using TMPro;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// Highlights the label of the step a dial or slider currently rests on.
    /// Assign one label per step, in step order (label 0 = step 0). The inspector's
    /// "Sync Labels To Steps" tool builds and lays out the labels from the step settings.
    /// In edit mode the labels preview the source's current (starting) step.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Feedback/Step Label Highlighter")]
    [DefaultExecutionOrder(100)] // After the interactable's Start sets its starting step.
    [ExecuteAlways]
    public class StepLabelHighlighter : MonoBehaviour
    {
        /// <summary>What the sync tool does with label positions.</summary>
        public enum LabelPositionMode
        {
            /// <summary>Leave existing labels where they are; new labels are placed on their step.</summary>
            KeepCurrent,
            /// <summary>Place every label on its step position.</summary>
            MoveToSteps
        }

        [Tooltip("Dial or slider to follow. Must implement IStepInteractable. " +
                 "If empty, searches this object and its parents.")]
        [SerializeField] private InteractableBase source;

        [Tooltip("One label per step, in step order.")]
        [SerializeField] private TMP_Text[] labels = new TMP_Text[0];

        [Tooltip("Highlight follows the step live while dragging. " +
                 "Off: only updates once the step is confirmed (after the snap).")]
        [SerializeField] private bool updateWhileMoving = true;

        [Header("Active Step")]
        [SerializeField] private Color activeColor = new(1f, 0.8f, 0.2f);

        [Tooltip("Optional font material for the active label (e.g. a glow/outline preset).")]
        [SerializeField] private Material activeMaterial;

        [Tooltip("Scale multiplier applied to the active label.")]
        [SerializeField, Min(0.01f)] private float activeScale = 1.2f;

        [Header("Inactive Steps")]
        [SerializeField] private Color inactiveColor = new(1f, 1f, 1f, 0.5f);

        // Sync tool settings — drawn by StepLabelHighlighterEditor.
        [SerializeField] private LabelPositionMode positionMode = LabelPositionMode.KeepCurrent;
        [SerializeField] private bool writeNumbers = true;
        [SerializeField] private int firstLabelNumber = 1;
        [SerializeField, Min(0f)] private float labelRadius = 0.15f;
        [SerializeField] private Vector3 labelOffset;

        [Tooltip("Edit mode: highlight the source's current step on the labels.")]
        [SerializeField] private bool previewInEditMode = true;

        // Edit-mode preview state. Serialized so a scene saved mid-preview can undo the active
        // label's scale and material before the runtime caches its defaults.
        [SerializeField, HideInInspector] private int previewLabel = -1;
        [SerializeField, HideInInspector] private float previewAppliedScale = 1f;
        [SerializeField, HideInInspector] private Material previewOriginalMaterial;
        [SerializeField, HideInInspector] private Color previewOriginalColor = Color.white;
        [SerializeField, HideInInspector] private Color[] previewOriginalColors = new Color[0];

        private IStepInteractable _stepper;
        private Material[] _defaultMaterials;
        private Vector3[] _defaultScales;

        /// <summary>The dial or slider this component follows (resolved from parents if unassigned).</summary>
        public IStepInteractable Stepper =>
            (source != null ? source : GetComponentInParent<InteractableBase>()) as IStepInteractable;

        /// <summary>The interactable component behind <see cref="Stepper"/>.</summary>
        public InteractableBase Source => source != null ? source : GetComponentInParent<InteractableBase>();

        /// <summary>World position a label for <paramref name="step"/> should sit at, including radius and offset.</summary>
        public Vector3 GetLabelWorldPosition(int step)
        {
            var src = Source;
            return Stepper.GetStepWorldPosition(step, labelRadius) + src.transform.TransformVector(labelOffset);
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;

            ClearPreview();
            _stepper = Stepper;

            if (_stepper == null)
            {
                Debug.LogError($"{nameof(StepLabelHighlighter)} on {name}: source must be a Dial or Slider (IStepInteractable).", this);
                enabled = false;
                return;
            }

            _defaultMaterials = new Material[labels.Length];
            _defaultScales = new Vector3[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null) continue;
                _defaultMaterials[i] = labels[i].fontSharedMaterial;
                _defaultScales[i] = labels[i].transform.localScale;
            }

            if (labels.Length != _stepper.NumberOfSteps)
                Debug.LogWarning($"{nameof(StepLabelHighlighter)} on {name}: {labels.Length} labels for {_stepper.NumberOfSteps} steps.", this);
        }

        private void Start()
        {
            if (!Application.isPlaying || _stepper == null) return;

            var stream = updateWhileMoving ? _stepper.OnStepChanged : _stepper.OnStepConfirmed;
            stream.Subscribe(Highlight).AddTo(this);
            Highlight(_stepper.CurrentStep);
        }

        /// <summary>Highlights the label for <paramref name="step"/> and resets the others.</summary>
        public void Highlight(int step)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                if (label == null) continue;

                bool active = i == step;
                label.color = active ? activeColor : inactiveColor;
                label.transform.localScale = active ? _defaultScales[i] * activeScale : _defaultScales[i];

                var material = active && activeMaterial != null ? activeMaterial : _defaultMaterials[i];
                if (label.fontSharedMaterial != material) label.fontSharedMaterial = material;
            }
        }

        /// <summary>
        /// Edit mode: shows <paramref name="step"/> as the active label, restoring whatever the
        /// previous preview changed first. Pass -1 to only restore.
        /// </summary>
        public void PreviewStep(int step)
        {
            if (Application.isPlaying) return;

            ClearPreview();
            if (step < 0) return;

            previewOriginalColors = new Color[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                if (label == null) continue;

                previewOriginalColors[i] = label.color;
                bool active = i == step;
                label.color = active ? activeColor : inactiveColor;
                MarkDirty(label);

                if (!active) continue;
                previewLabel = i;
                previewAppliedScale = activeScale;
                previewOriginalMaterial = label.fontSharedMaterial;
                label.transform.localScale *= activeScale;
                if (activeMaterial != null) label.fontSharedMaterial = activeMaterial;
                MarkDirty(label.transform);
            }
            MarkDirty(this);
        }

        /// <summary>Restores the labels' authored color, scale and material after an edit-mode preview.</summary>
        public void ClearPreview()
        {
            if (previewOriginalColors.Length == 0 && previewLabel < 0) return;

            for (int i = 0; i < labels.Length && i < previewOriginalColors.Length; i++)
            {
                if (labels[i] == null) continue;
                labels[i].color = previewOriginalColors[i];
                MarkDirty(labels[i]);
            }

            if (previewLabel >= 0 && previewLabel < labels.Length && labels[previewLabel] != null)
            {
                var label = labels[previewLabel];
                if (previewAppliedScale > 0f) label.transform.localScale /= previewAppliedScale;
                if (previewOriginalMaterial != null) label.fontSharedMaterial = previewOriginalMaterial;
                MarkDirty(label);
                MarkDirty(label.transform);
            }

            previewLabel = -1;
            previewAppliedScale = 1f;
            previewOriginalMaterial = null;
            previewOriginalColors = new Color[0];
            MarkDirty(this);
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

        private void OnDisable()
        {
            if (Application.isPlaying) return;
            ClearPreview();
            _lastPreviewedStep = int.MinValue;
        }

        /// <summary>Edit mode: re-applies the preview for the source's current step.</summary>
        public void RefreshPreview(bool force)
        {
            if (Application.isPlaying) return;

            var stepper = Stepper;
            int step = previewInEditMode && stepper != null ? stepper.CurrentStep : -1;
            if (!force && step == _lastPreviewedStep) return;

            _lastPreviewedStep = step;
            PreviewStep(step);
        }

        private static void MarkDirty(Object target)
        {
            UnityEditor.EditorUtility.SetDirty(target);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
#else
        private static void MarkDirty(Object target) { }
#endif
    }
}
