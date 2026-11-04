using Shababeek.Interactions.Animations;
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Runtime-selected <see cref="HandData"/>. A null value means "use the Config's default".
    /// The value is never meant to persist: it clears when the asset loads and when play mode
    /// starts or stops, so a swap made in one play session cannot leak into the next.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Hand Data Variable")]
    public class HandDataVariable : ScriptableVariable<HandData>
    {
        private void OnEnable()
        {
            SetValueWithoutNotify(null);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode ||
                state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                SetValueWithoutNotify(null);
        }
#endif
    }
}
