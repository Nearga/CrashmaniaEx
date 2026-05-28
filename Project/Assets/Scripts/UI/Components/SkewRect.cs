using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    public sealed class SkewRect : BaseMeshEffect
    {
        [SerializeField] private float angle = -5f;

        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                graphic.SetVerticesDirty();
            }
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

            var centerY = (minY + maxY) * 0.5f;
            var shear = Mathf.Tan(angle * Mathf.Deg2Rad);
            for (var index = 0; index < vertices.Count; index++)
            {
                var vertex = vertices[index];
                var position = vertex.position;
                position.x += (position.y - centerY) * shear;
                vertex.position = position;
                vertices[index] = vertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}
