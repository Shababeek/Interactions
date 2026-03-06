using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Shababeek
{
    /// <summary>Visualizes rig bones with colored gizmo lines in the scene editor.</summary>
    public class RigVisualizer : MonoBehaviour
    {
        /// <summary>Child rig visualizers for this bone.</summary>
        [HideInInspector] public RigVisualizer[] children;
        /// <summary>Gizmo color for this bone.</summary>
        [HideInInspector] public Color color = Color.red;
        /// <summary>GameObject reference for this bone.</summary>
        [HideInInspector] public GameObject bone;
        /// <summary>Root rig visualizer in the hierarchy.</summary>
        [HideInInspector] public RigVisualizer root ;
        /// <summary>Currently selected rig visualizer.</summary>
        [HideInInspector] public static RigVisualizer selected;
        void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(this.transform.position, .01f);
            Color childColor = color;
            childColor.r = color.b;
            childColor.g = color.r;
            childColor.b = color.g;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<MeshRenderer>()) continue;
                Gizmos.DrawLine(this.transform.position, child.position);
                var visualizer = child.GetComponent<RigVisualizer>();
                if (!visualizer)
                {
                    child.gameObject.AddComponent<RigVisualizer>().color = childColor;
                }
                else
                {
                    visualizer.color = childColor;
                }

            }

        }
        /// <summary>Initializes the rig visualizer hierarchy.</summary>
        public void Init()
        {
            if (!transform.parent||!(transform.parent.GetComponent < RigVisualizer>()) )
            {
                root = this;
            }
            children = new RigVisualizer[transform.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                var child = transform.GetChild(i);
                children[i] = child.GetComponent< RigVisualizer>();
                if (!children[i])
                {
                    children[i]=child.gameObject.AddComponent<RigVisualizer>();
                }
                children[i].Init();

            }

        }


    }
}
