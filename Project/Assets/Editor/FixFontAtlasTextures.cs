#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace Crashmania.Editor
{
    public static class FixFontAtlasTextures
    {
        [MenuItem("Crashmania/Fix Font Atlas Textures")]
        public static void Run()
        {
            string[] sdfPaths = {
                "Assets/UI/Fonts/TMP/Murecho-Black SDF.asset",
                "Assets/UI/Fonts/TMP/Murecho-Bold SDF.asset",
                "Assets/UI/Fonts/TMP/Murecho-Regular SDF.asset",
                "Assets/UI/Fonts/TMP/Murecho-SemiBold SDF.asset",
                "Assets/UI/Fonts/TMP/SairaCondensed-Black SDF.asset"
            };

            int fixedCount = 0;

            foreach (var sdfPath in sdfPaths)
            {
                var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath);
                if (fontAsset == null)
                {
                    Debug.LogWarning($"[FixFontAtlasTextures] Font asset not found at {sdfPath}");
                    continue;
                }

                // Check if the atlas texture array is empty or contains null references
                bool needsFix = fontAsset.atlasTextures == null || 
                               fontAsset.atlasTextures.Length == 0 || 
                               fontAsset.atlasTextures[0] == null;

                if (needsFix)
                {
                    // Create a new Texture2D as a sub-asset to act as the dynamic texture buffer
                    Texture2D tex = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
                    tex.name = fontAsset.name + " Atlas";
                    
                    // Add it directly as a sub-asset inside the .asset file
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                    
                    // Assign the texture using SerializedObject to bypass read-only fields
                    SerializedObject so = new SerializedObject(fontAsset);
                    SerializedProperty atlasTexturesProp = so.FindProperty("m_AtlasTextures");
                    if (atlasTexturesProp != null)
                    {
                        atlasTexturesProp.ClearArray();
                        atlasTexturesProp.InsertArrayElementAtIndex(0);
                        atlasTexturesProp.GetArrayElementAtIndex(0).objectReferenceValue = tex;
                    }
                    
                    SerializedProperty atlasProp = so.FindProperty("atlas");
                    if (atlasProp != null)
                    {
                        atlasProp.objectReferenceValue = tex;
                    }
                    
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(fontAsset);
                    fixedCount++;
                    Debug.Log($"[FixFontAtlasTextures] Surgically restored dynamic atlas sub-asset texture for: {sdfPath}");
                }
            }

            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[FixFontAtlasTextures] Successfully fixed {fixedCount} font assets!");
            }
            else
            {
                Debug.Log("[FixFontAtlasTextures] All font assets already have valid atlas textures assigned.");
            }
        }
    }
}
#endif
