using System;
using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BaseDefenseBoardPanelController : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private BaseDefenseBaseView playerBaseView;
        [SerializeField] private RectTransform playerUnitsRoot;
        [SerializeField] private BattleBoardUnitView playerUnitPrefab;
        [SerializeField] private RectTransform enemyUnitsRoot;
        [SerializeField] private BattleBoardUnitView enemyUnitPrefab;
        [SerializeField] private RectTransform previewUnitsRoot;
        [SerializeField] private BattleBoardUnitView previewUnitPrefab;
        [SerializeField] private BaseDefenseGridCellView[] boardCells;
        [SerializeField] private BaseDefenseGridCellView[] previewCells;
        [SerializeField] private BattleTurnBannerView turnBannerView;
        [SerializeField] private float moveDuration = 0.18f;
        [SerializeField] private float spawnInterval = 0.08f;

        private readonly Dictionary<int, BattleBoardUnitView> _playerViews = new();
        private readonly Dictionary<int, BattleBoardUnitView> _enemyViews = new();
        private readonly Dictionary<int, BattleBoardUnitView> _previewViews = new();
        private DefenceBattleHudViewData _viewData = new();
        private Action<int, int> _cellClicked;
        private string _presentedTurnKey = string.Empty;
        private HandCardViewData _selectedCard;
        private bool _canDeploy;
        private int _currentMana;

        public void Init(Action<int, int> cellClicked)
        {
            _cellClicked = cellClicked;
            for (var index = 0; index < boardCells.Length; index++)
            {
                boardCells[index].Bind(HandleBoardCellClicked);
            }
        }

        public void ResetState()
        {
            _presentedTurnKey = string.Empty;
            _selectedCard = null;
            ClearViews(_playerViews);
            ClearViews(_enemyViews);
            ClearViews(_previewViews);
            turnBannerView.HideImmediate();
            RefreshCellStates();
        }

        public void Refresh(DefenceBattleHudViewData viewData)
        {
            _viewData = viewData;
            backgroundImage.sprite = viewData.BackgroundSprite;
            backgroundImage.enabled = viewData.BackgroundSprite != null;
            playerBaseView.Refresh(viewData.PlayerBase, viewData.BaseDefenseCompletedTurns, viewData.BaseDefenseRequiredTurns);

            var turnKey = $"{viewData.LevelKey}:{viewData.CurrentTurn}";
            var isNewTurn = viewData.Phase == Plinko.Scripts.Data.Common.Enums.PhaseType.BattlePreparation &&
                            !string.IsNullOrWhiteSpace(viewData.LevelKey) &&
                            _presentedTurnKey != turnKey;
            if (isNewTurn)
            {
                _presentedTurnKey = turnKey;
                turnBannerView.ShowTurn(viewData.CurrentTurn);
            }

            SyncUnits(viewData.PlayerUnits, playerUnitPrefab, playerUnitsRoot, _playerViews, false);
            SyncUnits(viewData.EnemyUnits, enemyUnitPrefab, enemyUnitsRoot, _enemyViews, false);
            SyncPreviewUnits(viewData.NextWaveUnits);
            RefreshCellStates();
        }

        public void SetSelectedCard(HandCardViewData selectedCard, bool canDeploy, int currentMana)
        {
            _selectedCard = selectedCard;
            _canDeploy = canDeploy;
            _currentMana = currentMana;
            RefreshCellStates();
        }

        public void ClearSelectedCard()
        {
            _selectedCard = null;
            RefreshCellStates();
        }

        private void HandleBoardCellClicked(BaseDefenseGridCellView cellView)
        {
            if (_selectedCard == null || !_canDeploy || _currentMana < _selectedCard.ManaCost)
            {
                return;
            }

            for (var index = 0; index < boardCells.Length; index++)
            {
                if (boardCells[index] != cellView)
                {
                    continue;
                }

                var laneIndex = _viewData.BaseDefenseCellsPerLane > 0 ? index / _viewData.BaseDefenseCellsPerLane : 0;
                var cellIndex = _viewData.BaseDefenseCellsPerLane > 0 ? index % _viewData.BaseDefenseCellsPerLane : 0;
                _cellClicked?.Invoke(laneIndex, cellIndex);
                return;
            }
        }

        private void RefreshCellStates()
        {
            var occupiedPlayerCells = new HashSet<int>();
            foreach (var unit in _viewData.PlayerUnits)
            {
                occupiedPlayerCells.Add(ToBoardCellIndex(unit.LaneIndex, unit.CellIndex));
            }

            for (var index = 0; index < boardCells.Length; index++)
            {
                var cellIsPlayerSide = _viewData.BaseDefenseCellsPerLane > 0 &&
                                       index % _viewData.BaseDefenseCellsPerLane < _viewData.BaseDefensePlayerSideCellCount;
                var occupied = occupiedPlayerCells.Contains(index);
                var available = _selectedCard != null &&
                                _canDeploy &&
                                _currentMana >= _selectedCard.ManaCost &&
                                cellIsPlayerSide &&
                                !occupied;
                boardCells[index].SetState(available, available, false, occupied && cellIsPlayerSide);
            }
        }

        private void SyncUnits(
            IReadOnlyList<BattleBoardUnitViewData> units,
            BattleBoardUnitView prefab,
            RectTransform root,
            Dictionary<int, BattleBoardUnitView> viewsByRuntimeId,
            bool isPreview)
        {
            var activeRuntimeIds = new HashSet<int>();
            var occupancyCounts = new Dictionary<int, int>();
            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);
                if (!viewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(prefab, root);
                    viewsByRuntimeId.Add(unit.RuntimeId, view);
                }

                view.Refresh(unit);
                var boardIndex = ToBoardCellIndex(unit.LaneIndex, unit.CellIndex);
                var cellView = boardIndex >= 0 && boardIndex < boardCells.Length ? boardCells[boardIndex] : null;
                if (cellView == null)
                {
                    continue;
                }

                var stackIndex = occupancyCounts.TryGetValue(boardIndex, out var currentCount) ? currentCount : 0;
                occupancyCounts[boardIndex] = stackIndex + 1;
                var targetPosition = GetStackedTargetPosition(root, cellView.UnitAnchor, stackIndex, unit.IsEnemy);
                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    isPreview ? "preview-cell" : "board-cell",
                    targetPosition,
                    Vector3.one,
                    moveDuration,
                    Ease.OutCubic,
                    Ease.OutBack);
            }

            RemoveStaleViews(viewsByRuntimeId, activeRuntimeIds);
        }

        private void SyncPreviewUnits(IReadOnlyList<BattleBoardUnitViewData> units)
        {
            var activeRuntimeIds = new HashSet<int>();
            var occupancyCounts = new Dictionary<int, int>();
            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);
                if (!_previewViews.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(previewUnitPrefab, previewUnitsRoot);
                    _previewViews.Add(unit.RuntimeId, view);
                    view.RectTransform.localScale = Vector3.zero;
                    UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "spawn", Vector3.one, spawnInterval, Ease.OutBack);
                }

                view.Refresh(unit);
                var laneIndex = Mathf.Clamp(unit.LaneIndex, 0, previewCells.Length - 1);
                var anchor = previewCells[laneIndex].UnitAnchor;
                var stackIndex = occupancyCounts.TryGetValue(laneIndex, out var currentCount) ? currentCount : 0;
                occupancyCounts[laneIndex] = stackIndex + 1;
                var targetPosition = GetStackedTargetPosition(previewUnitsRoot, anchor, stackIndex, true);
                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    "preview-cell",
                    targetPosition,
                    Vector3.one,
                    moveDuration,
                    Ease.OutCubic,
                    Ease.OutBack);
            }

            RemoveStaleViews(_previewViews, activeRuntimeIds);
        }

        private Vector2 GetStackedTargetPosition(RectTransform root, RectTransform anchor, int stackIndex, bool isEnemy)
        {
            var basePosition = UiRectTransformUtility.WorldToAnchoredPosition(root, uiCamera, anchor.position);
            var horizontalOffset = isEnemy ? -18f : 18f;
            return basePosition + new Vector2(horizontalOffset * stackIndex, 0f);
        }

        private int ToBoardCellIndex(int laneIndex, int cellIndex)
        {
            return laneIndex * _viewData.BaseDefenseCellsPerLane + cellIndex;
        }

        private static void RemoveStaleViews(Dictionary<int, BattleBoardUnitView> viewsByRuntimeId, HashSet<int> activeRuntimeIds)
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

        private static void ClearViews(Dictionary<int, BattleBoardUnitView> views)
        {
            foreach (var pair in views)
            {
                Destroy(pair.Value.gameObject);
            }

            views.Clear();
        }
    }
}
