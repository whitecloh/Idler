using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
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
        [SerializeField] private Transform projectilesRoot;
        [SerializeField] private PowerLineProjectileWorldView projectilePrefab;
        [SerializeField] private Transform plugsRoot;
        [SerializeField] private PowerLinePlugWorldView plugPrefab;
        [SerializeField] private float moveDuration = 0.18f;
        [SerializeField] private float unitStackSpacing = 0.22f;
        [SerializeField] private float laneCongestionThreshold = 0.035f;
        [SerializeField] private float exitDistance = 0.35f;
        [SerializeField] private float exitDuration = 0.22f;
        [SerializeField] private float projectileDuration = 0.12f;

        private readonly Dictionary<int, PowerLineUnitWorldView> _playerViews = new();
        private readonly Dictionary<int, PowerLineUnitWorldView> _enemyViews = new();
        private readonly Dictionary<Enums.PowerLineLane, PowerLinePlugWorldView> _plugViews = new();
        private readonly Dictionary<Enums.PowerLineLane, PowerLineLaneWorldView> _laneViewsByType = new();
        private static Sprite _fallbackProjectileSprite;
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
            if (UiFloatingTextManager.Instance != null)
            {
                UiFloatingTextManager.Instance.ConfigureWorldViewport(viewportRect, worldCamera);
            }
        }

        public void ResetState()
        {
            _selectedCard = null;
            _currentMana = 0;
            ClearUnitViews(_playerViews);
            ClearUnitViews(_enemyViews);
            ClearProjectiles();
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
            var previousNormalizedPositionByLane = new Dictionary<int, float>();
            var congestionIndexByLane = new Dictionary<int, int>();

            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);
                if (!_laneViewsByType.TryGetValue((Enums.PowerLineLane)unit.LaneIndex, out var laneView))
                {
                    continue;
                }

                var lateralOffset = 0f;
                if (previousNormalizedPositionByLane.TryGetValue(unit.LaneIndex, out var previousNormalizedPosition) &&
                    Mathf.Abs(unit.NormalizedPosition - previousNormalizedPosition) <= laneCongestionThreshold)
                {
                    congestionIndexByLane.TryGetValue(unit.LaneIndex, out var congestionIndex);
                    congestionIndex++;
                    congestionIndexByLane[unit.LaneIndex] = congestionIndex;
                    lateralOffset = GetCongestionOffset(congestionIndex);
                }
                else
                {
                    congestionIndexByLane[unit.LaneIndex] = 0;
                }

                previousNormalizedPositionByLane[unit.LaneIndex] = unit.NormalizedPosition;
                var targetPosition = laneView.GetWorldPosition(unit.NormalizedPosition, lateralOffset);

                if (!viewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(prefab, parent);
                    view.RootTransform.position = targetPosition;
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
                    AttackType = unit.AttackType,
                    IsEnemy = unit.IsEnemy,
                    PortraitSprite = unit.PortraitSprite,
                    ProjectileSprite = unit.ProjectileSprite,
                    BattleAnimations = unit.BattleAnimations
                });

                var delta = targetPosition - view.RootTransform.position;
                var isMoving = delta.sqrMagnitude > 0.0004f;
                var facingRight = Mathf.Abs(delta.x) > 0.001f ? delta.x >= 0f : !isEnemy;
                view.SetFacing(facingRight);
                view.SetMoving(isMoving);
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
            var playedPlayerAttackSound = false;
            var playedEnemyAttackSound = false;
            var playedDamageSound = false;

            for (var index = 0; index < _viewData.UnitSpawnEvents.Count; index++)
            {
                var evt = _viewData.UnitSpawnEvents[index];
                var views = evt.IsEnemy ? _enemyViews : _playerViews;
                if (views.TryGetValue(evt.RuntimeId, out var unitView))
                {
                    UiAnimationManager.Instance.PlayTransformPunch(unitView.RootTransform);
                }
            }

            for (var index = 0; index < _viewData.AttackEvents.Count; index++)
            {
                var evt = _viewData.AttackEvents[index];
                var views = evt.AttackerIsEnemy ? _enemyViews : _playerViews;
                if (views.TryGetValue(evt.AttackerRuntimeId, out var attackerView))
                {
                    var targetWorldPosition = evt.TargetIsBase
                        ? playerBaseView.RootTransform.position
                        : ResolveLaneWorldPosition(evt.LaneIndex, evt.TargetNormalizedPosition);
                    attackerView.SetFacing(targetWorldPosition.x >= attackerView.RootTransform.position.x);
                    attackerView.PlayAttack();
                }

                if (evt.AttackerIsEnemy)
                {
                    if (!playedEnemyAttackSound)
                    {
                        AudioManager.Instance?.Play(GameAudioCueType.EnemyAttack);
                        playedEnemyAttackSound = true;
                    }
                }
                else if (!playedPlayerAttackSound)
                {
                    AudioManager.Instance?.Play(GameAudioCueType.UnitAttack);
                    playedPlayerAttackSound = true;
                }

                if (evt.AttackType == Enums.AttackType.Ranged)
                {
                    SpawnProjectile(evt);
                }
            }

            for (var index = 0; index < _viewData.DamageEvents.Count; index++)
            {
                var evt = _viewData.DamageEvents[index];
                if (!playedDamageSound)
                {
                    AudioManager.Instance?.Play(GameAudioCueType.DamageTaken);
                    playedDamageSound = true;
                }

                if (evt.TargetIsBase)
                {
                    UiAnimationManager.Instance.PlayTransformPunch(playerBaseView.RootTransform);
                    if (UiFloatingTextManager.Instance != null)
                    {
                        UiFloatingTextManager.Instance.SpawnAtWorldPosition($"-{evt.Amount}", new Color(1f, 0.35f, 0.35f), playerBaseView.RootTransform.position);
                    }

                    continue;
                }

                var views = evt.TargetIsEnemy ? _enemyViews : _playerViews;
                if (!views.TryGetValue(evt.TargetRuntimeId, out var unitViewTarget))
                {
                    if (_laneViewsByType.TryGetValue((Enums.PowerLineLane)evt.LaneIndex, out var laneView))
                    {
                        var worldPosition = laneView.GetWorldPosition(evt.NormalizedPosition);
                        if (UiFloatingTextManager.Instance != null)
                        {
                            UiFloatingTextManager.Instance.SpawnAtWorldPosition($"-{evt.Amount}", new Color(1f, 0.35f, 0.35f), worldPosition);
                        }
                    }

                    continue;
                }

                unitViewTarget.PlayHit();
                UiAnimationManager.Instance.PlayTransformPunch(unitViewTarget.RootTransform);
                if (UiFloatingTextManager.Instance != null)
                {
                    UiFloatingTextManager.Instance.SpawnAtWorldPosition($"-{evt.Amount}", new Color(1f, 0.35f, 0.35f), unitViewTarget.RootTransform.position);
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

        private float GetCongestionOffset(int congestionIndex)
        {
            if (congestionIndex <= 0)
            {
                return 0f;
            }

            var magnitude = ((congestionIndex + 1) / 2) * unitStackSpacing;
            return congestionIndex % 2 == 1 ? magnitude : -magnitude;
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

        private void SpawnProjectile(PowerLineAttackEventViewData evt)
        {
            if (projectilePrefab == null || projectilesRoot == null)
            {
                return;
            }

            var startWorldPosition = ResolveLaneWorldPosition(evt.LaneIndex, evt.StartNormalizedPosition);
            var targetWorldPosition = evt.TargetIsBase
                ? ResolveLaneWorldPosition(evt.LaneIndex, 0f)
                : ResolveLaneWorldPosition(evt.LaneIndex, evt.TargetNormalizedPosition);
            var projectileView = Instantiate(projectilePrefab, projectilesRoot);
            projectileView.RootTransform.position = startWorldPosition;
            var projectileScale = projectileView.RootTransform.localScale;
            projectileView.Refresh(evt.ProjectileSprite != null ? evt.ProjectileSprite : GetFallbackProjectileSprite(), targetWorldPosition.x >= startWorldPosition.x);

            UiAnimationManager.Instance.PlayWorldMoveAndScale(
                projectileView.RootTransform,
                $"power-line-projectile-{evt.AttackerRuntimeId}-{evt.LaneIndex}",
                targetWorldPosition,
                projectileScale,
                projectileDuration,
                Ease.Linear,
                Ease.Linear,
                () =>
                {
                    if (projectileView != null)
                    {
                        Destroy(projectileView.gameObject);
                    }
                });

            if (projectileView.SpriteRenderer != null)
            {
                UiAnimationManager.Instance.PlaySpriteFade(
                    projectileView.SpriteRenderer,
                    $"power-line-projectile-fade-{evt.AttackerRuntimeId}-{evt.LaneIndex}",
                    0f,
                    projectileDuration,
                    Ease.Linear);
            }
        }

        private Vector3 ResolveLaneWorldPosition(int laneIndex, float normalizedPosition)
        {
            if (_laneViewsByType.TryGetValue((Enums.PowerLineLane)laneIndex, out var laneView))
            {
                return laneView.GetWorldPosition(normalizedPosition);
            }

            return Vector3.zero;
        }

        private static Sprite GetFallbackProjectileSprite()
        {
            if (_fallbackProjectileSprite != null)
            {
                return _fallbackProjectileSprite;
            }

            _fallbackProjectileSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            return _fallbackProjectileSprite;
        }

        private void ClearPlugViews()
        {
            foreach (var pair in _plugViews)
            {
                Destroy(pair.Value.gameObject);
            }

            _plugViews.Clear();
        }

        private void ClearProjectiles()
        {
            if (projectilesRoot == null)
            {
                return;
            }

            for (var index = projectilesRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(projectilesRoot.GetChild(index).gameObject);
            }
        }
    }
}
