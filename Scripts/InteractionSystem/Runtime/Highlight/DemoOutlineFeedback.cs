using UnityEngine;
using UniRx;
using Shababeek.Interactions;

/// <summary>
/// Self-contained outline feedback for demos. Drives a QuickOutline component from an
/// InteractableBase's hover/select events: enables on hover, swaps color/width when selected,
/// disables on release, and can pulse an idle "grab hint" outline.
/// Unlike <see cref="InteractableOutlineFeedback"/>, every color/width/mode/toggle is a local
/// serialized field on THIS component — nothing is read from Resources/OutlineFeedbackConfig.
/// Drop it on an object, tweak in the Inspector, and it just works with no project-wide asset.
/// </summary>
[RequireComponent(typeof(InteractableBase))]
[DisallowMultipleComponent]
[AddComponentMenu("Shababeek/Interactions/Demo Outline Feedback (Config-Free)")]
public class DemoOutlineFeedback : MonoBehaviour
{
    /// <summary>Color/width/mode settings for a single outline state.</summary>
    [System.Serializable]
    public class OutlineState
    {
        [Tooltip("Master switch for this state. When off the outline stays hidden for this state.")]
        public bool enabled = true;
        public Color color = new Color(1f, 0.85f, 0.2f, 0.2f);
        [Range(0f, 5f)] public float width = 0.1f;
        public Outline.Mode mode = Outline.Mode.OutlineVisible;
    }

    [Tooltip("Outline to drive. Auto-found on this GameObject (or children) if not assigned. Created on this GameObject when missing.")]
    [SerializeField] private Outline outline;

    [Tooltip("Global kill switch for this component's outline feedback.")]
    [SerializeField] private bool feedbackEnabled = true;

    [Tooltip("When on, shows the outline PLUS the inside fresnel/stripe fill. When off, shows the outline only. " +
             "Requires the '_FRESNEL_ON' Boolean Keyword in the OutlineFill shader graph.")]
    [SerializeField] private bool insideFill = true;

    [Header("Hover")]
    [SerializeField] private OutlineState hover = new()
    {
        color = new Color(1f, 0.85f, 0.2f, 0.2f),
        width = 0.1f,
        mode = Outline.Mode.OutlineVisible
    };

    [Header("Selected")]
    [Tooltip("Keep outline visible while held.")]
    [SerializeField] private OutlineState selected = new()
    {
        color = new Color(0.2f, 0.9f, 1f, 0.2f),
        width = 0.1f,
        mode = Outline.Mode.OutlineVisible
    };

    [Header("Grab Hint (Idle)")]
    [Tooltip("Pulsing outline shown when idle (not hovered/selected) to hint the object is grabbable.")]
    [SerializeField] private OutlineState hint = new()
    {
        enabled = false,
        color = new Color(1f, 1f, 1f, 0.1f),
        width = 0.02f,
        mode = Outline.Mode.OutlineVisible
    };
    [Tooltip("Minimum outline width during the pulse cycle.")]
    [SerializeField, Range(0f, 5f)] private float hintMinWidth = 0.02f;
    [Tooltip("Maximum outline width during the pulse cycle.")]
    [SerializeField, Range(0f, 5f)] private float hintMaxWidth = 0.08f;
    [Tooltip("Pulses per second.")]
    [SerializeField, Range(0.1f, 5f)] private float hintPulseSpeed = 1f;

    private static readonly Color HiddenColor = new Color(0f, 0f, 0f, 0f);

    // Shader keyword the OutlineFill graph exposes to gate the inside fresnel/stripe fill.
    private const string FresnelKeyword = "_FRESNEL_ON";

    private InteractableBase _interactable;
    // The per-instance OutlineFill material, added to a renderer by the Outline component at runtime.
    private Material _fillMaterial;
    private readonly CompositeDisposable _disposables = new();
    private bool _isHovering;
    private bool _isSelected;
    // Per-instance runtime toggle for the idle grab hint. Initialized from the local hint state;
    // this decides whether THIS object currently advertises itself (e.g. driven by sockets).
    private bool _showGrabHint;

    private void Awake()
    {
        _interactable = GetComponent<InteractableBase>();
        _showGrabHint = hint.enabled;
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

    private void Start()
    {
        // Runs after every component's OnEnable, so the Outline component has already
        // instantiated its OutlineFill material and added it to a renderer by now.
        ApplyFillMode();
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
        if (!feedbackEnabled || !_showGrabHint) return;
        if (_isHovering || _isSelected) return;
        if (outline == null) return;

        float t = (Mathf.Sin(Time.time * hintPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        Set(hint.mode, hint.color, Mathf.Lerp(hintMinWidth, hintMaxWidth, t));
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

        if (!feedbackEnabled)
        {
            HideOutline();
            return;
        }

        if (_isSelected && selected.enabled)
        {
            Set(selected.mode, selected.color, selected.width);
            return;
        }

        if (_isHovering && !_isSelected && hover.enabled)
        {
            Set(hover.mode, hover.color, hover.width);
            return;
        }

        if (_showGrabHint)
        {
            Set(hint.mode, hint.color, hintMinWidth);
            return;
        }

        HideOutline();
    }

    /// <summary>Toggle this object's idle grab-hint outline at runtime (style comes from local fields).</summary>
    public void SetGrabHint(bool enabled)
    {
        _showGrabHint = enabled;
        ApplyState();
    }

    /// <summary>
    /// Toggle the inside fresnel/stripe fill at runtime. On = outline + fill, Off = outline only.
    /// </summary>
    public void SetInsideFill(bool enabled)
    {
        insideFill = enabled;
        ApplyFillMode();
    }

    // Enables/disables the fresnel fill by toggling the shader keyword on the OutlineFill material.
    private void ApplyFillMode()
    {
        if (!Application.isPlaying) return; // Fill material only exists in play mode.
        ResolveFillMaterial();
        if (_fillMaterial == null) return;

        if (insideFill) _fillMaterial.EnableKeyword(FresnelKeyword);
        else _fillMaterial.DisableKeyword(FresnelKeyword);
    }

    // Finds the OutlineFill instance the Outline component added to one of our renderers.
    private void ResolveFillMaterial()
    {
        if (_fillMaterial != null) return;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null && mat.name.StartsWith("OutlineFill"))
                {
                    _fillMaterial = mat;
                    return;
                }
            }
        }
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

    private void OnValidate()
    {
        if (outline == null) outline = GetComponent<Outline>();
        if (hintMaxWidth < hintMinWidth) hintMaxWidth = hintMinWidth;
        ApplyFillMode();
    }
}
