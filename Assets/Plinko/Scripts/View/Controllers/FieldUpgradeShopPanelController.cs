using System.Collections.Generic;
using DG.Tweening;
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
    public sealed class FieldUpgradeShopPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private RectTransform offersRoot;
        [SerializeField] private FieldUpgradePinOfferCardView offerCardPrefab;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollPriceText;
        [SerializeField] private RectTransform animationLayerRoot;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float purchaseFlyDuration = 0.28f;
        [SerializeField] private float purchaseFlyOffset = 120f;

        private readonly List<FieldUpgradePinOfferCardView> _offerViews = new();
        private readonly Dictionary<int, FieldUpgradePinOfferCardView> _offerViewsById = new();
        private readonly HashSet<int> _animatedOfferIds = new();
        private FieldUpgradeBridge _fieldUpgradeBridge;
        private string _levelKey = string.Empty;
        private bool _listenersBound;

        public void Init(FieldUpgradeBridge fieldUpgradeBridge)
        {
            _fieldUpgradeBridge = fieldUpgradeBridge;
            BindListeners();
        }

        public void ResetState()
        {
            _levelKey = string.Empty;
            _animatedOfferIds.Clear();
            for (var index = 0; index < _offerViews.Count; index++)
            {
                Destroy(_offerViews[index].gameObject);
            }

            _offerViews.Clear();
            _offerViewsById.Clear();
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
        {
            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                _animatedOfferIds.Clear();
            }

            AnimateStartedPurchases(viewData.StartedPurchases);
            EnsureOfferViews(viewData.Offers);
            ApplyOffers(viewData);
            goldText.text = viewData.Gold.ToString();
            rerollPriceText.text = viewData.RerollPrice.ToString();
            rerollButton.interactable = viewData.CanReroll;
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
                _fieldUpgradeBridge.RequestRerollShop();
            });
            _listenersBound = true;
        }

        private void EnsureOfferViews(IReadOnlyList<PinOfferViewData> offers)
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
            _offerViewsById.Clear();

            for (var index = 0; index < offers.Count; index++)
            {
                var view = Instantiate(offerCardPrefab, offersRoot);
                view.Bind(offers[index].OfferId, HandleBuyClicked);
                _offerViews.Add(view);
                _offerViewsById[offers[index].OfferId] = view;
            }
        }

        private void ApplyOffers(FieldUpgradePhaseViewData viewData)
        {
            for (var index = 0; index < viewData.Offers.Count; index++)
            {
                var offer = viewData.Offers[index];
                var canBuy = viewData.Gold >= offer.Price && !viewData.HasPendingPin;
                _offerViews[index].Refresh(offer, canBuy);
            }
        }

        private void AnimateStartedPurchases(IReadOnlyList<FieldUpgradeStartedPurchaseViewData> startedPurchases)
        {
            for (var index = 0; index < startedPurchases.Count; index++)
            {
                var started = startedPurchases[index];
                if (_animatedOfferIds.Contains(started.OfferId) || !_offerViewsById.TryGetValue(started.OfferId, out var view))
                {
                    continue;
                }

                _animatedOfferIds.Add(started.OfferId);
                var ghost = Instantiate(offerCardPrefab, animationLayerRoot);
                ghost.ApplySnapshot(view.CaptureSnapshot());
                ghost.SetInteractable(false);

                var ghostRect = ghost.RectTransform;
                var sourceRect = view.RectTransform;
                ghostRect.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(
                    animationLayerRoot,
                    uiCamera,
                    UiRectTransformUtility.GetWorldCenter(sourceRect));
                ghostRect.localScale = Vector3.one;

                UiAnimationManager.Instance.PlayMoveAndScale(
                    ghostRect,
                    "field-upgrade-offer-fly",
                    ghostRect.anchoredPosition + Vector2.up * purchaseFlyOffset,
                    Vector3.one * 0.85f,
                    purchaseFlyDuration,
                    Ease.OutCubic,
                    Ease.OutQuad,
                    () => Destroy(ghost.gameObject));
            }
        }

        private void HandleBuyClicked(int offerId)
        {
            _fieldUpgradeBridge.RequestBuyPin(offerId);
        }
    }
}
