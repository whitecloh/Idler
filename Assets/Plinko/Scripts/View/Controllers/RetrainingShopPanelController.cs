using System.Collections;
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
    public sealed class RetrainingShopPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private RectTransform offersRoot;
        [SerializeField] private RetrainingShopOfferCardView offerCardPrefab;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyPriceText;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollPriceText;
        [SerializeField] private RectTransform animationLayerRoot;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float offerRevealDuration = 0.28f;
        [SerializeField] private float offerDismissDuration = 0.22f;
        [SerializeField] private float purchaseSequenceInterval = 0.5f;
        [SerializeField] private float dismissYOffset = 100f;

        private readonly Dictionary<int, RetrainingShopOfferCardView> _viewsByRuntimeId = new();
        private readonly Dictionary<int, RetrainingOfferViewData> _offersByRuntimeId = new();
        private readonly List<int> _orderedRuntimeIds = new();
        private RetrainingPhaseBridge _retrainingPhaseBridge;
        private bool _listenersBound;
        private bool _isPurchaseAnimating;
        private string _levelKey = string.Empty;
        private RetrainingPhaseViewData _deferredViewData;
        private RetrainingNextLevelPanelController _sourcePanel;
        private Coroutine _purchaseRoutine;

        public void Init(RetrainingPhaseBridge retrainingPhaseBridge)
        {
            _retrainingPhaseBridge = retrainingPhaseBridge;
            if (_listenersBound)
            {
                return;
            }

            buyButton.onClick.AddListener(HandleBuyClicked);
            rerollButton.onClick.AddListener(HandleRerollClicked);
            _listenersBound = true;
        }

        public void ResetState()
        {
            if (_purchaseRoutine != null)
            {
                StopCoroutine(_purchaseRoutine);
                _purchaseRoutine = null;
            }

            _isPurchaseAnimating = false;
            _deferredViewData = null;
            _sourcePanel = null;
            _levelKey = string.Empty;
            ClearViews();
            ApplyButtons(false, false, 0, 0);
        }

        public void ShowIntroState(RetrainingPhaseViewData viewData)
        {
            if (_levelKey != viewData.LevelKey)
            {
                ClearViews();
                _levelKey = viewData.LevelKey;
            }

            ApplyButtons(false, false, 0, 0);
        }

        public void Refresh(RetrainingPhaseViewData viewData, RetrainingNextLevelPanelController sourcePanel)
        {
            _sourcePanel = sourcePanel;
            if (_levelKey != viewData.LevelKey)
            {
                ClearViews();
                _levelKey = viewData.LevelKey;
            }

            if (_isPurchaseAnimating)
            {
                _deferredViewData = viewData;
                goldText.text = viewData.CurrentGold.ToString();
                ApplyButtons(false, false, viewData.BatchPrice, viewData.RerollPrice);
                return;
            }

            goldText.text = viewData.CurrentGold.ToString();
            ApplyOffers(viewData.Offers);
            ApplyButtons(viewData.CanBuyBatch, viewData.CanReroll, viewData.BatchPrice, viewData.RerollPrice);
        }

        private void HandleBuyClicked()
        {
            if (_isPurchaseAnimating || _orderedRuntimeIds.Count == 0)
            {
                return;
            }

            UiAnimationManager.Instance.PlaySpringPunch(buyButton.transform as RectTransform);
            AudioManager.Instance?.Play(GameAudioCueType.PurchaseGold);
            _retrainingPhaseBridge.RequestBuyBatch();
            _isPurchaseAnimating = true;
            ApplyButtons(false, false, 0, 0);
            _purchaseRoutine = StartCoroutine(PlayPurchaseSequence());
        }

        private void HandleRerollClicked()
        {
            if (_isPurchaseAnimating)
            {
                return;
            }

            UiAnimationManager.Instance.PlaySpringPunch(rerollButton.transform as RectTransform);
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
            _retrainingPhaseBridge.RequestRerollShop();
        }

        private IEnumerator PlayPurchaseSequence()
        {
            var runtimeIds = new List<int>(_orderedRuntimeIds);
            for (var index = 0; index < runtimeIds.Count; index++)
            {
                var runtimeId = runtimeIds[index];
                if (_viewsByRuntimeId.TryGetValue(runtimeId, out var view) &&
                    _offersByRuntimeId.TryGetValue(runtimeId, out var offer))
                {
                    var ghost = Instantiate(offerCardPrefab, animationLayerRoot);
                    ghost.Refresh(offer);
                    ghost.RectTransform.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(
                        animationLayerRoot,
                        uiCamera,
                        UiRectTransformUtility.GetWorldCenter(view.RectTransform));
                    ghost.RectTransform.localScale = Vector3.one;

                    Destroy(view.gameObject);
                    _viewsByRuntimeId.Remove(runtimeId);
                    _offersByRuntimeId.Remove(runtimeId);

                    UiAnimationManager.Instance.PlayMoveAndScale(
                        ghost.RectTransform,
                        $"retraining-shop-dismiss-{runtimeId}",
                        ghost.RectTransform.anchoredPosition + Vector2.up * dismissYOffset,
                        Vector3.zero,
                        offerDismissDuration,
                        Ease.OutQuad,
                        Ease.InBack,
                        () =>
                        {
                            if (ghost != null)
                            {
                                Destroy(ghost.gameObject);
                            }
                        });
                }

                yield return new WaitForSecondsRealtime(purchaseSequenceInterval);
            }

            _orderedRuntimeIds.Clear();
            _isPurchaseAnimating = false;
            _purchaseRoutine = null;

            if (_deferredViewData != null)
            {
                var deferred = _deferredViewData;
                _deferredViewData = null;
                ApplyOffers(deferred.Offers);
                ApplyButtons(deferred.CanBuyBatch, deferred.CanReroll, deferred.BatchPrice, deferred.RerollPrice);
            }
        }

        private void ApplyOffers(IReadOnlyList<RetrainingOfferViewData> offers)
        {
            var activeRuntimeIds = new HashSet<int>();
            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index];
                activeRuntimeIds.Add(offer.RuntimeId);
                _offersByRuntimeId[offer.RuntimeId] = offer;

                var isNew = false;
                if (!_viewsByRuntimeId.TryGetValue(offer.RuntimeId, out var view))
                {
                    view = Instantiate(offerCardPrefab, offersRoot);
                    _viewsByRuntimeId[offer.RuntimeId] = view;
                    isNew = true;
                }

                view.transform.SetSiblingIndex(index);
                view.Refresh(offer);

                if (isNew)
                {
                    AnimateNewOffer(view, offer);
                }
                else
                {
                    view.RectTransform.localScale = Vector3.one;
                }
            }

            _orderedRuntimeIds.Clear();
            for (var index = 0; index < offers.Count; index++)
            {
                _orderedRuntimeIds.Add(offers[index].RuntimeId);
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in _viewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
                {
                    staleRuntimeIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleRuntimeIds.Count; index++)
            {
                var runtimeId = staleRuntimeIds[index];
                Destroy(_viewsByRuntimeId[runtimeId].gameObject);
                _viewsByRuntimeId.Remove(runtimeId);
                _offersByRuntimeId.Remove(runtimeId);
            }
        }

        private void AnimateNewOffer(RetrainingShopOfferCardView view, RetrainingOfferViewData offer)
        {
            Canvas.ForceUpdateCanvases();
            if (_sourcePanel != null && _sourcePanel.TryGetLastKnownPendingWorldPosition(offer.RuntimeId, out var sourceWorldPosition))
            {
                view.RectTransform.localScale = Vector3.zero;
                var targetAnchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(
                    animationLayerRoot,
                    uiCamera,
                    UiRectTransformUtility.GetWorldCenter(view.RectTransform));

                var ghost = Instantiate(offerCardPrefab, animationLayerRoot);
                ghost.Refresh(offer);
                ghost.RectTransform.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(
                    animationLayerRoot,
                    uiCamera,
                    sourceWorldPosition);
                ghost.RectTransform.localScale = Vector3.one;

                UiAnimationManager.Instance.PlayMoveAndScale(
                    ghost.RectTransform,
                    $"retraining-shop-reveal-{offer.RuntimeId}",
                    targetAnchoredPosition,
                    Vector3.one * 0.9f,
                    offerRevealDuration,
                    Ease.OutCubic,
                    Ease.OutQuad,
                    () =>
                    {
                        if (ghost != null)
                        {
                            Destroy(ghost.gameObject);
                        }

                        UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, $"retraining-shop-show-{offer.RuntimeId}", Vector3.one, 0.18f, Ease.OutBack);
                    });
            }
            else
            {
                view.RectTransform.localScale = Vector3.zero;
                UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, $"retraining-shop-show-{offer.RuntimeId}", Vector3.one, 0.18f, Ease.OutBack);
            }
        }

        private void ApplyButtons(bool canBuy, bool canReroll, int batchPrice, int rerollPrice)
        {
            buyButton.gameObject.SetActive(batchPrice > 0);
            rerollButton.gameObject.SetActive(rerollPrice > 0 || _orderedRuntimeIds.Count > 0);
            buyButton.interactable = canBuy;
            rerollButton.interactable = canReroll;
            buyPriceText.text = batchPrice.ToString();
            rerollPriceText.text = rerollPrice.ToString();
        }

        private void ClearViews()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _viewsByRuntimeId.Clear();
            _offersByRuntimeId.Clear();
            _orderedRuntimeIds.Clear();
        }
    }
}
