using DG.Tweening;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Items;
using UnityEngine;

namespace Plinko.Scripts.View.Tooltips
{
    public sealed class UiTooltipManager : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas tooltipCanvas;
        [SerializeField] private UiTooltipTextView textTooltipView;
        [SerializeField] private UiTooltipUnitCardView unitCardTooltipView;
        [SerializeField] private FieldUpgradeSelectedPinCardView pinTooltipCardView;
        [SerializeField] private float fadeDuration = 0.12f;

        private Object _currentOwner;
        private Tween _fadeTween;

        public static UiTooltipManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (tooltipCanvas != null)
            {
                tooltipCanvas.overrideSorting = true;
            }

            HideImmediate();
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        public void ShowText(Object owner, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(true);
                textTooltipView.Refresh(text);
            }

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(false);
            }
            if (pinTooltipCardView != null)
            {
                pinTooltipCardView.gameObject.SetActive(false);
            }

            Show(owner);
        }

        public void ShowUnitCard(Object owner, UnitTooltipViewData viewData)
        {
            if (viewData == null)
            {
                return;
            }

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(true);
                unitCardTooltipView.Refresh(viewData);
            }

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(false);
            }
            if (pinTooltipCardView != null)
            {
                pinTooltipCardView.gameObject.SetActive(false);
            }

            Show(owner);
        }

        public void ShowPin(Object owner, string text, FieldUpgradeSelectedPinViewData viewData)
        {
            if (viewData == null)
            {
                return;
            }

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    textTooltipView.Refresh(text);
                }
            }

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(false);
            }

            if (pinTooltipCardView != null)
            {
                pinTooltipCardView.gameObject.SetActive(true);
                pinTooltipCardView.Refresh(viewData);
            }

            Show(owner);
        }

        public void Hide(Object owner)
        {
            if (_currentOwner != null && owner != _currentOwner)
            {
                return;
            }

            HideAnimated();
        }

        public void HideImmediate()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
            _currentOwner = null;

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(false);
            }

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(false);
            }
            if (pinTooltipCardView != null)
            {
                pinTooltipCardView.gameObject.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (root != null)
            {
                root.gameObject.SetActive(true);
                root.SetAsLastSibling();
            }
        }

        private void Show(Object owner)
        {
            if (root == null)
            {
                return;
            }

            _fadeTween?.Kill();
            _currentOwner = owner;
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                _fadeTween = canvasGroup
                    .DOFade(1f, fadeDuration)
                    .SetEase(Ease.OutQuad);
                return;
            }
        }

        private void HideAnimated()
        {
            _fadeTween?.Kill();
            _currentOwner = null;
            if (canvasGroup != null)
            {
                _fadeTween = canvasGroup
                    .DOFade(0f, fadeDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        if (textTooltipView != null)
                        {
                            textTooltipView.gameObject.SetActive(false);
                        }

                        if (unitCardTooltipView != null)
                        {
                            unitCardTooltipView.gameObject.SetActive(false);
                        }

                        if (pinTooltipCardView != null)
                        {
                            pinTooltipCardView.gameObject.SetActive(false);
                        }
                    });
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}
