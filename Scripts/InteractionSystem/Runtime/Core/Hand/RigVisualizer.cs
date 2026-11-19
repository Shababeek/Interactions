using UnityEngine;

namespace Shababeek
{
    /// <summary>Draws the bones below this transform as coloured gizmo lines in the Scene view.</summary>
    [AddComponentMenu("Shababeek/Interactions/Debug/Rig Visualizer")]
    public class RigVisualizer : MonoBehaviour
    {
        private const float JointRadius = 0.01f;

        /// <summary>Child rig visualizers for this bone.</summary>
        [HideInInspector] public RigVisualizer[] children;
        /// <summary>Gizmo color for this bone.</summary>
        [HideInInspector] public Color color = Color.red;
        /// <summary>GameObject reference for this bone.</summary>
        [HideInInspector] public GameObject bone;
        /// <summary>Root rig visualizer in the hierarchy.</summary>
        [HideInInspector] public RigVisualizer root;
        /// <summary>Currently selected rig visualizer.</summary>
        public static RigVisualizer selected;

        private void OnDrawGizmos()
        {
            if (HasVisualizerAbove()) return;
            DrawBone(transform, color);
        }

        /// <summary>Collects the visualizers already present on the bones below this one.</summary>
        public void Init()
        {
            if (!HasVisualizerAbove()) root = this;
            children = GetComponentsInChildren<RigVisualizer>(true);
        }

        private bool HasVisualizerAbove()
        {
            return transform.parent && transform.parent.GetComponentInParent<RigVisualizer>();
        }

        private static void DrawBone(Transform bone, Color boneColor)
        {
            Gizmos.color = boneColor;
            Gizmos.DrawSphere(bone.position, JointRadius);

            var childColor = new Color(boneColor.b, boneColor.r, boneColor.g, boneColor.a);
            for (int i = 0; i < bone.childCount; i++)
            {
                var child = bone.GetChild(i);
                if (child.GetComponent<MeshRenderer>()) continue;

                Gizmos.color = boneColor;
                Gizmos.DrawLine(bone.position, child.position);
                DrawBone(child, childColor);
            }
        }
    }
}
