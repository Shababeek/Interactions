using NUnit.Framework;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class RigVisualizerTests
    {
        [Test]
        public void Init_DoesNotAddComponentsToTheBones()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Bone");
            var grandChild = new GameObject("Tip");
            child.transform.SetParent(root.transform);
            grandChild.transform.SetParent(child.transform);
            var visualizer = root.AddComponent<RigVisualizer>();

            visualizer.Init();

            Assert.AreSame(visualizer, visualizer.root);
            Assert.IsNull(child.GetComponent<RigVisualizer>());
            Assert.IsNull(grandChild.GetComponent<RigVisualizer>());
            Object.DestroyImmediate(root);
        }
    }
}
