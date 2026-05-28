using Crashmania.UI.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Tests
{
    public sealed class UiMeshEffectTests
    {
        [Test]
        public void SkewRectModifiesMeshWithoutThrowing()
        {
            var effect = CreateEffectObject<SkewRect>("SkewRectTest");
            var helper = CreateQuad();

            Assert.DoesNotThrow(() => effect.ModifyMesh(helper));
            Assert.Greater(helper.currentVertCount, 0);

            Object.DestroyImmediate(effect.gameObject);
        }

        [Test]
        public void GradientImageModifiesMeshWithoutThrowing()
        {
            var effect = CreateEffectObject<GradientImage>("GradientImageTest");
            var helper = CreateQuad();

            Assert.DoesNotThrow(() => effect.ModifyMesh(helper));
            Assert.Greater(helper.currentVertCount, 0);

            Object.DestroyImmediate(effect.gameObject);
        }

        private static T CreateEffectObject<T>(string name) where T : Component
        {
            var gameObject = new GameObject(name);
            gameObject.AddComponent<Image>();
            return gameObject.AddComponent<T>();
        }

        private static VertexHelper CreateQuad()
        {
            var helper = new VertexHelper();
            AddVertex(helper, -1f, -1f);
            AddVertex(helper, -1f, 1f);
            AddVertex(helper, 1f, 1f);
            AddVertex(helper, 1f, -1f);
            helper.AddTriangle(0, 1, 2);
            helper.AddTriangle(2, 3, 0);
            return helper;
        }

        private static void AddVertex(VertexHelper helper, float x, float y)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = new Vector3(x, y, 0f);
            helper.AddVert(vertex);
        }
    }
}
