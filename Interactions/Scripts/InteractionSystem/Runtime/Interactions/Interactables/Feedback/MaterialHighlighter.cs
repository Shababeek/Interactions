using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// changes the material color of all child renderers onHover event 
    /// </summary>
    [RequireComponent(typeof(InteractableBase))]
    public class MaterialHighlighter : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField]private string colorPropertyName;
        [SerializeField] private Color highlightColor;
        private Color[] _color;

        private void Awake()
        {
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