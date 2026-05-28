using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    public static class CanvasResolutionPolicy
    {
        public static readonly Vector2 ReferenceResolution = new(1170f, 2532f);

        public const float MatchWidthOrHeight = 0f;
        public const int TargetFrameRate = 60;

        public static void ApplyTo(CanvasScaler scaler)
        {
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
        }
    }
}
