using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using Plinko.Scripts.View.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PowerLineBattleTurnPanelController : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private RectTransform animationLayerRoot;
        [SerializeField] private RectTransform deckAnchor;
        [SerializeField] private RectTransform manaAnchor;
        [SerializeField] private RectTransform handRoot;
        [SerializeField] private BattleHandCardView handCardPrefab;
        [SerializeField] private Button deckButton;
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollCostText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private UiTextHighlightFeedback manaHighlightFeedback;
        [SerializeField] private BattleDeckPopupController deckPopup;
        [SerializeField] private GameObject selectedEnemyCardRoot;
        [SerializeField] private UiTooltipUnitCardView selectedEnemyCardView;
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float hoverDuration = 0.12f;
        [SerializeField] private float dealDuration = 0.22f;
        [SerializeField] private float returnDuration = 0.18f;

        private readonly Dictionary<int, BattleHandCardView> _viewsByRuntimeId = new();
        private BattleBridge _battleBridge;
        private PowerLineBattleHudViewData _viewData = new();
        private System.Action<HandCardViewData> _selectedCardChanged;
        private bool _listenersBound;

        public HandCardViewData SelectedCard { get; private set; }

        public void Init(BattleBridge battleBridge)
        {
            _battleBridge = battleBridge;
            deckPopup.Init();
            if (_listenersBound)
            {
                return;
            }

            rerollButton.onClick.AddListener(HandleRerollClicked);
            BindDeckHover();
            _listenersBound = true;
        }

        public void SetLaneSelectionHandler(System.Action<HandCardViewData> selectedCardChanged)
        {
            _selectedCardChanged = selectedCardChanged;
        }

        public void ResetState()
        {
            SelectedCard = null;
            SetSelectedEnemy(null);
            deckPopup.HideImmediate();
            ClearViews();
        }

        public void Refresh(PowerLineBattleHudViewData viewData)
        {
            _viewData = viewData;
            manaText.text = $"{viewData.CurrentMana}/{viewData.MaxMana}";
            rerollCostText.text = $"{viewData.RerollManaCost}";
            rerollButton.interactable = viewData.CanReroll && !viewData.IsInteractionLocked;
            deckButton.interactable = viewData.RemainingDeckCount > 0 && !viewData.IsInteractionLocked;
            if (deckCountText != null)
            {
                deckCountText.text = viewData.RemainingDeckCount.ToString();
            }
            deckPopup.Refresh(viewData.DeckUnits);
            if (viewData.RemainingDeckCount <= 0)
            {
                deckPopup.HideImmediate();
            }

            SyncHand(viewData.HandCards);
            SyncAvailability();
        }

        public bool TryDeploySelectedCard(int laneIndex)
        {
            if (SelectedCard == null || _viewData.IsInteractionLocked)
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
            _battleBridge.RequestDeployCard(SelectedCard.HandCardRuntimeId, laneIndex, 0);
            _viewsByRuntimeId.Remove(SelectedCard.HandCardRuntimeId);
            SelectedCard = null;
            _selectedCardChanged?.Invoke(null);
            AnimateCardToDeck(view);
            return true;
        }

        private void HandleRerollClicked()
        {
            if (_viewData.CurrentMana < _viewData.RerollManaCost)
            {
                manaHighlightFeedback.Play();
                return;
            }

            SelectedCard = null;
            _selectedCardChanged?.Invoke(null);
            UiFloatingTextManager.Instance?.SpawnAtRectTransform($"-{_viewData.RerollManaCost}", Color.white, manaAnchor);
            ReturnCardsToDeck();
            deckPopup.Hide();
            UiAnimationManager.Instance.PlaySpringPunch(rerollButton.transform as RectTransform);
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
            _battleBridge.RequestRerollPowerLineHand();
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
                    AnimateCardFromDeck(view, index);
                }
                else
                {
                    view.Refresh(card);
                    view.transform.SetSiblingIndex(index);
                }
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

            if (SelectedCard != null && !activeRuntimeIds.Contains(SelectedCard.HandCardRuntimeId))
            {
                SelectedCard = null;
                _selectedCardChanged?.Invoke(null);
            }
        }

        private void SyncAvailability()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                var isSelected = SelectedCard != null && SelectedCard.HandCardRuntimeId == pair.Key;
                var isAffordable = _viewData.CurrentMana >= pair.Value.ViewData.ManaCost;
                pair.Value.SetSelected(isSelected);
                pair.Value.SetDimmed(!isAffordable);
                UiAnimationManager.Instance.PlayScaleTo(
                    pair.Value.RectTransform,
                    "hover",
                    isSelected ? Vector3.one * hoverScale : Vector3.one,
                    hoverDuration,
                    DG.Tweening.Ease.OutQuad);
            }
        }

        private BattleHandCardView CreateCardView(HandCardViewData viewData, int siblingIndex)
        {
            var view = Instantiate(handCardPrefab, handRoot);
            view.Bind(HandlePointerEntered, HandlePointerExited, HandleIgnoredDrag, HandleIgnoredDrag, HandleIgnoredDrag, HandleCardClicked);
            view.Refresh(viewData);
            view.transform.SetSiblingIndex(siblingIndex);
            _viewsByRuntimeId[viewData.HandCardRuntimeId] = view;
            return view;
        }

        private void AnimateCardFromDeck(BattleHandCardView view, int siblingIndex)
        {
            view.RectTransform.localScale = Vector3.zero;
            Canvas.ForceUpdateCanvases();
            var targetWorldPosition = UiRectTransformUtility.GetWorldCenter(view.RectTransform);
            view.LayoutElement.ignoreLayout = true;
            view.RectTransform.SetParent(animationLayerRoot, false);
            view.RectTransform.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, deckAnchor.position);
            var targetAnchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, targetWorldPosition);

            UiAnimationManager.Instance.PlayMoveAndScale(
                view.RectTransform,
                $"power-line-deal-{view.ViewData.HandCardRuntimeId}",
                targetAnchoredPosition,
                Vector3.one,
                dealDuration,
                DG.Tweening.Ease.OutCubic,
                DG.Tweening.Ease.OutBack,
                () => ReturnCardToHand(view, siblingIndex));
            AudioManager.Instance?.Play(GameAudioCueType.CardAppear);
        }

        private void AnimateCardToDeck(BattleHandCardView view)
        {
            view.LayoutElement.ignoreLayout = true;
            view.RectTransform.SetParent(animationLayerRoot, false);
            UiAnimationManager.Instance.PlayMoveAndScale(
                view.RectTransform,
                $"power-line-return-{view.ViewData.HandCardRuntimeId}",
                UiRectTransformUtility.WorldToAnchoredPosition(animationLayerRoot, uiCamera, deckAnchor.position),
                Vector3.zero,
                returnDuration,
                DG.Tweening.Ease.InQuad,
                DG.Tweening.Ease.InBack,
                () =>
                {
                    if (view != null)
                    {
                        Destroy(view.gameObject);
                    }
                });
        }

        private void ReturnCardsToDeck()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                AnimateCardToDeck(pair.Value);
            }

            _viewsByRuntimeId.Clear();
        }

        private void ReturnCardToHand(BattleHandCardView view, int siblingIndex)
        {
            view.RectTransform.SetParent(handRoot, false);
            view.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, handRoot.childCount));
            view.LayoutElement.ignoreLayout = false;
            view.CanvasGroup.blocksRaycasts = true;
            view.SetDimmed(_viewData.CurrentMana < view.ViewData.ManaCost);
            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one, hoverDuration, DG.Tweening.Ease.OutQuad);
        }

        private void HandlePointerEntered(BattleHandCardView view)
        {
            if (SelectedCard != null && SelectedCard.HandCardRuntimeId == view.ViewData.HandCardRuntimeId)
            {
                return;
            }

            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one * hoverScale, hoverDuration, DG.Tweening.Ease.OutQuad);
        }

        private void HandlePointerExited(BattleHandCardView view)
        {
            if (SelectedCard != null && SelectedCard.HandCardRuntimeId == view.ViewData.HandCardRuntimeId)
            {
                return;
            }

            UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "hover", Vector3.one, hoverDuration, DG.Tweening.Ease.OutQuad);
        }

        private void HandleCardClicked(BattleHandCardView view)
        {
            if (_viewData.CurrentMana < view.ViewData.ManaCost)
            {
                PlayInsufficientManaFeedback(view);
                return;
            }

            SelectedCard = SelectedCard != null && SelectedCard.HandCardRuntimeId == view.ViewData.HandCardRuntimeId
                ? null
                : view.ViewData;
            SyncAvailability();
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

        private BattleHandCardView GetCardView(int handCardRuntimeId)
        {
            _viewsByRuntimeId.TryGetValue(handCardRuntimeId, out var view);
            return view;
        }

        private void HandleIgnoredDrag(BattleHandCardView view, UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        public void SetSelectedEnemy(PowerLineUnitViewData enemyViewData)
        {
            var hasEnemy = selectedEnemyCardRoot != null && selectedEnemyCardView != null && enemyViewData != null;
            if (selectedEnemyCardRoot != null)
            {
                selectedEnemyCardRoot.SetActive(hasEnemy);
            }

            if (!hasEnemy)
            {
                return;
            }

            selectedEnemyCardView.Refresh(UnitTooltipViewDataFactory.FromPowerLineEnemy(enemyViewData));
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

            trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
            AddTrigger(trigger, EventTriggerType.PointerEnter, HandleDeckPointerEntered);
            AddTrigger(trigger, EventTriggerType.PointerExit, HandleDeckPointerExited);
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry
            {
                eventID = eventType
            };
            entry.callback.AddListener(data => callback.Invoke(data));
            trigger.triggers.Add(entry);
        }

        private void ClearViews()
        {
            foreach (var pair in _viewsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _viewsByRuntimeId.Clear();
        }
    }
}
