using Crashmania.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Lobby
{
    public sealed class StoreItemCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text ccAmountText;
        [SerializeField] private TMP_Text scBonusText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button purchaseButton;

        public event System.Action<string> PurchaseClicked;
        private string packageId;

        private void Awake()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(() => PurchaseClicked?.Invoke(packageId));
            }
        }

        public void Bind(StorePackage package)
        {
            packageId = package.Id;
            if (ccAmountText != null) ccAmountText.text = package.CoinsCC.ToString("N0");
            if (scBonusText != null) scBonusText.text = $"+ {package.BonusSC:N2} SC";
            if (priceText != null) priceText.text = package.PriceLabel;
        }
    }
}
