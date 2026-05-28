using TMPro;
using UnityEngine;

namespace Crashmania.Config
{
    [CreateAssetMenu(fileName = "DesignTokens", menuName = "Lobby/Design Tokens")]
    public sealed class DesignTokens : ScriptableObject
    {
        [Header("Backgrounds")]
        public Color bgMain = new(0.157f, 0.169f, 0.220f);
        public Color bgCard = new(0.227f, 0.259f, 0.314f);
        public Color bgFooter = new(0.102f, 0.114f, 0.141f);
        public Color bgHeader = new(0.282f, 0.325f, 0.392f);

        [Header("Brand")]
        public Color brandPurple = new(0.541f, 0.239f, 0.918f);
        public Color ctaBlueTop = new(0.310f, 0.667f, 1.000f);
        public Color ctaBlueEnd = new(0.110f, 0.310f, 0.780f);
        public Color accentYellow = new(0.996f, 0.867f, 0.141f);
        public Color accentGreen = new(0.059f, 0.824f, 0.314f);

        [Header("Status")]
        public Color errorRed = new(1f, 0.247f, 0.235f);

        [Header("Text")]
        public Color textPrimary = Color.white;
        public Color textSecondary = new(0.639f, 0.659f, 0.718f);

        [Header("Typography")]
        public TMP_FontAsset fontDefault;
        public TMP_FontAsset fontHeading;
        public TMP_FontAsset fontEmphasis;
        public TMP_FontAsset fontDisplay;
    }
}
