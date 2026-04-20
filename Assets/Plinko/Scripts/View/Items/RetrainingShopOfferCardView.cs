using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class RetrainingShopOfferCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private RectTransform statsRoot;
        [SerializeField] private UnitStatEntryView statEntryPrefab;
        [SerializeField] private RectTransform tooltipAnchor;

        private readonly List<UnitStatEntryView> _statViews = new();
        private RetrainingOfferViewData _viewData = new();

        public int RuntimeId { get; private set; }
        public RectTransform RectTransform => root;

        public void Refresh(RetrainingOfferViewData viewData)
        {
            _viewData = viewData;
            RuntimeId = viewData.RuntimeId;
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            if (manaText != null)
            {
                manaText.text = viewData.ManaCost.ToString();
            }
            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, viewData.Stats);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowUnitCard(
                this,
                tooltipAnchor != null ? tooltipAnchor : root,
                UnitTooltipViewDataFactory.FromRetrainingOffer(_viewData));
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
