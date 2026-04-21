using System;
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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BaseDefenseTurnPanelController : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private RectTransform animationLayerRoot;
        [SerializeField] private RectTransform deckAnchor;
        [SerializeField] private RectTransform manaAnchor;
        [SerializeField] private RectTransform handRoot;
        [SerializeField] private BattleHandCardView handCardPrefab;
        [SerializeField] private Button deckButton;
        [SerializeField] private Button autoBattleButton;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private UiTextHighlightFeedback manaHighlightFeedback;
        [SerializeField] private BattleDeckPopupController deckPopup;
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float hoverDuration = 0.12f;
        [SerializeField] private float dealDuration = 0.22f;
        [SerializeField] private float dealInterval = 0.08f;
        [SerializeField] private float returnDuration = 0.18f;

        private readonly Dictionary<int, BattleHandCardView> _viewsByRuntimeId = new();
        private BattleBridge _battleBridge;
        private DefenceBattleHudViewData _viewData = new();
        private Coroutine _dealRoutine;
        private string _presentedTurnKey = string.Empty;
        private bool _listenersBound;
        private Action<HandCardViewData> _selectedCardChanged;

        public HandCardViewData SelectedCard { get; private set; }

        public void Init(BattleBridge battleBridge)
        {
            _battleBridge = battleBridge;
            deckPopup.Init();
            if (_listenersBound)
            {
                return;
            }

            autoBattleButton.onClick.AddListener(HandleAutoBattleClicked);
            BindDeckHover();
            _listenersBound = true;
        }

        public void SetBoardSelectionHandler(Action<HandCardViewData> selectedCardChanged)
        {
            _selectedCardChanged = selectedCardChanged;
        }

        public void ResetState()
        {
            if (_dealRoutine != null)
            {
                StopCoroutine(_dealRoutine);
                _dealRoutine = null;
            }

            _presentedTurnKey = string.Empty;
            SelectedCard = null;
            deckPopup.HideImmediate();
            ClearViews();
        }

        public void Refresh(DefenceBattleHudViewData viewData)
        {
            _viewData = viewData;
            manaText.text = $"{viewData.CurrentMana}/{viewData.MaxMana}";
            autoBattleButton.interactable = viewData.CanStartBattle;
            deckPopup.Refresh(viewData.DeckUnits);

            var turnKey = $"{viewData.LevelKey}:{viewData.CurrentTurn}";
            var isNewTurn = viewData.Phase == Plinko.Scripts.Data.Common.Enums.PhaseType.BattlePreparation &&
                            !string.IsNullOrWhiteSpace(viewData.LevelKey) &&
                            _presentedTurnKey != turnKey;
            if (isNewTurn)
            {
                _presentedTurnKey = turnKey;
                SelectedCard = null;
                _selectedCardChanged?.Invoke(null);
                PlayDealSequence(viewData.HandCards);
                return;
            }

            if (viewData.Phase != Plinko.Scripts.Data.Common.Enums.PhaseType.BattlePreparation)
            {
                autoBattleButton.interactable = false;
                SelectedCard = null;
                _selectedCardChanged?.Invoke(null);
                return;
            }

            if (_dealRoutine != null)
            {
                return;
            }

            SyncHand(viewData.HandCards);
            if (SelectedCard != null && (_viewData.CurrentMana < SelectedCard.ManaCost || !ContainsCard(SelectedCard.HandCardRuntimeId)))
            {
                SelectedCard = null;
                _selectedCardChanged?.Invoke(null);
            }
        }

        public bool TryDeploySelectedCard(int laneIndex, int cellIndex)
        {
            if (SelectedCard == null || !_viewData.CanDeploy)
            {
                return false;
            }

            if (_viewData.CurrentMana < SelectedCard.ManaCost)
            {
                PlayInsufficientManaFeedback(GetCardView(SelectedCard.HandCardRuntimeId));
                return false;
            }

            if (!_viewsByRuntimeId.TryGetValue(SelectedCard.HandCardRuntimeId, out var view))
            {
                SelectedCard = null;
                _selectedCardChanged?.Invoke(null);
                return false;
            }

            UiFloatingTextManager.Instance?.SpawnAtRectTransform($"-{SelectedCard.ManaCost}", Color.white, manaAnchor);
            AudioManager.Instance?.Play(GameAudioCueType.PurchaseMana);
            AudioManager.Instance?.Play(GameAudioCueType.CardDeploy);
            _battleBridge.RequestDeployCard(SelectedCard.HandCardRuntimeId, laneIndex, cellIndex);
            _viewsByRuntimeId.Remove(SelectedCard.HandCardRuntimeId);
            SelectedCard = null;
            _selectedCardChanged?.Invoke(null);

            AnimateCardToBoard(view);
            return true;
        }

        private void HandleDeckPointerEntered(BaseEventData _)
        {
            if (deckButton == null || !deckButton.interactable)
            {
                return;
            }

            deckPopup.Show();
        }

        private void HandleDeckPointerExited(BaseEventData _)
        {
            deckPopup.Hide();
        }

        private void HandleAutoBattleClicked()
        {
            SelectedCard = null;
            _selectedCardChanged?.Invoke(null);
            ReturnCardsToDeck();
            deckPopup.Hide();
            UiAnimationManager.Instance.PlaySpringPunch(autoBattleButton.transform as RectTransform);
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
            _battleBridge.RequestStartBattle();
        }

        private void PlayDealSequence(IReadOnlyList<HandCardViewData> handCards)
        {
            if (_dealRoutine != null)
            {
                StopCoroutine(_dealRoutine);
                _dealRoutine = null;
            }

            ClearViews();
            _dealRoutine = StartCoroutine(DealRoutine(handCards));
        }

        private IEnumerator DealRoutine(IReadOnlyList<HandCardViewData> handCards)
        {
            for (var index = 0; index < handCards.Count; index++)
            {
                var view = CreateCardView(handCards[index], index);
                view.RectTransform.localScale = Vector3.zero;

                Canvas.ForceUpdateCanvases();
                var targetWorldPosition = UiRectTransformUtility.GetWorldCenter(view.RectTransform);
                view.LayoutElement.ignoreLayout = true;
                view.RectTransform.SetParent(animationLayerRoot, false);
                view.RectTransform.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, deckAnchor.position);

                var targetAnchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, targetWorldPosition);
                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    "deal",
                    targetAnchoredPosition,
                    Vector3.one,
                    dealDuration,
                    Ease.OutCubic,
                    Ease.OutBack,
                    () => ReturnCardToHand(view, index));
                AudioManager.Instance?.Play(GameAudioCueType.CardAppear);

                yield return new WaitForSecondsRealtime(dealInterval);
            }

            _dealRoutine = null;
        }

        private void SyncHand(IReadOnlyList<HandCardViewData> handCards)
        {
            var activeRuntimeIds = new HashSet<int>();
            for (var index = 0; index < handCards.Count; index++)
            {
                var card = handCards[index];
                activeRuntimeIds.Add(card.HandCardRuntimeId);
                if (!_viewsByRuntimeId.TryGetValue(card.HandCardRuntimeId, out var view))
                {
                    view = CreateCardView(card, index);
                }

                view.Refresh(card);
                view.SetSelected(SelectedCard != null && SelectedCard.HandCardRuntimeId == card.HandCardRuntimeId);
                view.transform.SetSiblingIndex(index);
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
                Destroy(_viewsByRuntimeId[staleRuntimeIds[index]].gameObject);
                _viewsByRuntimeId.Remove(staleRuntimeIds[index]);
            }
        }

        private BattleHandCardView CreateCardView(HandCardViewData viewData, int siblingIndex)
        {
            var view = Instantiate(handCardPrefab, handRoot);
            view.Bind(HandlePointerEntered, HandlePointerExited, HandleBeginDragIgnored, HandleDragIgnored, HandleEndDragIgnored, HandleCardClicked);
            view.Refresh(viewData);
            view.transform.SetSiblingIndex(siblingIndex);
            _viewsByRuntimeId[viewData.HandCardRuntimeId] = view;
            return view;
        }

        private void HandlePointerEntered(BattleHandCardView view)
        {
            if (SelectedCard != null && SelectedCard.HandCardRuntimeId == view.ViewData.HandCardRuntimeId)
            {
                return;
            }

            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one * hoverScale, hoverDuration, Ease.OutQuad);
        }

        private void HandlePointerExited(BattleHandCardView view)
        {
            if (SelectedCard != null && SelectedCard.HandCardRuntimeId == view.ViewData.HandCardRuntimeId)
            {
                return;
            }

            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one, hoverDuration, Ease.OutQuad);
        }

        private void HandleCardClicked(BattleHandCardView view)
        {
            if (!_viewData.CanDeploy)
            {
                return;
            }

            if (_viewData.CurrentMana < view.ViewData.ManaCost)
            {
                PlayInsufficientManaFeedback(view);
                return;
            }

            SelectedCard = SelectedCard != null && SelectedCard.HandCardRuntimeId == view.ViewData.HandCardRuntimeId
                ? null
                : view.ViewData;
            SyncSelectionVisuals();
            _selectedCardChanged?.Invoke(SelectedCard);
        }

        private void PlayInsufficientManaFeedback(BattleHandCardView view)
        {
            if (view != null)
            {
                UiAnimationManager.Instance.PlayShake(view.RectTransform);
            }

            manaHighlightFeedback.Play();
        }

        private void SyncSelectionVisuals()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                var isSelected = SelectedCard != null && SelectedCard.HandCardRuntimeId == pair.Key;
                pair.Value.SetSelected(isSelected);
                UiAnimationManager.Instance.PlayScaleTo(
                    pair.Value.RectTransform,
                    "hover",
                    isSelected ? Vector3.one * hoverScale : Vector3.one,
                    hoverDuration,
                    Ease.OutQuad);
            }
        }

        private void AnimateCardToBoard(BattleHandCardView view)
        {
            view.LayoutElement.ignoreLayout = true;
            view.RectTransform.SetParent(animationLayerRoot, false);
            UiAnimationManager.Instance.PlayMoveAndScale(
                view.RectTransform,
                "deploy-out",
                UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, deckAnchor.position),
                Vector3.zero,
                returnDuration,
                Ease.InQuad,
                Ease.InBack,
                () =>
                {
                    if (view != null)
                    {
                        Destroy(view.gameObject);
                    }
                });
        }

        private void ReturnCardToHand(BattleHandCardView view, int siblingIndex)
        {
            view.RectTransform.SetParent(handRoot, false);
            view.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, handRoot.childCount));
            view.LayoutElement.ignoreLayout = false;
            view.CanvasGroup.blocksRaycasts = true;
            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one, hoverDuration, Ease.OutQuad);
        }

        private void ReturnCardsToDeck()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                var view = pair.Value;
                view.LayoutElement.ignoreLayout = true;
                view.RectTransform.SetParent(animationLayerRoot, false);
                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    "return-to-deck",
                    UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, deckAnchor.position),
                    Vector3.zero,
                    returnDuration,
                    Ease.InQuad,
                    Ease.InBack,
                    () =>
                    {
                        if (view != null)
                        {
                            Destroy(view.gameObject);
                        }
                    });
            }

            _viewsByRuntimeId.Clear();
        }

        private bool ContainsCard(int handCardRuntimeId)
        {
            return _viewsByRuntimeId.ContainsKey(handCardRuntimeId);
        }

        private BattleHandCardView GetCardView(int handCardRuntimeId)
        {
            _viewsByRuntimeId.TryGetValue(handCardRuntimeId, out var view);
            return view;
        }

        private void HandleBeginDragIgnored(BattleHandCardView view, UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        private void HandleDragIgnored(BattleHandCardView view, UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        private void HandleEndDragIgnored(BattleHandCardView view, UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        private void ClearViews()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _viewsByRuntimeId.Clear();
        }

        private void BindDeckHover()
        {
            if (deckButton == null)
            {
                return;
            }

            var trigger = deckButton.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = deckButton.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers ??= new List<EventTrigger.Entry>();
            AddTrigger(trigger, EventTriggerType.PointerEnter, HandleDeckPointerEntered);
            AddTrigger(trigger, EventTriggerType.PointerExit, HandleDeckPointerExited);
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType eventType, Action<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry
            {
                eventID = eventType
            };
            entry.callback.AddListener(data => callback.Invoke(data));
            trigger.triggers.Add(entry);
        }
    }
}
