using UnityEngine;

namespace Crashmania.UI.Components
{
    public static class MobileResolutionPolicy
    {
        public const int MaxCrashGameRenderHeight = 1440;
        public const int MaxHeavy3DRenderHeight = 1080;
        public const int MaxLongScreenWidth = 2340;
        public const int MinRecommendedTargetDpi = 200;
        public const int MaxRecommendedTargetDpi = 300;

        public static void ApplyRuntimePolicy()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = CanvasResolutionPolicy.TargetFrameRate;

#if UNITY_IOS && !UNITY_EDITOR
            var current = Screen.currentResolution;
            var width = current.width > 0 ? current.width : Screen.width;
            var height = current.height > 0 ? current.height : Screen.height;
            var target = CalculateClampedResolution(width, height, MaxCrashGameRenderHeight);
            if (target.x != width || target.y != height)
            {
                Screen.SetResolution(target.x, target.y, true);
            }
#endif
        }

        public static Vector2Int CalculateClampedResolution(int width, int height, int maxLongSide)
        {
            if (width <= 0 || height <= 0 || maxLongSide <= 0)
            {
                return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
            }

            var longSide = Mathf.Max(width, height);
            if (longSide <= maxLongSide)
            {
                return new Vector2Int(width, height);
            }

            var scale = (float)maxLongSide / longSide;
            return new Vector2Int(
                Mathf.Max(1, Mathf.RoundToInt(width * scale)),
                Mathf.Max(1, Mathf.RoundToInt(height * scale)));
        }
    }
}
