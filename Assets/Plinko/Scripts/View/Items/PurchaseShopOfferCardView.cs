using System;
using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseShopOfferCardView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private RectTransform statsRoot;
        [SerializeField] private UnitStatEntryView statEntryPrefab;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

        private int _offerId;
        private readonly List<UnitStatEntryView> _statViews = new();
        private readonly List<StatDisplayViewData> _currentStats = new();
        private UnitShopOfferViewData _viewData = new();

        public int OfferId => _offerId;
        public RectTransform RectTransform => root;

        public void Bind(int offerId, Action<int> onBuyClicked)
        {
            _offerId = offerId;
            buyButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(buyButton.transform as RectTransform);
                AudioManager.Instance?.Play(GameAudioCueType.PurchaseGold);
                onBuyClicked.Invoke(_offerId);
            });
        }

        public void Refresh(UnitShopOfferViewData viewData, bool canBuy)
        {
            _viewData = viewData;
            _offerId = viewData.OfferId;
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;

            _currentStats.Clear();
            if (viewData.Stats != null)
            {
                _currentStats.AddRange(viewData.Stats);
            }

            if (manaText != null)
            {
                manaText.text = viewData.ManaCost.ToString();
            }
            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, _currentStats);

            priceText.text = viewData.Price.ToString();
            buyButton.interactable = canBuy;
        }

        public OfferVisualSnapshot CaptureSnapshot()
        {
            var snapshot = new OfferVisualSnapshot
            {
                Portrait = portraitImage.sprite,
                Name = nameText.text,
                Mana = manaText != null ? manaText.text : string.Empty,
                Price = priceText.text
            };

            for (var index = 0; index < _currentStats.Count; index++)
            {
                snapshot.Stats.Add(new StatDisplayViewData
                {
                    StatTypeId = _currentStats[index].StatTypeId,
                    DisplayName = _currentStats[index].DisplayName,
                    Icon = _currentStats[index].Icon,
                    ValueText = _currentStats[index].ValueText
                });
            }

            return snapshot;
        }

        public void ApplySnapshot(OfferVisualSnapshot snapshot)
        {
            portraitImage.sprite = snapshot.Portrait;
            portraitImage.enabled = snapshot.Portrait != null;
            nameText.text = snapshot.Name;

            _currentStats.Clear();
            if (snapshot.Stats != null)
            {
                _currentStats.AddRange(snapshot.Stats);
            }

            if (manaText != null)
            {
                manaText.text = snapshot.Mana;
            }
            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, _currentStats);

            priceText.text = snapshot.Price;
        }

        public void SetInteractable(bool isInteractable)
        {
            buyButton.interactable = isInteractable;
        }

        public sealed class OfferVisualSnapshot
        {
            public Sprite Portrait;
            public string Name;
            public string Mana;
            public string Price;
            public List<StatDisplayViewData> Stats = new();
        }
    }
}
