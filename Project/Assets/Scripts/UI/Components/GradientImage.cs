using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    [RequireComponent(typeof(Image))]
    public sealed class GradientImage : BaseMeshEffect
    {
        [SerializeField] private Color topColor = new(0.157f, 0.439f, 1f, 1f);
        [SerializeField] private Color bottomColor = new(0.478f, 0.22f, 0.988f, 1f);

        public void SetColors(Color top, Color bottom)
        {
            topColor = top;
            bottomColor = bottom;
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

            var minY = float.MaxValue;
            var maxY = float.MinValue;
            for (var index = 0; index < vertices.Count; index++)
            {
                var y = vertices[index].position.y;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            var height = Mathf.Max(0.001f, maxY - minY);
            for (var index = 0; index < vertices.Count; index++)
            {
                var vertex = vertices[index];
                var t = Mathf.Clamp01((vertex.position.y - minY) / height);
                vertex.color *= Color.Lerp(bottomColor, topColor, t);
                vertices[index] = vertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}
