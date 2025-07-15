using Kandooz.InteractionSystem.Interactions;
using UniRx;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    [RequireComponent(typeof(InteractableBase))]
    public class MaterialHighlighter : MonoBehaviour
    {
        private Renderer[] renderers;
        private Color[] color;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            color = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                color[i] = renderers[i].material.color;
            }
            color = new Color[renderers.Length];
            var interactable = GetComponent<InteractableBase>();

            interactable.OnHoverStarted.Do(OnHoverStart).Subscribe().AddTo(this);


            interactable.OnHoverEnded.Do(OnHoverEnded).Subscribe().AddTo(this);
        }
        void OnHoverEnded( InteractorBase interactor)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = color[i];
            }
        }
        void OnHoverStart(InteractorBase interactor)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = color[i] * .3f;
            }
        }
    }
}