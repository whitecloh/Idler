using DG.Tweening;
using Plinko.Scripts.View.Animations;
using TMPro;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class BattleTurnBannerView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private float duration = 0.7f;
        [SerializeField] private float travelDistance = 40f;

        private Vector2 _basePosition;

        private void Awake()
        {
            _basePosition = root.anchoredPosition;
        }

        public void ShowTurn(int turnIndex)
        {
            ShowText($"Ход {turnIndex}");
        }

        public void ShowText(string textValue)
        {
            root.gameObject.SetActive(true);
            valueText.text = textValue;
            root.anchoredPosition = _basePosition;
            root.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            UiAnimationManager.Instance.PlayFloatAndFade(
                root,
                canvasGroup,
                "turn-banner",
                _basePosition + Vector2.up * travelDistance,
                Vector3.one * 0.96f,
                duration,
                Ease.OutCubic,
                Ease.OutQuad,
                HideImmediate);
        }

        public void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            root.anchoredPosition = _basePosition;
            root.localScale = Vector3.one;
            root.gameObject.SetActive(false);
        }
    }
}
