using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Lobby
{
    public sealed class CategoryChipView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;

        private string categoryId;

        public string CategoryId => categoryId;

        public event Action<string> Selected;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (background == null) background = GetComponent<Image>();
            if (label == null) label = transform.Find("Label")?.GetComponent<TMP_Text>();

            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }

        public void Bind(string id, string text, bool active)
        {
            categoryId = id;
            if (label == null) label = transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null) label.text = text;
            SetActive(active);
        }

        public void SetActive(bool active)
        {
            if (background == null) background = GetComponent<Image>();
            if (background != null)
            {
                // Active has solid gold/yellow background; Inactive has fully transparent background
                background.color = active
                    ? new Color(1f, 0.79f, 0.12f, 1f)
                    : new Color(0f, 0f, 0f, 0f);
            }

            if (label == null) label = transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null)
            {
                label.color = active ? Color.black : Color.white;
            }
        }

        private void OnClicked()
        {
            Selected?.Invoke(categoryId);
        }
    }
}
