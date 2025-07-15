using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kandooz
{
    public class RigVisualizer : MonoBehaviour
    {
        [HideInInspector] public RigVisualizer[] children;
        [HideInInspector] public Color color = Color.red;
        [HideInInspector] public GameObject bone;
        [HideInInspector] public RigVisualizer root ;
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
        public void Init()
        {
            if (!transform.parent||!(transform.parent.GetComponent < RigVisualizer>()) )
            {
                root = this;
            }
            children = new RigVisualizer[transform.childCount];
            Debug.Log(children.Length);
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
