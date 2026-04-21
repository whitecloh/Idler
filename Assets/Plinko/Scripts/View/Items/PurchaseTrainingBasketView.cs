using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseTrainingBasketView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text manaText;

        private PurchaseFieldBasketViewData _viewData = new();

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseFieldBasketViewData viewData)
        {
            _viewData = viewData;
            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;
            manaText.text = viewData.ManaValue.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowText(this, _viewData.TooltipText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.Hide(this);
        }

        public void PlayPunch()
        {
            UiAnimationManager.Instance.PlaySpringPunch(root);
        }

        private void OnDisable()
        {
            UiTooltipManager.Instance?.Hide(this);
        }
    }
}
