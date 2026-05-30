using System;
using Crashmania.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Lobby
{
    public sealed class GameCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image thumbnail;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text onlineText;
        [SerializeField] private TMP_Text rankText;

        private string gameId;

        public event Action<string> Selected;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (thumbnail == null) thumbnail = transform.Find("Thumbnail")?.GetComponent<Image>();
            if (nameText == null) nameText = transform.Find("Name")?.GetComponent<TMP_Text>();
            if (onlineText == null) onlineText = transform.Find("Online/Text")?.GetComponent<TMP_Text>();
            if (rankText == null) rankText = transform.Find("RankText")?.GetComponent<TMP_Text>();

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

        public void Bind(GameModel game, int rank = 0)
        {
            gameId = game != null ? game.Id : string.Empty;
            if (nameText != null) nameText.text = game != null ? game.Name : string.Empty;
            if (onlineText != null) onlineText.text = game != null && game.OnlineCount > 0 ? "<color=#11D950>👤</color> " + game.OnlineCount.ToString() : string.Empty;
            if (rankText != null) rankText.text = rank > 0 ? rank.ToString() : string.Empty;

            if (thumbnail != null)
            {
                var sprite = game != null && !string.IsNullOrEmpty(game.ThumbnailResourcePath)
                     ? Resources.Load<Sprite>(game.ThumbnailResourcePath)
                     : null;
                thumbnail.sprite = sprite;
                // Use DesignTokens.bgCard signature blue-grey color (new Color(0.227f, 0.259f, 0.314f, 1f)) for fallback card thumbnail background
                thumbnail.color = sprite != null ? Color.white : new Color(0.227f, 0.259f, 0.314f, 1f);
                thumbnail.preserveAspect = true;
            }
        }

        private void OnClicked()
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.05f, 0.15f, 1, 0.5f);
            Selected?.Invoke(gameId);
        }
    }
}
