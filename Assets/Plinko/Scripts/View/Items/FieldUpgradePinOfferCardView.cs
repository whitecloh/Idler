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
    public sealed class FieldUpgradePinOfferCardView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private RectTransform modifiersRoot;
        [SerializeField] private FieldUpgradeModifierLineView modifierLinePrefab;
        [SerializeField] private Button buyButton;

        private readonly List<FieldUpgradeModifierLineView> _modifierViews = new();
        private int _offerId;

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

        public void Refresh(PinOfferViewData viewData, bool canBuy)
        {
            _offerId = viewData.OfferId;
            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;
            nameText.text = viewData.DisplayName;
            priceText.text = viewData.Price.ToString();
            buyButton.interactable = canBuy;

            SyncModifierLines(viewData.ModifierLines);
        }

        public void SetInteractable(bool isInteractable)
        {
            buyButton.interactable = isInteractable;
        }

        public PinOfferVisualSnapshot CaptureSnapshot()
        {
            var snapshot = new PinOfferVisualSnapshot
            {
                Sprite = iconImage.sprite,
                Name = nameText.text,
                Price = priceText.text
            };

            for (var index = 0; index < _modifierViews.Count; index++)
            {
                snapshot.Modifiers.Add(_modifierViews[index].CaptureSnapshot());
            }

            return snapshot;
        }

        public void ApplySnapshot(PinOfferVisualSnapshot snapshot)
        {
            iconImage.sprite = snapshot.Sprite;
            iconImage.enabled = snapshot.Sprite != null;
            nameText.text = snapshot.Name;
            priceText.text = snapshot.Price;
            SyncModifierLines(snapshot.Modifiers);
        }

        private void SyncModifierLines(IReadOnlyList<PinModifierLineViewData> modifiers)
        {
            var targetCount = modifiers != null ? modifiers.Count : 0;
            while (_modifierViews.Count < targetCount)
            {
                _modifierViews.Add(Instantiate(modifierLinePrefab, modifiersRoot));
            }

            for (var index = _modifierViews.Count - 1; index >= targetCount; index--)
            {
                Destroy(_modifierViews[index].gameObject);
                _modifierViews.RemoveAt(index);
            }

            for (var index = 0; index < targetCount; index++)
            {
                _modifierViews[index].Refresh(modifiers[index]);
            }
        }
    }

    public sealed class PinOfferVisualSnapshot
    {
        public Sprite Sprite;
        public string Name;
        public string Price;
        public List<PinModifierLineViewData> Modifiers = new();
    }
}
