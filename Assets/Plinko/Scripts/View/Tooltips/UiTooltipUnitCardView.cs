using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Tooltips
{
    public sealed class UiTooltipUnitCardView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private RectTransform statsRoot;
        [SerializeField] private UnitStatEntryView statEntryPrefab;

        private readonly List<UnitStatEntryView> _statViews = new();

        public RectTransform RectTransform => root;

        public void Refresh(UnitTooltipViewData viewData)
        {
            if (portraitImage != null)
            {
                portraitImage.sprite = viewData != null ? viewData.PortraitSprite : null;
                portraitImage.enabled = portraitImage.sprite != null;
            }

            if (nameText != null)
            {
                nameText.text = viewData != null ? viewData.DisplayName : string.Empty;
            }

            if (manaText != null)
            {
                manaText.text = viewData != null ? viewData.ManaCost.ToString() : string.Empty;
            }

            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, viewData != null ? viewData.Stats : null);
        }
    }
}
