using System.Collections.Generic;
using System;
using DG.Tweening;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [SerializeField] private float victoryMergeDuration = 0.65f;
        [SerializeField] private float victoryPostMergeDelay = 0.7f;
        [SerializeField] private float victoryMergeContactPadding = 0f;
        [SerializeField] private float enemySelectionPadding = 0.5f;

        private readonly Dictionary<int, PowerLineUnitWorldView> _playerViews = new();
        private readonly Dictionary<int, PowerLineUnitWorldView> _enemyViews = new();
        private readonly Dictionary<Enums.PowerLineLane, PowerLinePlugWorldView> _plugViews = new();
        private readonly Dictionary<Enums.PowerLineLane, PowerLineLaneWorldView> _laneViewsByType = new();
        private static Sprite _fallbackProjectileSprite;
        private PowerLineBattleHudViewData _viewData = new();
        private HandCardViewData _selectedCard;
        private int _currentMana;
        private int _selectedEnemyRuntimeId = -1;
        private Vector3 _playerBaseInitialPosition;
        private Vector3 _enemyBaseInitialPosition;
        private Vector3 _playerBaseInitialScale = Vector3.one;
        private Vector3 _enemyBaseInitialScale = Vector3.one;
        private Vector2 _playerBaseInitialRendererSize = Vector2.one;
        private Vector2 _enemyBaseInitialRendererSize = Vector2.one;
        private SpriteDrawMode _playerBaseInitialDrawMode = SpriteDrawMode.Simple;
        private SpriteDrawMode _enemyBaseInitialDrawMode = SpriteDrawMode.Simple;
        private bool _baseTransformsCaptured;
        private RectTransform _viewportRect;
        private Action<PowerLineUnitViewData> _enemySelectionChanged;

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

            CaptureBaseTransforms();
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
            _viewportRect = viewportRect;
            if (UiFloatingTextManager.Instance != null)
            {
                UiFloatingTextManager.Instance.ConfigureWorldViewport(viewportRect, worldCamera);
            }
        }

        public void SetEnemySelectionHandler(Action<PowerLineUnitViewData> enemySelectionChanged)
        {
            _enemySelectionChanged = enemySelectionChanged;
            RefreshSelectedEnemy();
        }

        public void ResetState()
        {
            _selectedCard = null;
            _currentMana = 0;
            _selectedEnemyRuntimeId = -1;
            StopBaseTweens();
            RestoreBaseTransforms();
            ClearUnitViews(_playerViews);
            ClearUnitViews(_enemyViews);
            ClearProjectiles();
            ClearPlugViews();
            RefreshLaneStates();
            _enemySelectionChanged?.Invoke(null);
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
            RefreshSelectedEnemy();
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

        public bool TrySelectEnemyAtScreenPoint(Vector2 screenPosition)
        {
            if (_viewportRect == null || worldCamera == null)
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, screenPosition, null, out var localPoint))
            {
                return false;
            }

            var rect = _viewportRect.rect;
            var viewportX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            var viewportY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
            if (viewportX < 0f || viewportX > 1f || viewportY < 0f || viewportY > 1f)
            {
                return false;
            }

            var worldPoint = worldCamera.ViewportToWorldPoint(new Vector3(
                viewportX,
                viewportY,
                Mathf.Abs(worldCamera.transform.position.z)));
            worldPoint.z = 0f;

            if (!TryFindEnemyAtWorldPoint(worldPoint, out var enemyViewData) &&
                !TryFindNearestEnemyAtWorldPoint(worldPoint, out enemyViewData))
            {
                return false;
            }

            _selectedEnemyRuntimeId = enemyViewData.RuntimeId;
            _enemySelectionChanged?.Invoke(enemyViewData);
            return true;
        }

        public void PlayVictorySequence(Action onMerge, Action onComplete)
        {
            CaptureBaseTransforms();
            if (playerBaseView == null || enemyBaseView == null)
            {
                onMerge?.Invoke();
                onComplete?.Invoke();
                return;
            }

            var playerTransform = playerBaseView.RootTransform;
            var enemyTransform = enemyBaseView.RootTransform;
            StopBaseTweens();
            playerTransform.position = _playerBaseInitialPosition;
            playerTransform.localScale = _playerBaseInitialScale;
            enemyTransform.position = _enemyBaseInitialPosition;
            enemyTransform.localScale = _enemyBaseInitialScale;
            RestoreBaseRendererState();
            var contactX = (_playerBaseInitialPosition.x + _enemyBaseInitialPosition.x) * 0.5f;
            var playerHalfWidth = GetBaseHalfWidth(playerBaseView);
            var enemyHalfWidth = GetBaseHalfWidth(enemyBaseView);
            var playerTargetPosition = new Vector3(
                contactX - playerHalfWidth - victoryMergeContactPadding,
                playerTransform.position.y,
                playerTransform.position.z);
            var enemyTargetPosition = new Vector3(
                contactX + enemyHalfWidth + victoryMergeContactPadding,
                enemyTransform.position.y,
                enemyTransform.position.z);
            var completedMoves = 0;

            void HandleBaseMerged()
            {
                completedMoves++;
                if (completedMoves != 2)
                {
                    return;
                }

                onMerge?.Invoke();
                DOVirtual.DelayedCall(victoryPostMergeDelay, () => onComplete?.Invoke(), false);
            }

            UiAnimationManager.Instance.PlayWorldMove(
                playerTransform,
                "power-line-victory-player-base",
                playerTargetPosition,
                victoryMergeDuration,
                Ease.InQuad,
                HandleBaseMerged);
            UiAnimationManager.Instance.PlayWorldMove(
                enemyTransform,
                "power-line-victory-enemy-base",
                enemyTargetPosition,
                victoryMergeDuration,
                Ease.InQuad,
                HandleBaseMerged);
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
                    UiAnimationManager.Instance.PlayTransformShake(playerBaseView.RootTransform, 0.2f);
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

                UiAnimationManager.Instance.PlayTransformShake(enemyBaseView.RootTransform, 0.2f);
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

        private void CaptureBaseTransforms()
        {
            if (_baseTransformsCaptured)
            {
                return;
            }

            if (playerBaseView != null)
            {
                _playerBaseInitialPosition = playerBaseView.RootTransform.position;
                _playerBaseInitialScale = playerBaseView.RootTransform.localScale;
                if (playerBaseView.SpriteRenderer != null)
                {
                    _playerBaseInitialDrawMode = playerBaseView.SpriteRenderer.drawMode;
                    _playerBaseInitialRendererSize = playerBaseView.SpriteRenderer.size;
                }
            }

            if (enemyBaseView != null)
            {
                _enemyBaseInitialPosition = enemyBaseView.RootTransform.position;
                _enemyBaseInitialScale = enemyBaseView.RootTransform.localScale;
                if (enemyBaseView.SpriteRenderer != null)
                {
                    _enemyBaseInitialDrawMode = enemyBaseView.SpriteRenderer.drawMode;
                    _enemyBaseInitialRendererSize = enemyBaseView.SpriteRenderer.size;
                }
            }

            _baseTransformsCaptured = true;
        }

        private void RestoreBaseTransforms()
        {
            CaptureBaseTransforms();
            if (playerBaseView != null)
            {
                playerBaseView.RootTransform.position = _playerBaseInitialPosition;
                playerBaseView.RootTransform.localScale = _playerBaseInitialScale;
            }

            if (enemyBaseView != null)
            {
                enemyBaseView.RootTransform.position = _enemyBaseInitialPosition;
                enemyBaseView.RootTransform.localScale = _enemyBaseInitialScale;
            }

            RestoreBaseRendererState();
        }

        private void StopBaseTweens()
        {
            if (UiAnimationManager.Instance == null)
            {
                return;
            }

            if (playerBaseView != null)
            {
                UiAnimationManager.Instance.StopFeedback(playerBaseView.RootTransform);
                UiAnimationManager.Instance.Stop(playerBaseView.RootTransform, "power-line-victory-player-base");
            }

            if (enemyBaseView != null)
            {
                UiAnimationManager.Instance.StopFeedback(enemyBaseView.RootTransform);
                UiAnimationManager.Instance.Stop(enemyBaseView.RootTransform, "power-line-victory-enemy-base");
            }
        }

        private void RestoreBaseRendererState()
        {
            if (playerBaseView != null && playerBaseView.SpriteRenderer != null)
            {
                playerBaseView.SpriteRenderer.drawMode = _playerBaseInitialDrawMode;
                playerBaseView.SpriteRenderer.size = _playerBaseInitialRendererSize;
            }

            if (enemyBaseView != null && enemyBaseView.SpriteRenderer != null)
            {
                enemyBaseView.SpriteRenderer.drawMode = _enemyBaseInitialDrawMode;
                enemyBaseView.SpriteRenderer.size = _enemyBaseInitialRendererSize;
            }
        }

        private static float GetBaseHalfWidth(PowerLinePlayerBaseWorldView baseView)
        {
            if (baseView == null || baseView.SpriteRenderer == null)
            {
                return 0f;
            }

            return baseView.SpriteRenderer.bounds.extents.x;
        }

        private static float GetBaseHalfWidth(PowerLineEnemyBaseWorldView baseView)
        {
            if (baseView == null || baseView.SpriteRenderer == null)
            {
                return 0f;
            }

            return baseView.SpriteRenderer.bounds.extents.x;
        }

        private bool TryFindEnemyAtWorldPoint(Vector3 worldPoint, out PowerLineUnitViewData enemyViewData)
        {
            var bestDistance = float.MaxValue;
            enemyViewData = null;

            for (var index = 0; index < _viewData.EnemyUnits.Count; index++)
            {
                var candidate = _viewData.EnemyUnits[index];
                if (!_enemyViews.TryGetValue(candidate.RuntimeId, out var enemyView) || enemyView.PrimaryRenderer == null)
                {
                    continue;
                }

                var bounds = enemyView.PrimaryRenderer.bounds;
                bounds.Expand(new Vector3(enemySelectionPadding, enemySelectionPadding, 0f));
                if (!bounds.Contains(worldPoint))
                {
                    continue;
                }

                var distance = (enemyView.RootTransform.position - worldPoint).sqrMagnitude;
                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                enemyViewData = candidate;
            }

            return enemyViewData != null;
        }

        private bool TryFindNearestEnemyAtWorldPoint(Vector3 worldPoint, out PowerLineUnitViewData enemyViewData)
        {
            var bestDistance = float.MaxValue;
            enemyViewData = null;

            for (var index = 0; index < _viewData.EnemyUnits.Count; index++)
            {
                var candidate = _viewData.EnemyUnits[index];
                if (!_enemyViews.TryGetValue(candidate.RuntimeId, out var enemyView) || enemyView.PrimaryRenderer == null)
                {
                    continue;
                }

                var bounds = enemyView.PrimaryRenderer.bounds;
                bounds.Expand(new Vector3(enemySelectionPadding, enemySelectionPadding, 0f));
                var closestPoint = bounds.ClosestPoint(worldPoint);
                var distance = (closestPoint - worldPoint).sqrMagnitude;
                if (distance > enemySelectionPadding * enemySelectionPadding || distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                enemyViewData = candidate;
            }

            return enemyViewData != null;
        }

        private void RefreshSelectedEnemy()
        {
            if (_enemySelectionChanged == null)
            {
                return;
            }

            if (_selectedEnemyRuntimeId < 0)
            {
                _enemySelectionChanged.Invoke(null);
                return;
            }

            for (var index = 0; index < _viewData.EnemyUnits.Count; index++)
            {
                var enemy = _viewData.EnemyUnits[index];
                if (enemy.RuntimeId != _selectedEnemyRuntimeId)
                {
                    continue;
                }

                _enemySelectionChanged.Invoke(enemy);
                return;
            }

            _selectedEnemyRuntimeId = -1;
            _enemySelectionChanged.Invoke(null);
        }
    }
}
