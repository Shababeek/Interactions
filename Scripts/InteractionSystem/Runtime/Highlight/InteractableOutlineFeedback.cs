using UnityEngine;
using UniRx;
using Shababeek.Interactions;

/// <summary>
/// Drives a QuickOutline component from an InteractableBase's hover/select events.
/// Enables the outline on hover, swaps color/width when selected, disables on release.
/// Optionally shows a pulsing idle "grab hint" outline to advertise the object as grabbable.
/// </summary>
[RequireComponent(typeof(InteractableBase))]
[DisallowMultipleComponent]
[AddComponentMenu("Shababeek/Interactions/Interactable Outline Feedback")]
public class InteractableOutlineFeedback : MonoBehaviour
{
    [Tooltip("Outline to drive. Auto-found on this GameObject (or children) if not assigned. Created on this GameObject when missing.")]
    [SerializeField] private Outline outline;

    [Header("Hover")]
    [SerializeField] private Color hoverColor = new Color(1f, 0.85f, 0.2f, 0.2f);
    [SerializeField, Range(0f, 5f)] private float hoverWidth = 0.1f;
    [SerializeField] private Outline.Mode hoverMode = Outline.Mode.OutlineVisible;

    [Header("Selected")]
    [Tooltip("Keep outline visible while held.")]
    [SerializeField] private bool showWhileSelected = true;
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.9f, 1f, 0.2f);
    [SerializeField, Range(0f, 5f)] private float selectedWidth = 0.1f;
    [SerializeField] private Outline.Mode selectedMode = Outline.Mode.OutlineVisible;

    [Header("Grab Hint (Idle)")]
    [Tooltip("Show a pulsing outline when idle (not hovered/selected) to hint the object is grabbable.")]
    [SerializeField] private bool showGrabHint = false;
    [SerializeField] private Color hintColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Outline.Mode hintMode = Outline.Mode.OutlineVisible;
    [Tooltip("Minimum outline width during the pulse cycle.")]
    [SerializeField, Range(0f, 5f)] private float hintMinWidth = 0.02f;
    [Tooltip("Maximum outline width during the pulse cycle.")]
    [SerializeField, Range(0f, 5f)] private float hintMaxWidth = 0.08f;
    [Tooltip("Pulses per second.")]
    [SerializeField, Range(0.1f, 5f)] private float hintPulseSpeed = 1f;

    private static readonly Color HiddenColor = new Color(0f, 0f, 0f, 0f);

    private InteractableBase _interactable;
    private readonly CompositeDisposable _disposables = new();
    private bool _isHovering;
    private bool _isSelected;

    private void Awake()
    {
        _interactable = GetComponent<InteractableBase>();
        EnsureOutline();
        ApplyState();
    }

    private void OnEnable()
    {
        _disposables.Clear();
        if (_interactable == null) return;

        _interactable.OnHoverStarted
            .Subscribe(_ => { _isHovering = true;  ApplyState(); })
            .AddTo(_disposables);

        _interactable.OnHoverEnded
            .Subscribe(_ => { _isHovering = false; ApplyState(); })
            .AddTo(_disposables);

        _interactable.OnSelected
            .Subscribe(_ => { _isSelected = true;  ApplyState(); })
            .AddTo(_disposables);

        _interactable.OnDeselected
            .Subscribe(_ => { _isSelected = false; _isHovering = false; ApplyState(); })
            .AddTo(_disposables);

        ApplyState();
    }

    private void OnDisable()
    {
        _disposables.Clear();
        _isHovering = false;
        _isSelected = false;
        HideOutline();
    }

    private void Update()
    {
        if (!showGrabHint) return;
        if (_isHovering || _isSelected) return;
        if (outline == null) return;

        float t = (Mathf.Sin(Time.time * hintPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        Set(hintMode, hintColor, Mathf.Lerp(hintMinWidth, hintMaxWidth, t));
    }

    private void EnsureOutline()
    {
        if (outline == null) outline = GetComponent<Outline>();
        if (outline == null) outline = GetComponentInChildren<Outline>(true);
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        // Keep the component enabled so materials stay attached to renderers.
        // We hide the outline by zeroing width AND alpha instead of toggling enabled,
        // which would strip/re-add materials and can cause a one-frame pink flash.
        outline.enabled = true;
        HideOutline();
    }

    private void ApplyState()
    {
        if (outline == null) return;

        if (_isSelected && showWhileSelected)
        {
            Set(selectedMode, selectedColor, selectedWidth);
            return;
        }

        if (_isHovering && !_isSelected)
        {
            Set(hoverMode, hoverColor, hoverWidth);
            return;
        }

        if (showGrabHint)
        {
            Set(hintMode, hintColor, hintMinWidth);
            return;
        }

        HideOutline();
    }

    private void HideOutline()
    {
        Set(Outline.Mode.OutlineVisible, HiddenColor, 0f);
    }

    private void Set(Outline.Mode mode, Color color, float width)
    {
        if (outline == null) return;
        outline.OutlineMode = mode;
        outline.OutlineColor = color;
        outline.OutlineWidth = width;
    }

    /// <summary>Toggle the idle grab-hint outline at runtime.</summary>
    public void SetGrabHint(bool enabled)
    {
        showGrabHint = enabled;
        ApplyState();
    }

    private void OnValidate()
    {
        if (outline == null) outline = GetComponent<Outline>();
        if (hintMaxWidth < hintMinWidth) hintMaxWidth = hintMinWidth;
    }
}
