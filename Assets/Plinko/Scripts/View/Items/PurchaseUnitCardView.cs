using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseUnitCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private RectTransform statsRoot;
        [SerializeField] private UnitStatEntryView statEntryPrefab;
        [SerializeField] private RectTransform tooltipAnchor;

        private readonly List<UnitStatEntryView> _statViews = new();
        private UnitTooltipViewData _tooltipViewData = new();

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseTrainedUnitCardViewData viewData)
        {
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            if (manaText != null)
            {
                manaText.text = viewData.ManaCost.ToString();
            }
            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, viewData.Stats);
            _tooltipViewData = UnitTooltipViewDataFactory.FromPurchaseUnit(viewData);
        }

        public void Refresh(SignalPurchasePendingUnitCardViewData viewData)
        {
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            if (manaText != null)
            {
                manaText.text = viewData.ManaCost.ToString();
            }
            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, viewData.Stats);
            _tooltipViewData = UnitTooltipViewDataFactory.FromSignalPendingUnit(viewData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowUnitCard(
                this,
                tooltipAnchor != null ? tooltipAnchor : root,
                _tooltipViewData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.Hide(this);
        }

        private void OnDisable()
        {
            UiTooltipManager.Instance?.Hide(this);
        }
    }
}
