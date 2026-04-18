using System.Collections;
using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleBoardPanelController : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform dropArea;
        [SerializeField] private BattleBaseView playerBaseView;
        [SerializeField] private BattleBaseView enemyBaseView;
        [SerializeField] private RectTransform playerUnitsRoot;
        [SerializeField] private BattleBoardUnitView playerUnitPrefab;
        [SerializeField] private RectTransform[] playerLaneAnchors;
        [SerializeField] private RectTransform enemyUnitsRoot;
        [SerializeField] private BattleBoardUnitView enemyUnitPrefab;
        [SerializeField] private RectTransform[] enemyLaneAnchors;
        [SerializeField] private BattleTurnBannerView turnBannerView;
        [SerializeField] private float enemySpawnInterval = 0.08f;
        [SerializeField] private float spawnDuration = 0.18f;

        private readonly Dictionary<int, BattleBoardUnitView> _playerViews = new();
        private readonly Dictionary<int, BattleBoardUnitView> _enemyViews = new();
        private string _presentedTurnKey = string.Empty;
        private Coroutine _enemySpawnRoutine;

        public void ResetState()
        {
            _presentedTurnKey = string.Empty;
            StopEnemySpawnRoutine();
            ClearViews(_playerViews);
            ClearViews(_enemyViews);
            turnBannerView.HideImmediate();
        }

        public void Refresh(BattleHudViewData viewData)
        {
            backgroundImage.sprite = viewData.BackgroundSprite;
            backgroundImage.enabled = viewData.BackgroundSprite != null;
            playerBaseView.Refresh(viewData.PlayerBase);
            enemyBaseView.Refresh(viewData.EnemyBase);

            var turnKey = $"{viewData.LevelKey}:{viewData.CurrentTurn}";
            var isNewTurn = viewData.Phase == Data.Common.Enums.PhaseType.BattlePreparation &&
                            !string.IsNullOrWhiteSpace(viewData.LevelKey) &&
                            _presentedTurnKey != turnKey;
            if (isNewTurn)
            {
                _presentedTurnKey = turnKey;
                ClearViews(_playerViews);
                ClearViews(_enemyViews);
                SyncUnits(viewData.PlayerUnits, playerUnitPrefab, playerUnitsRoot, playerLaneAnchors, _playerViews, false);
                SyncUnits(viewData.EnemyUnits, enemyUnitPrefab, enemyUnitsRoot, enemyLaneAnchors, _enemyViews, true);
                turnBannerView.ShowTurn(viewData.CurrentTurn);
                return;
            }

            if (viewData.Phase == Data.Common.Enums.PhaseType.BattlePreparation)
            {
                SyncUnits(viewData.PlayerUnits, playerUnitPrefab, playerUnitsRoot, playerLaneAnchors, _playerViews, false);
                SyncUnits(viewData.EnemyUnits, enemyUnitPrefab, enemyUnitsRoot, enemyLaneAnchors, _enemyViews, false);
            }
        }

        public bool IsScreenPointOverDropArea(Vector2 screenPoint)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(dropArea, screenPoint, uiCamera);
        }

        private void SyncUnits(
            IReadOnlyList<BattleBoardUnitViewData> units,
            BattleBoardUnitView prefab,
            RectTransform root,
            IReadOnlyList<RectTransform> anchors,
            Dictionary<int, BattleBoardUnitView> viewsByRuntimeId,
            bool animateNewViews)
        {
            var activeRuntimeIds = new HashSet<int>();
            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);

                var isNew = false;
                if (!viewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(prefab, root);
                    viewsByRuntimeId.Add(unit.RuntimeId, view);
                    isNew = true;
                }

                view.Refresh(unit);
                var anchorIndex = Mathf.Clamp(unit.BoardIndex, 0, anchors.Count - 1);
                var targetAnchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(root, uiCamera, anchors[anchorIndex].position);
                if (isNew)
                {
                    view.RectTransform.anchoredPosition = targetAnchoredPosition;
                    view.RectTransform.localScale = animateNewViews ? Vector3.zero : Vector3.one;
                }

                UiAnimationManager.Instance.PlayMoveAndScale(
                    view.RectTransform,
                    "board-position",
                    targetAnchoredPosition,
                    Vector3.one,
                    spawnDuration,
                    DG.Tweening.Ease.OutCubic,
                    DG.Tweening.Ease.OutBack);
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in viewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
                {
                    staleRuntimeIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleRuntimeIds.Count; index++)
            {
                var runtimeId = staleRuntimeIds[index];
                Destroy(viewsByRuntimeId[runtimeId].gameObject);
                viewsByRuntimeId.Remove(runtimeId);
            }

            if (animateNewViews)
            {
                StopEnemySpawnRoutine();
                _enemySpawnRoutine = StartCoroutine(PlaySpawnSequence(viewsByRuntimeId, units, enemySpawnInterval));
            }
        }

        private IEnumerator PlaySpawnSequence(
            IReadOnlyDictionary<int, BattleBoardUnitView> viewsByRuntimeId,
            IReadOnlyList<BattleBoardUnitViewData> units,
            float interval)
        {
            for (var index = 0; index < units.Count; index++)
            {
                if (viewsByRuntimeId.TryGetValue(units[index].RuntimeId, out var view))
                {
                    view.RectTransform.localScale = Vector3.zero;
                    UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, "spawn", Vector3.one, spawnDuration, DG.Tweening.Ease.OutBack);
                }

                yield return new WaitForSecondsRealtime(interval);
            }

            _enemySpawnRoutine = null;
        }

        private void StopEnemySpawnRoutine()
        {
            if (_enemySpawnRoutine == null)
            {
                return;
            }

            StopCoroutine(_enemySpawnRoutine);
            _enemySpawnRoutine = null;
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
