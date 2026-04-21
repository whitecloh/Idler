using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseTrainingPinView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;

        private PurchaseFieldPinViewData _viewData = new();

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseFieldPinViewData viewData)
        {
            _viewData = viewData;
            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;
            nameText.text = viewData.DisplayName;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowPin(this, _viewData.TooltipText, new FieldUpgradeSelectedPinViewData
            {
                PinTypeId = _viewData.PinTypeId,
                DisplayName = _viewData.DisplayName,
                Sprite = _viewData.Sprite,
                ModifierLines = _viewData.ModifierLines
            });
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
