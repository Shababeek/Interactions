using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// [DEPRECATED] Changes the material color of renderers when hovering over an interactable.
    /// Provides visual feedback by dimming the material color during hover events.
    /// </summary>
    /// <remarks>
    /// <b>DEPRECATED:</b> This component is deprecated. Please use the FeedbackSystem with MaterialFeedback instead.
    /// The FeedbackSystem provides more comprehensive feedback options and better integration with the interaction system.
    /// 
    /// To migrate:
    /// 1. Remove this MaterialHighlighter component
    /// 2. Add a FeedbackSystem component to your interactable
    /// 3. Add a MaterialFeedback to the FeedbackSystem
    /// 4. Configure the MaterialFeedback settings for your desired highlighting behavior
    /// </remarks>
    [RequireComponent(typeof(InteractableBase))]
    [System.Obsolete("This component is deprecated. Use FeedbackSystem with MaterialFeedback instead for better integration and more features.")]
    public class MaterialHighlighter : MonoBehaviour
    {
        [Tooltip("Array of renderers to apply color changes to. If empty, uses all child renderers.")]
        [SerializeField] private Renderer[] renderers;
        
        [Tooltip("Name of the material property to modify (e.g., '_Color', '_EmissionColor').")]
        [SerializeField] private string colorPropertyName;
        
        [Tooltip("Color to apply when hovering over the interactable.")]
        [SerializeField] private Color highlightColor;
        private Color[] _color;

        private void Awake()
        {
            // Deprecation warning
            Debug.LogWarning($"[DEPRECATED] MaterialHighlighter on {gameObject.name} is deprecated. " +
                           "Please migrate to FeedbackSystem with MaterialFeedback for better integration and more features. " +
                           "See the component documentation for migration steps.");
            
            renderers ??= GetComponentsInChildren<Renderer>();
            _color = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                _color[i] = renderers[i].material.color;
            }

            _color = new Color[renderers.Length];
            var interactable = GetComponent<InteractableBase>();

            interactable.OnHoverStarted.Do(OnHoverStart).Subscribe().AddTo(this);
            interactable.OnHoverEnded.Do(OnHoverEnded).Subscribe().AddTo(this);
        }

        void OnHoverEnded(InteractorBase interactor)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = _color[i];
            }
        }

        void OnHoverStart(InteractorBase interactor)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = _color[i] * .3f;
            }
        }
    }
}