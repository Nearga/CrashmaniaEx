using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    /// <summary>
    /// Horizontal vertex-color gradient for UGUI Image components.
    /// Left color → Right color, applied via IMeshModifier so it works
    /// with any Image sprite (including the default white sprite).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class HorizontalGradientImage : BaseMeshEffect
    {
        [SerializeField] private Color leftColor = new(0f, 0f, 0f, 0f);
        [SerializeField] private Color rightColor = new(0f, 0f, 0f, 1f);

        public void SetColors(Color left, Color right)
        {
            leftColor = left;
            rightColor = right;
            graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            var vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);
            if (vertices.Count == 0)
            {
                return;
            }

            var minX = float.MaxValue;
            var maxX = float.MinValue;
            for (var i = 0; i < vertices.Count; i++)
            {
                var x = vertices[i].position.x;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
            }

            var width = Mathf.Max(0.001f, maxX - minX);
            for (var i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                var t = Mathf.Clamp01((vertex.position.x - minX) / width);
                vertex.color *= Color.Lerp(leftColor, rightColor, t);
                vertices[i] = vertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}