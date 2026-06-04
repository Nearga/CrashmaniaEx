#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Crashmania.Editor
{
    public static class FixFontAtlasTextures
    {
        private const string SanityCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;!?+-/()[]%$#@'_";

        private static readonly string[] FontAssetPaths =
        {
            "Assets/UI/Fonts/TMP/Murecho-Black SDF.asset",
            "Assets/UI/Fonts/TMP/Murecho-Bold SDF.asset",
            "Assets/UI/Fonts/TMP/Murecho-Regular SDF.asset",
            "Assets/UI/Fonts/TMP/Murecho-SemiBold SDF.asset",
            "Assets/UI/Fonts/TMP/SairaCondensed-Black SDF.asset"
        };

        [MenuItem("Crashmania/Fix Font Atlas Textures")]
        public static void Run()
        {
            var repairedPaths = new List<string>();
            foreach (var assetPath in FontAssetPaths)
            {
                var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                if (fontAsset == null)
                {
                    Debug.LogWarning($"[FixFontAtlasTextures] Font asset not found at {assetPath}");
                    continue;
                }

                var repaired = EnsureAtlas(fontAsset);
                repaired |= EnsureMaterial(fontAsset);
                repaired |= EnsureGlyphData(fontAsset, assetPath);
                if (repaired)
                {
                    EditorUtility.SetDirty(fontAsset);
                    repairedPaths.Add(assetPath);
                }

                AssertHealthy(fontAsset, assetPath);
            }

            if (repairedPaths.Count > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[FixFontAtlasTextures] Repaired {repairedPaths.Count} font assets: {string.Join(", ", repairedPaths)}");
            }
            else
            {
                Debug.Log("[FixFontAtlasTextures] All approved font assets are healthy.");
            }
        }

        private static bool EnsureAtlas(TMP_FontAsset fontAsset)
        {
            if (fontAsset.atlasTextures != null &&
                fontAsset.atlasTextures.Length > 0 &&
                fontAsset.atlasTextures[0] != null)
            {
                return false;
            }

            var texture = new Texture2D(1024, 1024, TextureFormat.Alpha8, false)
            {
                name = fontAsset.name + " Atlas"
            };
            AssetDatabase.AddObjectToAsset(texture, fontAsset);

            var serializedObject = new SerializedObject(fontAsset);
            var atlasTextures = serializedObject.FindProperty("m_AtlasTextures");
            atlasTextures.ClearArray();
            atlasTextures.InsertArrayElementAtIndex(0);
            atlasTextures.GetArrayElementAtIndex(0).objectReferenceValue = texture;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool EnsureMaterial(TMP_FontAsset fontAsset)
        {
            var atlas = fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0
                ? fontAsset.atlasTextures[0]
                : null;
            var material = fontAsset.material;
            var repaired = false;

            if (material == null)
            {
                var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                if (shader == null)
                {
                    throw new InvalidOperationException($"{fontAsset.name} has no TMP material and no TMP distance-field shader is available.");
                }

                material = new Material(shader) { name = fontAsset.name + " Material" };
                AssetDatabase.AddObjectToAsset(material, fontAsset);
                var serializedObject = new SerializedObject(fontAsset);
                serializedObject.FindProperty("m_Material").objectReferenceValue = material;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                repaired = true;
            }

            if (atlas != null && material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != atlas)
            {
                material.SetTexture("_MainTex", atlas);
                EditorUtility.SetDirty(material);
                repaired = true;
            }

            return repaired;
        }

        private static bool EnsureGlyphData(TMP_FontAsset fontAsset, string context)
        {
            if (fontAsset.characterTable != null && fontAsset.characterTable.Count > 0 &&
                fontAsset.glyphTable != null && fontAsset.glyphTable.Count > 0)
            {
                return false;
            }

            if (fontAsset.sourceFontFile == null)
            {
                throw new InvalidOperationException($"{context} has no source font and cannot regenerate glyph data.");
            }

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            if (!fontAsset.TryAddCharacters(SanityCharacters, out var missingCharacters))
            {
                throw new InvalidOperationException($"{context} could not generate required sanity characters. Missing: {missingCharacters}");
            }

            return true;
        }

        private static void AssertHealthy(TMP_FontAsset fontAsset, string context)
        {
            if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0 || fontAsset.atlasTextures[0] == null)
            {
                throw new InvalidOperationException($"{context} has no valid atlas texture.");
            }

            if (fontAsset.material == null ||
                !fontAsset.material.HasProperty("_MainTex") ||
                fontAsset.material.GetTexture("_MainTex") == null)
            {
                throw new InvalidOperationException($"{context} has no valid TMP material.");
            }

            if (fontAsset.characterTable == null || fontAsset.characterTable.Count == 0 ||
                fontAsset.glyphTable == null || fontAsset.glyphTable.Count == 0)
            {
                throw new InvalidOperationException($"{context} has no TMP character/glyph data.");
            }

            var canvasObject = new GameObject("TMP Font Sanity Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.hideFlags = HideFlags.HideAndDontSave;
            var textObject = new GameObject("TMP Font Sanity Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.hideFlags = HideFlags.HideAndDontSave;
            textObject.transform.SetParent(canvasObject.transform, false);

            try
            {
                var text = textObject.GetComponent<TextMeshProUGUI>();
                text.font = fontAsset;
                text.fontSharedMaterial = fontAsset.material;
                text.text = "ABC123 1.25x BET + -";
                text.rectTransform.sizeDelta = new Vector2(600f, 120f);
                text.ForceMeshUpdate(true, true);
                if (text.mesh == null || text.mesh.vertexCount == 0)
                {
                    throw new InvalidOperationException($"{context} failed TMP mesh generation.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(textObject);
                UnityEngine.Object.DestroyImmediate(canvasObject);
            }
        }
    }
}
#endif
