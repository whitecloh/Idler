using System;
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
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private TMP_Text moveSpeedText;
        [SerializeField] private TMP_Text attackRangeText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

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

        public void Refresh(UnitShopOfferViewData viewData, bool canBuy)
        {
            _offerId = viewData.OfferId;
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            attackText.text = viewData.Attack.ToString();
            healthText.text = viewData.Health.ToString();
            manaText.text = viewData.ManaCost.ToString();
            if (moveSpeedText != null)
            {
                moveSpeedText.text = viewData.MoveSpeed.ToString("0.##");
            }

            if (attackRangeText != null)
            {
                attackRangeText.text = viewData.AttackRange.ToString();
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text = viewData.AttackSpeed.ToString("0.##");
            }

            priceText.text = viewData.Price.ToString();
            buyButton.interactable = canBuy;
        }

        public OfferVisualSnapshot CaptureSnapshot()
        {
            return new OfferVisualSnapshot
            {
                Portrait = portraitImage.sprite,
                Name = nameText.text,
                Attack = attackText.text,
                Health = healthText.text,
                Mana = manaText.text,
                MoveSpeed = moveSpeedText != null ? moveSpeedText.text : string.Empty,
                AttackRange = attackRangeText != null ? attackRangeText.text : string.Empty,
                AttackSpeed = attackSpeedText != null ? attackSpeedText.text : string.Empty,
                Price = priceText.text
            };
        }

        public void ApplySnapshot(OfferVisualSnapshot snapshot)
        {
            portraitImage.sprite = snapshot.Portrait;
            portraitImage.enabled = snapshot.Portrait != null;
            nameText.text = snapshot.Name;
            attackText.text = snapshot.Attack;
            healthText.text = snapshot.Health;
            manaText.text = snapshot.Mana;
            if (moveSpeedText != null)
            {
                moveSpeedText.text = snapshot.MoveSpeed;
            }

            if (attackRangeText != null)
            {
                attackRangeText.text = snapshot.AttackRange;
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text = snapshot.AttackSpeed;
            }

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
            public string Attack;
            public string Health;
            public string Mana;
            public string MoveSpeed;
            public string AttackRange;
            public string AttackSpeed;
            public string Price;
        }
    }
}
