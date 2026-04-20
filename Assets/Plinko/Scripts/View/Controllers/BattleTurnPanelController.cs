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
    public sealed class BattleTurnPanelController : MonoBehaviour
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
        private readonly Dictionary<int, int> _dragSiblingByRuntimeId = new();
        private BattleBridge _battleBridge;
        private BattleBoardPanelController _boardPanel;
        private StandardBattleHudViewData _viewData = new();
        private BattleHandCardView _draggedView;
        private string _presentedTurnKey = string.Empty;
        private Coroutine _dealRoutine;
        private bool _listenersBound;

        public void Init(BattleBridge battleBridge, BattleBoardPanelController boardPanel)
        {
            _battleBridge = battleBridge;
            _boardPanel = boardPanel;
            deckPopup.Init();
            if (_listenersBound)
            {
                return;
            }

            deckButton.onClick.AddListener(HandleDeckClicked);
            autoBattleButton.onClick.AddListener(HandleAutoBattleClicked);
            _listenersBound = true;
        }

        public void ResetState()
        {
            if (_dealRoutine != null)
            {
                StopCoroutine(_dealRoutine);
                _dealRoutine = null;
            }

            _presentedTurnKey = string.Empty;
            _draggedView = null;
            deckPopup.HideImmediate();
            ClearViews();
            _dragSiblingByRuntimeId.Clear();
        }

        public void Refresh(StandardBattleHudViewData viewData)
        {
            _viewData = viewData;
            manaText.text = $"{viewData.CurrentMana}/{viewData.MaxMana}";
            autoBattleButton.interactable = viewData.CanStartBattle;
            deckPopup.Refresh(viewData.DeckUnits);

            var turnKey = $"{viewData.LevelKey}:{viewData.CurrentTurn}";
            var isNewTurn = viewData.Phase == Data.Common.Enums.PhaseType.BattlePreparation &&
                            !string.IsNullOrWhiteSpace(viewData.LevelKey) &&
                            _presentedTurnKey != turnKey;
            if (isNewTurn)
            {
                _presentedTurnKey = turnKey;
                PlayDealSequence(viewData.HandCards);
                return;
            }

            if (viewData.Phase != Data.Common.Enums.PhaseType.BattlePreparation)
            {
                autoBattleButton.interactable = false;
                return;
            }

            if (_dealRoutine != null)
            {
                return;
            }

            SyncHand(viewData.HandCards);
        }

        private void HandleDeckClicked()
        {
            UiAnimationManager.Instance.PlaySpringPunch(deckButton.transform as RectTransform);
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
            deckPopup.Toggle();
        }

        private void HandleAutoBattleClicked()
        {
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
                _dragSiblingByRuntimeId.Remove(staleRuntimeIds[index]);
                Destroy(_viewsByRuntimeId[staleRuntimeIds[index]].gameObject);
                _viewsByRuntimeId.Remove(staleRuntimeIds[index]);
            }
        }

        private BattleHandCardView CreateCardView(HandCardViewData viewData, int siblingIndex)
        {
            var view = Instantiate(handCardPrefab, handRoot);
            view.Bind(HandlePointerEntered, HandlePointerExited, HandleBeginDrag, HandleDrag, HandleEndDrag);
            view.Refresh(viewData);
            view.transform.SetSiblingIndex(siblingIndex);
            _viewsByRuntimeId[viewData.HandCardRuntimeId] = view;
            return view;
        }

        private void HandlePointerEntered(BattleHandCardView view)
        {
            if (_draggedView == view)
            {
                return;
            }

            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one * hoverScale, hoverDuration, Ease.OutQuad);
        }

        private void HandlePointerExited(BattleHandCardView view)
        {
            if (_draggedView == view)
            {
                return;
            }

            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one, hoverDuration, Ease.OutQuad);
        }

        private void HandleBeginDrag(BattleHandCardView view, PointerEventData eventData)
        {
            if (!_viewData.CanDeploy || _viewData.CurrentMana < view.ViewData.ManaCost)
            {
                UiAnimationManager.Instance.PlayShake(view.RectTransform);
                if (_viewData.CurrentMana < view.ViewData.ManaCost)
                {
                    manaHighlightFeedback.Play();
                }

                return;
            }

            _draggedView = view;
            _dragSiblingByRuntimeId[view.ViewData.HandCardRuntimeId] = view.transform.GetSiblingIndex();
            view.LayoutElement.ignoreLayout = true;
            view.CanvasGroup.blocksRaycasts = false;
            view.RectTransform.SetParent(animationLayerRoot, false);
            UpdateDraggedViewPosition(eventData.position, eventData.pressEventCamera);
        }

        private void HandleDrag(BattleHandCardView view, PointerEventData eventData)
        {
            if (_draggedView != view)
            {
                return;
            }

            UpdateDraggedViewPosition(eventData.position, eventData.pressEventCamera);
        }

        private void HandleEndDrag(BattleHandCardView view, PointerEventData eventData)
        {
            if (_draggedView != view)
            {
                return;
            }

            _draggedView = null;
            var eventCamera = eventData.pressEventCamera != null ? eventData.pressEventCamera : eventData.enterEventCamera;
            if (_boardPanel.IsScreenPointOverDropArea(eventData.position, eventCamera) &&
                _viewData.CanDeploy &&
                _viewData.CurrentMana >= view.ViewData.ManaCost)
            {
                UiFloatingTextManager.Instance?.SpawnAtRectTransform($"-{view.ViewData.ManaCost}", Color.white, manaAnchor);
                AudioManager.Instance?.Play(GameAudioCueType.PurchaseMana);
                AudioManager.Instance?.Play(GameAudioCueType.CardDeploy);
                _battleBridge.RequestDeployCard(view.ViewData.HandCardRuntimeId);
                _dragSiblingByRuntimeId.Remove(view.ViewData.HandCardRuntimeId);
                _viewsByRuntimeId.Remove(view.ViewData.HandCardRuntimeId);
                Destroy(view.gameObject);
                return;
            }

            var siblingIndex = _dragSiblingByRuntimeId.TryGetValue(view.ViewData.HandCardRuntimeId, out var storedSiblingIndex)
                ? storedSiblingIndex
                : handRoot.childCount;
            _dragSiblingByRuntimeId.Remove(view.ViewData.HandCardRuntimeId);
            ReturnCardToHand(view, siblingIndex);
        }

        private void UpdateDraggedViewPosition(Vector2 screenPosition, Camera eventCamera)
        {
            _draggedView.RectTransform.anchoredPosition = UiRectTransformUtility.ScreenToAnchoredPosition(
                animationLayerRoot,
                screenPosition,
                eventCamera);
            _draggedView.RectTransform.localScale = Vector3.one * hoverScale;
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

        private void ClearViews()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _viewsByRuntimeId.Clear();
            _dragSiblingByRuntimeId.Clear();
        }
    }
}
