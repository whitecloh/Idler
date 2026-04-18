using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchaseShopPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private RectTransform offersRoot;
        [SerializeField] private PurchaseShopOfferCardView offerCardPrefab;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollPriceText;
        [SerializeField] private RectTransform animationLayerRoot;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float purchaseFlyDuration = 0.28f;
        [SerializeField] private float purchaseFlyOffset = 120f;

        private readonly List<PurchaseShopOfferCardView> _offerViews = new();
        private readonly Dictionary<int, PurchaseShopOfferCardView> _offerViewsById = new();
        private readonly HashSet<int> _animatedRuntimeIds = new();
        private PurchasePhaseBridge _purchasePhaseBridge;
        private string _levelKey = string.Empty;
        private bool _listenersBound;

        public void Init(PurchasePhaseBridge purchasePhaseBridge)
        {
            _purchasePhaseBridge = purchasePhaseBridge;
            BindListeners();
        }

        public void ResetState()
        {
            _levelKey = string.Empty;
            _animatedRuntimeIds.Clear();

            for (var index = 0; index < _offerViews.Count; index++)
            {
                Destroy(_offerViews[index].gameObject);
            }

            _offerViews.Clear();
            _offerViewsById.Clear();
        }

        public void Refresh(PurchasePhaseViewData viewData)
        {
            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                _animatedRuntimeIds.Clear();
            }

            AnimateStartedPurchases(viewData.StartedTrainings);
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
                _purchasePhaseBridge.RequestRerollShop();
            });
            _listenersBound = true;
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
            _offerViewsById.Clear();

            for (var index = 0; index < offers.Count; index++)
            {
                var view = Instantiate(offerCardPrefab, offersRoot);
                view.Bind(offers[index].OfferId, HandleBuyClicked);
                _offerViews.Add(view);
                _offerViewsById[offers[index].OfferId] = view;
            }
        }

        private void ApplyOffers(PurchasePhaseViewData viewData)
        {
            for (var index = 0; index < viewData.Offers.Count; index++)
            {
                var offer = viewData.Offers[index];
                var canBuy = viewData.Gold >= offer.Price;
                _offerViews[index].Refresh(offer, canBuy);
            }
        }

        private void AnimateStartedPurchases(IReadOnlyList<PurchaseTrainingStartedViewData> startedTrainings)
        {
            for (var index = 0; index < startedTrainings.Count; index++)
            {
                var started = startedTrainings[index];
                if (_animatedRuntimeIds.Contains(started.RuntimeId) || !_offerViewsById.TryGetValue(started.SourceOfferId, out var view))
                {
                    continue;
                }

                _animatedRuntimeIds.Add(started.RuntimeId);
                var ghost = Instantiate(offerCardPrefab, animationLayerRoot);
                var snapshot = view.CaptureSnapshot();
                ghost.ApplySnapshot(snapshot);
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
                    "purchase-offer-fly",
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
            _purchasePhaseBridge.RequestBuyUnit(offerId);
        }
    }
}
