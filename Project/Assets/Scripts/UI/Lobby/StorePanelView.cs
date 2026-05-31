using System.Collections.Generic;
using Crashmania.Models;
using UnityEngine;

namespace Crashmania.UI.Lobby
{
    public sealed class StorePanelView : MonoBehaviour
    {
        [SerializeField] private StoreItemCardView itemPrefab;
        [SerializeField] private RectTransform container;

        public event System.Action<string> PurchaseRequested;

        public void Render(List<StorePackage> packages)
        {
            if (container == null || itemPrefab == null) return;

            Clear();

            foreach (var package in packages)
            {
                var item = Instantiate(itemPrefab, container);
                item.Bind(package);
                item.PurchaseClicked += id => PurchaseRequested?.Invoke(id);
            }
        }

        private void Clear()
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}
