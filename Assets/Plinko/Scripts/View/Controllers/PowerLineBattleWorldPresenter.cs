using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PowerLineBattleWorldPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private PowerLinePlayerBaseWorldView playerBaseView;
        [SerializeField] private PowerLineEnemyBaseWorldView enemyBaseView;
        [SerializeField] private PowerLineLaneWorldView[] laneViews;
        [SerializeField] private Transform playerUnitsRoot;
        [SerializeField] private PowerLineUnitWorldView playerUnitPrefab;
        [SerializeField] private Transform enemyUnitsRoot;
        [SerializeField] private PowerLineUnitWorldView enemyUnitPrefab;
        [SerializeField] private Transform plugsRoot;
        [SerializeField] private PowerLinePlugWorldView plugPrefab;
        [SerializeField] private UiFloatingTextManager floatingTextManager;
        [SerializeField] private float moveDuration = 0.18f;
        [SerializeField] private float laneSideOffset = 0.25f;
        [SerializeField] private float unitStackSpacing = 0.22f;
        [SerializeField] private float exitDistance = 0.35f;
        [SerializeField] private float exitDuration = 0.22f;

        private readonly Dictionary<int, PowerLineUnitWorldView> _playerViews = new();
        private readonly Dictionary<int, PowerLineUnitWorldView> _enemyViews = new();
        private readonly Dictionary<Enums.PowerLineLane, PowerLinePlugWorldView> _plugViews = new();
        private readonly Dictionary<Enums.PowerLineLane, PowerLineLaneWorldView> _laneViewsByType = new();
        private PowerLineBattleHudViewData _viewData = new();
        private HandCardViewData _selectedCard;
        private int _currentMana;

        public Camera WorldCamera => worldCamera;

        private void Awake()
        {
            _laneViewsByType.Clear();
            if (laneViews == null)
            {
                return;
            }

            for (var index = 0; index < laneViews.Length; index++)
            {
                var laneView = laneViews[index];
                if (laneView == null)
                {
                    continue;
                }

                _laneViewsByType[laneView.Lane] = laneView;
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
            }

            if (worldCamera != null)
            {
                worldCamera.enabled = isVisible;
            }
        }

        public void BindViewport(RectTransform viewportRect)
        {
            if (floatingTextManager != null)
            {
                floatingTextManager.ConfigureWorldViewport(viewportRect, worldCamera);
            }
        }

        public void ResetState()
        {
            _selectedCard = null;
            _currentMana = 0;
            ClearUnitViews(_playerViews);
            ClearUnitViews(_enemyViews);
            ClearPlugViews();
            RefreshLaneStates();
        }

        public void Refresh(PowerLineBattleHudViewData viewData)
        {
            _viewData = viewData ?? new PowerLineBattleHudViewData();
            playerBaseView.Refresh(_viewData.PlayerBase.Sprite);
            enemyBaseView.Refresh(_viewData.EnemyBaseSprite, _viewData.Lanes);
            SyncUnitViews(_viewData.PlayerUnits, _playerViews, playerUnitPrefab, playerUnitsRoot, false);
            SyncUnitViews(_viewData.EnemyUnits, _enemyViews, enemyUnitPrefab, enemyUnitsRoot, true);
            SyncPlugViews();
            RefreshLaneStates();
            ApplyTransientEvents();
        }

        public void SetSelectedCard(HandCardViewData selectedCard, int currentMana)
        {
            _selectedCard = selectedCard;
            _currentMana = currentMana;
            RefreshLaneStates();
        }

        public bool TryGetLaneSpawnWorldPosition(Enums.PowerLineLane lane, out Vector3 worldPosition)
        {
            if (_laneViewsByType.TryGetValue(lane, out var laneView))
            {
                worldPosition = laneView.GetSpawnWorldPosition();
                return true;
            }

            worldPosition = Vector3.zero;
            return false;
        }

        private void RefreshLaneStates()
        {
            foreach (var pair in _laneViewsByType)
            {
                var laneType = pair.Key;
                var laneView = pair.Value;
                var laneData = FindLaneData(laneType);
                var isAvailable = laneData != null &&
                                  laneData.IsSpawnAvailable &&
                                  _selectedCard != null &&
                                  _currentMana >= _selectedCard.ManaCost &&
                                  !_viewData.IsInteractionLocked;
                var isSelected = isAvailable;
                var isConnected = laneData != null && laneData.IsConnected;
                var isDisabled = laneData == null || laneData.IsConnected;
                laneView.SetState(isSelected, isAvailable, isConnected, isDisabled);
            }
        }

        private PowerLineLaneViewData FindLaneData(Enums.PowerLineLane lane)
        {
            for (var index = 0; index < _viewData.Lanes.Count; index++)
            {
                if (_viewData.Lanes[index].Lane == lane)
                {
                    return _viewData.Lanes[index];
                }
            }

            return null;
        }

        private void SyncUnitViews(
            IReadOnlyList<PowerLineUnitViewData> units,
            Dictionary<int, PowerLineUnitWorldView> viewsByRuntimeId,
            PowerLineUnitWorldView prefab,
            Transform parent,
            bool isEnemy)
        {
            var activeRuntimeIds = new HashSet<int>();
            var laneSlotCounters = new Dictionary<int, int>();

            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);
                if (!_laneViewsByType.TryGetValue((Enums.PowerLineLane)unit.LaneIndex, out var laneView))
                {
                    continue;
                }

                if (!viewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(prefab, parent);
                    viewsByRuntimeId.Add(unit.RuntimeId, view);
                }

                view.Refresh(new BattleBoardUnitViewData
                {
                    RuntimeId = unit.RuntimeId,
                    DisplayName = unit.DisplayName,
                    Attack = unit.Attack,
                    Health = unit.Health,
                    MaxHealth = unit.MaxHealth,
                    ManaCost = unit.ManaCost,
                    MoveSpeed = unit.MoveSpeed,
                    AttackRange = unit.AttackRange,
                    AttackSpeed = unit.AttackSpeed,
                    IsEnemy = unit.IsEnemy,
                    PortraitSprite = unit.PortraitSprite,
                    BattleAnimations = unit.BattleAnimations
                });

                laneSlotCounters.TryGetValue(unit.LaneIndex, out var slotIndex);
                laneSlotCounters[unit.LaneIndex] = slotIndex + 1;

                var lateralOffset = isEnemy ? -laneSideOffset : laneSideOffset;
                if (unit.IsCarryingPlug && !isEnemy)
                {
                    lateralOffset += 0.15f;
                }

                lateralOffset += GetStackOffset(slotIndex, isEnemy);
                var targetPosition = laneView.GetWorldPosition(unit.NormalizedPosition, lateralOffset);
                UiAnimationManager.Instance.PlayWorldMoveAndScale(
                    view.RootTransform,
                    $"power-line-world-unit-{unit.RuntimeId}",
                    targetPosition,
                    Vector3.one,
                    moveDuration,
                    Ease.Linear,
                    Ease.OutQuad);
            }

            RemoveStaleUnitViews(viewsByRuntimeId, activeRuntimeIds, isEnemy);
        }

        private void SyncPlugViews()
        {
            foreach (var pair in _laneViewsByType)
            {
                var laneType = pair.Key;
                var laneView = pair.Value;
                var laneData = FindLaneData(laneType);
                if (laneData == null)
                {
                    if (_plugViews.TryGetValue(laneType, out var stalePlug))
                    {
                        Destroy(stalePlug.gameObject);
                        _plugViews.Remove(laneType);
                    }

                    continue;
                }

                if (!_plugViews.TryGetValue(laneType, out var plugView))
                {
                    plugView = Instantiate(plugPrefab, plugsRoot);
                    _plugViews[laneType] = plugView;
                }

                plugView.Refresh(laneData.Plug);
                var plugPosition = laneData.Plug.Status == Models.PowerLinePlugStatus.Connected
                    ? laneView.GetPlugSocketWorldPosition()
                    : laneView.GetWorldPosition(laneData.Plug.NormalizedPosition);
                plugView.SetWorldPosition(plugPosition);
                plugView.SetWire(
                    laneView.GetWireStartWorldPosition(),
                    plugPosition,
                    laneData.Plug.Status == Models.PowerLinePlugStatus.Connected);
            }
        }

        private void ApplyTransientEvents()
        {
            for (var index = 0; index < _viewData.UnitSpawnEvents.Count; index++)
            {
                var evt = _viewData.UnitSpawnEvents[index];
                var views = evt.IsEnemy ? _enemyViews : _playerViews;
                if (views.TryGetValue(evt.RuntimeId, out var unitView))
                {
                    UiAnimationManager.Instance.PlayTransformPunch(unitView.RootTransform);
                }
            }

            for (var index = 0; index < _viewData.DamageEvents.Count; index++)
            {
                var evt = _viewData.DamageEvents[index];
                if (evt.TargetIsBase)
                {
                    UiAnimationManager.Instance.PlayTransformPunch(playerBaseView.RootTransform);
                    if (floatingTextManager != null)
                    {
                        floatingTextManager.SpawnAtWorldPosition($"-{evt.Amount}", new Color(1f, 0.35f, 0.35f), playerBaseView.RootTransform.position);
                    }

                    continue;
                }

                var views = evt.TargetIsEnemy ? _enemyViews : _playerViews;
                if (!views.TryGetValue(evt.TargetRuntimeId, out var unitViewTarget))
                {
                    if (_laneViewsByType.TryGetValue((Enums.PowerLineLane)evt.LaneIndex, out var laneView))
                    {
                        var worldPosition = laneView.GetWorldPosition(evt.NormalizedPosition);
                        if (floatingTextManager != null)
                        {
                            floatingTextManager.SpawnAtWorldPosition($"-{evt.Amount}", new Color(1f, 0.35f, 0.35f), worldPosition);
                        }
                    }

                    continue;
                }

                UiAnimationManager.Instance.PlayTransformPunch(unitViewTarget.RootTransform);
                if (floatingTextManager != null)
                {
                    floatingTextManager.SpawnAtWorldPosition($"-{evt.Amount}", new Color(1f, 0.35f, 0.35f), unitViewTarget.RootTransform.position);
                }
            }

            for (var index = 0; index < _viewData.PlugEvents.Count; index++)
            {
                var laneType = (Enums.PowerLineLane)_viewData.PlugEvents[index].LaneIndex;
                if (_plugViews.TryGetValue(laneType, out var plugView))
                {
                    UiAnimationManager.Instance.PlayTransformPunch(plugView.RootTransform);
                }
            }

            for (var index = 0; index < _viewData.LaneConnectedEvents.Count; index++)
            {
                var laneType = (Enums.PowerLineLane)_viewData.LaneConnectedEvents[index].LaneIndex;
                if (_laneViewsByType.TryGetValue(laneType, out var laneView))
                {
                    UiAnimationManager.Instance.PlayTransformPunch(laneView.RootTransform);
                }

                UiAnimationManager.Instance.PlayTransformPunch(enemyBaseView.RootTransform);
            }
        }

        private float GetStackOffset(int slotIndex, bool isEnemy)
        {
            if (slotIndex <= 0)
            {
                return 0f;
            }

            return (isEnemy ? -1f : 1f) * slotIndex * unitStackSpacing;
        }

        private void RemoveStaleUnitViews(Dictionary<int, PowerLineUnitWorldView> viewsByRuntimeId, HashSet<int> activeRuntimeIds, bool isEnemy)
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
                var view = viewsByRuntimeId[staleIds[index]];
                viewsByRuntimeId.Remove(staleIds[index]);
                AnimateAndDestroyUnitView(view, isEnemy);
            }
        }

        private void AnimateAndDestroyUnitView(PowerLineUnitWorldView view, bool isEnemy)
        {
            if (view == null)
            {
                return;
            }

            var endPosition = view.RootTransform.position + Vector3.up * (isEnemy ? -exitDistance : exitDistance);
            UiAnimationManager.Instance.PlayWorldMoveAndScale(
                view.RootTransform,
                $"power-line-world-unit-exit-{view.RuntimeId}",
                endPosition,
                Vector3.one * 0.85f,
                exitDuration,
                Ease.OutQuad,
                Ease.OutQuad);

            if (view.PrimaryRenderer != null)
            {
                UiAnimationManager.Instance.PlaySpriteFade(
                    view.PrimaryRenderer,
                    $"power-line-world-unit-fade-{view.RuntimeId}",
                    0f,
                    exitDuration,
                    Ease.OutQuad,
                    () =>
                    {
                        if (view != null)
                        {
                            Destroy(view.gameObject);
                        }
                    });
                return;
            }

            Destroy(view.gameObject, exitDuration);
        }

        private static void ClearUnitViews(Dictionary<int, PowerLineUnitWorldView> views)
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
