using DG.Tweening;
using TMPro;
using Plinko.Scripts.View.Animations;
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
            valueText.text = $"Ход {turnIndex}";
            root.anchoredPosition = _basePosition;
            root.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
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
            root.anchoredPosition = _basePosition;
            root.localScale = Vector3.one;
        }
    }
}
