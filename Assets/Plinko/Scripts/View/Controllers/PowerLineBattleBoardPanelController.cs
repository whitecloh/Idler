using System;
using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PowerLineBattleBoardPanelController : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private PowerLinePlayerBaseView playerBaseView;
        [SerializeField] private PowerLineEnemyBaseView enemyBaseView;
        [SerializeField] private PowerLineLaneView[] laneViews;
        [SerializeField] private BattleBoardUnitView playerUnitPrefab;
        [SerializeField] private BattleBoardUnitView enemyUnitPrefab;
        [SerializeField] private PowerLinePlugView plugPrefab;
        [SerializeField] private BattleTurnBannerView titleBannerView;
        [SerializeField] private float moveDuration = 0.18f;

        private readonly Dictionary<int, BattleBoardUnitView> _playerViews = new();
        private readonly Dictionary<int, BattleBoardUnitView> _enemyViews = new();
        private readonly Dictionary<int, PowerLinePlugView> _plugViews = new();
        private PowerLineBattleHudViewData _viewData = new();
        private Action<int> _laneClicked;
        private HandCardViewData _selectedCard;
        private int _currentMana;
        private string _presentedLevelKey = string.Empty;

        public void Init(Action<int> laneClicked)
        {
            _laneClicked = laneClicked;
            for (var index = 0; index < laneViews.Length; index++)
            {
                laneViews[index].Bind(HandleLaneClicked);
            }
        }

        public void ResetState()
        {
            _selectedCard = null;
            _currentMana = 0;
            _presentedLevelKey = string.Empty;
            ClearUnitViews(_playerViews);
            ClearUnitViews(_enemyViews);
            ClearPlugViews();
            titleBannerView.HideImmediate();
            RefreshLaneStates();
        }

        public void Refresh(PowerLineBattleHudViewData viewData)
        {
            _viewData = viewData;
            backgroundImage.sprite = viewData.BackgroundSprite;
            backgroundImage.enabled = viewData.BackgroundSprite != null;
            playerBaseView.Refresh(viewData.PlayerBase);
            enemyBaseView.Refresh(viewData.EnemyBaseSprite, viewData.ConnectedLaneCount, viewData.RequiredLaneCount, viewData.Lanes);

            if (_presentedLevelKey != viewData.LevelKey && !string.IsNullOrWhiteSpace(viewData.LevelKey))
            {
                _presentedLevelKey = viewData.LevelKey;
                titleBannerView.ShowText(viewData.LevelTitle);
            }

            SyncUnitViews(viewData.PlayerUnits, _playerViews, playerUnitPrefab, false);
            SyncUnitViews(viewData.EnemyUnits, _enemyViews, enemyUnitPrefab, true);
            SyncPlugViews(viewData.Lanes);
            RefreshLaneStates();
        }

        public void SetSelectedCard(HandCardViewData selectedCard, int currentMana)
        {
            _selectedCard = selectedCard;
            _currentMana = currentMana;
            RefreshLaneStates();
        }

        private void HandleLaneClicked(PowerLineLaneView laneView)
        {
            if (_selectedCard == null || _currentMana < _selectedCard.ManaCost || _viewData.IsInteractionLocked)
            {
                return;
            }

            for (var index = 0; index < laneViews.Length; index++)
            {
                if (laneViews[index] != laneView)
                {
                    continue;
                }

                _laneClicked?.Invoke(index);
                return;
            }
        }

        private void RefreshLaneStates()
        {
            for (var index = 0; index < laneViews.Length; index++)
            {
                var laneData = index < _viewData.Lanes.Count ? _viewData.Lanes[index] : null;
                var isAvailable = laneData != null &&
                                  laneData.IsSpawnAvailable &&
                                  _selectedCard != null &&
                                  _currentMana >= _selectedCard.ManaCost &&
                                  !_viewData.IsInteractionLocked;
                var isSelected = isAvailable;
                var isConnected = laneData != null && laneData.IsConnected;
                var isDisabled = laneData == null || laneData.IsConnected;
                laneViews[index].SetState(isSelected, isAvailable, isConnected, isDisabled);
            }
        }

        private void SyncUnitViews(
            IReadOnlyList<PowerLineUnitViewData> units,
            Dictionary<int, BattleBoardUnitView> viewsByRuntimeId,
            BattleBoardUnitView prefab,
            bool isEnemy)
        {
            var activeRuntimeIds = new HashSet<int>();
            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);
                if (!viewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(prefab, laneViews[unit.LaneIndex].ContentRoot);
                    viewsByRuntimeId.Add(unit.RuntimeId, view);
                }

                view.Refresh(new BattleBoardUnitViewData
                {
                    RuntimeId = unit.RuntimeId,
                    DisplayName = unit.DisplayName,
                    Attack = unit.Attack,
                    Health = unit.Health,
                    ManaCost = unit.ManaCost,
                    IsEnemy = unit.IsEnemy,
                    PortraitSprite = unit.PortraitSprite,
                    BattleAnimations = unit.BattleAnimations
                });

                if (view.transform.parent != laneViews[unit.LaneIndex].ContentRoot)
                {
                    view.RectTransform.SetParent(laneViews[unit.LaneIndex].ContentRoot, false);
                }

                var yOffset = isEnemy ? -10f : 10f;
                if (unit.IsCarryingPlug && !isEnemy)
                {
                    yOffset += 10f;
                }

                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    $"power-line-unit-{unit.RuntimeId}",
                    laneViews[unit.LaneIndex].GetAnchoredPosition(laneViews[unit.LaneIndex].ContentRoot, uiCamera, unit.NormalizedPosition, yOffset),
                    Vector3.one,
                    moveDuration,
                    DG.Tweening.Ease.Linear,
                    DG.Tweening.Ease.OutQuad);
            }

            RemoveStaleUnitViews(viewsByRuntimeId, activeRuntimeIds);
        }

        private void SyncPlugViews(IReadOnlyList<PowerLineLaneViewData> lanes)
        {
            for (var laneIndex = 0; laneIndex < laneViews.Length; laneIndex++)
            {
                var laneData = laneIndex < lanes.Count ? lanes[laneIndex] : null;
                if (laneData == null)
                {
                    if (_plugViews.TryGetValue(laneIndex, out var staleView))
                    {
                        Destroy(staleView.gameObject);
                        _plugViews.Remove(laneIndex);
                    }

                    continue;
                }

                if (!_plugViews.TryGetValue(laneIndex, out var view))
                {
                    view = Instantiate(plugPrefab, laneViews[laneIndex].ContentRoot);
                    _plugViews.Add(laneIndex, view);
                }

                view.Refresh(laneData.Plug);
                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    $"power-line-plug-{laneIndex}",
                    laneViews[laneIndex].GetAnchoredPosition(laneViews[laneIndex].ContentRoot, uiCamera, laneData.Plug.NormalizedPosition, 0f),
                    Vector3.one,
                    moveDuration,
                    DG.Tweening.Ease.Linear,
                    DG.Tweening.Ease.OutQuad);
            }
        }

        private static void RemoveStaleUnitViews(Dictionary<int, BattleBoardUnitView> viewsByRuntimeId, HashSet<int> activeRuntimeIds)
        {
            var staleIds = new List<int>();
            foreach (var pair in viewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
                {
                    staleIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleIds.Count; index++)
            {
                Destroy(viewsByRuntimeId[staleIds[index]].gameObject);
                viewsByRuntimeId.Remove(staleIds[index]);
            }
        }

        private static void ClearUnitViews(Dictionary<int, BattleBoardUnitView> views)
        {
            foreach (var pair in views)
            {
                Destroy(pair.Value.gameObject);
            }

            views.Clear();
        }

        private void ClearPlugViews()
        {
            foreach (var pair in _plugViews)
            {
                Destroy(pair.Value.gameObject);
            }

            _plugViews.Clear();
        }
    }
}
