using Crashmania.UI.Components;
using NUnit.Framework;
using UnityEngine;

namespace Crashmania.Tests
{
    public sealed class SafeAreaPanelTests
    {
        [Test]
        public void ApplySafeAreaDoesNotThrow()
        {
            var root = new GameObject("SafeAreaTest", typeof(RectTransform));
            var panel = root.AddComponent<SafeAreaPanel>();

            Assert.DoesNotThrow(panel.ApplySafeArea);

            Object.DestroyImmediate(root);
        }
    }
}
