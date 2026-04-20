using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class SignalPurchaseShopPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private RectTransform offersRoot;
        [SerializeField] private PurchaseShopOfferCardView offerCardPrefab;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollPriceText;

        private readonly List<PurchaseShopOfferCardView> _offerViews = new();
        private SignalPurchaseBridge _signalPurchaseBridge;
        private string _levelKey = string.Empty;
        private bool _listenersBound;

        public void Init(SignalPurchaseBridge signalPurchaseBridge)
        {
            _signalPurchaseBridge = signalPurchaseBridge;
            BindListeners();
        }

        public void ResetState()
        {
            _levelKey = string.Empty;
            for (var index = 0; index < _offerViews.Count; index++)
            {
                Destroy(_offerViews[index].gameObject);
            }

            _offerViews.Clear();
        }

        public void Refresh(SignalPurchasePhaseViewData viewData)
        {
            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
            }

            EnsureOfferViews(viewData.Offers);
            for (var index = 0; index < viewData.Offers.Count; index++)
            {
                var offer = viewData.Offers[index];
                var canBuy = viewData.CanBuyUnits && viewData.Gold >= offer.Price;
                _offerViews[index].Refresh(offer, canBuy);
            }

            goldText.text = viewData.Gold.ToString();
            rerollPriceText.text = viewData.RerollPrice.ToString();
            rerollButton.interactable = viewData.CanReroll;
        }

        private void EnsureOfferViews(IReadOnlyList<UnitShopOfferViewData> offers)
        {
            var requiresRebuild = _offerViews.Count != offers.Count;
            if (!requiresRebuild)
            {
                for (var index = 0; index < offers.Count; index++)
                {
                    if (_offerViews[index].OfferId != offers[index].OfferId)
                    {
                        requiresRebuild = true;
                        break;
                    }
                }
            }

            if (!requiresRebuild)
            {
                return;
            }

            for (var index = 0; index < _offerViews.Count; index++)
            {
                Destroy(_offerViews[index].gameObject);
            }

            _offerViews.Clear();
            for (var index = 0; index < offers.Count; index++)
            {
                var view = Instantiate(offerCardPrefab, offersRoot);
                view.Bind(offers[index].OfferId, HandleBuyClicked);
                _offerViews.Add(view);
            }
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            rerollButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(rerollButton.transform as RectTransform);
                AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
                _signalPurchaseBridge.RequestRerollShop();
            });
            _listenersBound = true;
        }

        private void HandleBuyClicked(int offerId)
        {
            _signalPurchaseBridge.RequestBuyUnit(offerId);
        }
    }
}
