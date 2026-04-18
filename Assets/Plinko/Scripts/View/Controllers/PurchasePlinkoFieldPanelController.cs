using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchaseTrainingCompletionVisualPayload
    {
        public int RuntimeId;
        public Vector3 WorldPosition;
        public PurchaseTrainedUnitCardViewData CardData;
    }

    public sealed class PurchasePlinkoFieldPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform spawnPoint;
        [SerializeField] private RectTransform exitPoint;
        [SerializeField] private RectTransform pinsRoot;
        [SerializeField] private PurchaseTrainingPinView pinPrefab;
        [SerializeField] private RectTransform basketsRoot;
        [SerializeField] private PurchaseTrainingBasketView basketPrefab;
        [SerializeField] private RectTransform tokensRoot;
        [SerializeField] private PurchaseTrainingTokenView tokenPrefab;
        [SerializeField] private float pixelsPerFieldUnit = 120f;

        private readonly List<PurchaseTrainingPinView> _pinViews = new();
        private readonly List<PurchaseTrainingBasketView> _basketViews = new();
        private readonly Dictionary<string, PurchaseTrainingPinView> _pinViewsByCell = new();
        private readonly Dictionary<string, PurchaseTrainingBasketView> _basketViewsById = new();
        private readonly Dictionary<int, PurchaseTrainingTokenView> _tokenViewsByRuntimeId = new();
        private readonly Dictionary<int, int> _lastLandmarkByRuntimeId = new();
        private string _fieldSignature = string.Empty;
        private float _horizontalSpacing = 1f;
        private float _verticalSpacing = 1f;

        public void ResetState()
        {
            _fieldSignature = string.Empty;
            ClearFieldViews();
            ClearTokens();
        }

        public List<PurchaseTrainingCompletionVisualPayload> Refresh(PurchasePhaseViewData viewData)
        {
            if (_fieldSignature != viewData.FieldSignature)
            {
                _fieldSignature = viewData.FieldSignature;
                _horizontalSpacing = viewData.FieldHorizontalSpacing;
                _verticalSpacing = viewData.FieldVerticalSpacing;
                RebuildField(viewData);
                ClearTokens();
            }
            
            var completions = ReleaseCompletedTokens(viewData.CompletedTrainings);
            SyncActiveTokens(viewData.ActiveTrainings, viewData.CompletedTrainings);
            
            return completions;
        }

        public List<PurchaseTrainingCompletionVisualPayload> Refresh(RetrainingPhaseViewData viewData)
        {
            if (_fieldSignature != viewData.FieldSignature)
            {
                _fieldSignature = viewData.FieldSignature;
                _horizontalSpacing = viewData.FieldHorizontalSpacing;
                _verticalSpacing = viewData.FieldVerticalSpacing;
                RebuildField(viewData);
                ClearTokens();
            }

            var completions = ReleaseCompletedTokens(viewData.CompletedTrainings);
            SyncActiveTokens(viewData.ActiveTrainings, viewData.CompletedTrainings);

            return completions;
        }

        private void RebuildField(PurchasePhaseViewData viewData)
        {
            ClearFieldViews();

            var rowCounts = BuildRowCounts(viewData.Pins);
            for (var index = 0; index < viewData.Pins.Count; index++)
            {
                var pinData = viewData.Pins[index];
                var pinView = Instantiate(pinPrefab, pinsRoot);
                var rect = pinView.RectTransform;
                rect.anchoredPosition = BuildPinPosition(pinData.RowIndex, pinData.ColumnIndex, rowCounts);
                pinView.Refresh(pinData);
                _pinViews.Add(pinView);
                _pinViewsByCell[BuildCellKey(pinData.RowIndex, pinData.ColumnIndex)] = pinView;
            }

            for (var index = 0; index < viewData.Baskets.Count; index++)
            {
                var basketData = viewData.Baskets[index];
                var basketView = Instantiate(basketPrefab, basketsRoot);
                var rect = basketView.RectTransform;
                rect.anchoredPosition = BuildBasketPosition(basketData.BasketIndex, viewData.Baskets.Count);
                basketView.Refresh(basketData);
                _basketViews.Add(basketView);
                _basketViewsById[basketData.BasketId] = basketView;
            }
        }

        private void RebuildField(RetrainingPhaseViewData viewData)
        {
            ClearFieldViews();

            var rowCounts = BuildRowCounts(viewData.Pins);
            for (var index = 0; index < viewData.Pins.Count; index++)
            {
                var pinData = viewData.Pins[index];
                var pinView = Instantiate(pinPrefab, pinsRoot);
                var rect = pinView.RectTransform;
                rect.anchoredPosition = BuildPinPosition(pinData.RowIndex, pinData.ColumnIndex, rowCounts);
                pinView.Refresh(pinData);
                _pinViews.Add(pinView);
                _pinViewsByCell[BuildCellKey(pinData.RowIndex, pinData.ColumnIndex)] = pinView;
            }

            for (var index = 0; index < viewData.Baskets.Count; index++)
            {
                var basketData = viewData.Baskets[index];
                var basketView = Instantiate(basketPrefab, basketsRoot);
                var rect = basketView.RectTransform;
                rect.anchoredPosition = BuildBasketPosition(basketData.BasketIndex, viewData.Baskets.Count);
                basketView.Refresh(basketData);
                _basketViews.Add(basketView);
                _basketViewsById[basketData.BasketId] = basketView;
            }
        }

        private List<PurchaseTrainingCompletionVisualPayload> ReleaseCompletedTokens(
            IReadOnlyList<PurchaseTrainedUnitCardViewData> completedTrainings)
        {
            var result = new List<PurchaseTrainingCompletionVisualPayload>();
            for (var index = 0; index < completedTrainings.Count; index++)
            {
                var completed = completedTrainings[index];
                var worldPosition = UiRectTransformUtility.GetWorldCenter(exitPoint);
                if (_tokenViewsByRuntimeId.TryGetValue(completed.RuntimeId, out var tokenView))
                {
                    worldPosition = UiRectTransformUtility.GetWorldCenter(tokenView.RectTransform);
                    Destroy(tokenView.gameObject);
                    _tokenViewsByRuntimeId.Remove(completed.RuntimeId);
                    _lastLandmarkByRuntimeId.Remove(completed.RuntimeId);
                }

                result.Add(new PurchaseTrainingCompletionVisualPayload
                {
                    RuntimeId = completed.RuntimeId,
                    WorldPosition = worldPosition,
                    CardData = completed
                });
            }

            return result;
        }

        private void SyncActiveTokens(
            IReadOnlyList<PurchaseTrainingRunViewData> activeTrainings,
            IReadOnlyList<PurchaseTrainedUnitCardViewData> completedTrainings)
        {
            var completedRuntimeIds = new HashSet<int>();
            for (var index = 0; index < completedTrainings.Count; index++)
            {
                completedRuntimeIds.Add(completedTrainings[index].RuntimeId);
            }

            var activeRuntimeIds = new HashSet<int>();
            for (var index = 0; index < activeTrainings.Count; index++)
            {
                var run = activeTrainings[index];
                activeRuntimeIds.Add(run.RuntimeId);
                if (!run.HasStarted)
                {
                    continue;
                }

                if (!_tokenViewsByRuntimeId.TryGetValue(run.RuntimeId, out var tokenView))
                {
                    tokenView = Instantiate(tokenPrefab, tokensRoot);
                    tokenView.SetSprite(run.TrainingFieldSprite);
                    _tokenViewsByRuntimeId[run.RuntimeId] = tokenView;
                    _lastLandmarkByRuntimeId[run.RuntimeId] = -1;
                }

                tokenView.SetSprite(run.TrainingFieldSprite);
                ApplyTokenPosition(run, tokenView);
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in _tokenViewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key) && !completedRuntimeIds.Contains(pair.Key))
                {
                    staleRuntimeIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleRuntimeIds.Count; index++)
            {
                var runtimeId = staleRuntimeIds[index];
                Destroy(_tokenViewsByRuntimeId[runtimeId].gameObject);
                _tokenViewsByRuntimeId.Remove(runtimeId);
                _lastLandmarkByRuntimeId.Remove(runtimeId);
            }
        }

        private void ApplyTokenPosition(PurchaseTrainingRunViewData run, PurchaseTrainingTokenView tokenView)
        {
            var points = BuildRoute(run);
            if (points.Count == 0)
            {
                tokenView.RectTransform.position = spawnPoint.position;
                return;
            }

            if (points.Count == 1)
            {
                tokenView.RectTransform.position = points[0];
                return;
            }

            var normalized = Mathf.Clamp01(run.Duration <= 0f ? 1f : run.Elapsed / run.Duration);
            var routeProgress = normalized * (points.Count - 1);
            var segmentIndex = Mathf.Clamp(Mathf.FloorToInt(routeProgress), 0, points.Count - 2);
            var segmentProgress = routeProgress - segmentIndex;
            tokenView.RectTransform.position = Vector3.Lerp(points[segmentIndex], points[segmentIndex + 1], segmentProgress);

            var landmarkIndex = Mathf.Clamp(Mathf.FloorToInt(routeProgress + 0.0001f), 0, points.Count - 1);
            if (!_lastLandmarkByRuntimeId.TryGetValue(run.RuntimeId, out var previousLandmark) || previousLandmark != landmarkIndex)
            {
                _lastLandmarkByRuntimeId[run.RuntimeId] = landmarkIndex;
                PlayLandmarkPunch(run, landmarkIndex, tokenView);
            }
        }

        private List<Vector3> BuildRoute(PurchaseTrainingRunViewData run)
        {
            var points = new List<Vector3> { UiRectTransformUtility.GetWorldCenter(spawnPoint) };

            for (var index = 0; index < run.Nodes.Count; index++)
            {
                var node = run.Nodes[index];
                if (_pinViewsByCell.TryGetValue(BuildCellKey(node.RowIndex, node.ColumnIndex), out var pinView))
                {
                    points.Add(UiRectTransformUtility.GetWorldCenter(pinView.RectTransform));
                }
            }

            if (!string.IsNullOrWhiteSpace(run.FinalBasketId) &&
                _basketViewsById.TryGetValue(run.FinalBasketId, out var basketView))
            {
                points.Add(UiRectTransformUtility.GetWorldCenter(basketView.RectTransform));
            }

            points.Add(UiRectTransformUtility.GetWorldCenter(exitPoint));
            return points;
        }

        private void PlayLandmarkPunch(PurchaseTrainingRunViewData run, int landmarkIndex, PurchaseTrainingTokenView tokenView)
        {
            if (landmarkIndex <= 0)
            {
                return;
            }

            tokenView.PlayPunch();

            if (landmarkIndex <= run.Nodes.Count)
            {
                var node = run.Nodes[landmarkIndex - 1];
                if (_pinViewsByCell.TryGetValue(BuildCellKey(node.RowIndex, node.ColumnIndex), out var pinView))
                {
                    pinView.PlayPunch();
                }

                return;
            }

            if (landmarkIndex == run.Nodes.Count + 1 &&
                !string.IsNullOrWhiteSpace(run.FinalBasketId) &&
                _basketViewsById.TryGetValue(run.FinalBasketId, out var basketView))
            {
                basketView.PlayPunch();
            }
        }

        private Vector2 BuildPinPosition(int rowIndex, int columnIndex, IReadOnlyDictionary<int, int> rowCounts)
        {
            var rowCount = rowCounts.TryGetValue(rowIndex, out var count) ? count : 1;
            var x = (columnIndex - (rowCount - 1) * 0.5f) * _horizontalSpacing * pixelsPerFieldUnit;
            var y = -rowIndex * _verticalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, y);
        }

        private Vector2 BuildBasketPosition(int basketIndex, int totalBasketCount)
        {
            var x = (basketIndex - (totalBasketCount - 1) * 0.5f) * _horizontalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, 0f);
        }

        private static Dictionary<int, int> BuildRowCounts(IReadOnlyList<PurchaseFieldPinViewData> pins)
        {
            var result = new Dictionary<int, int>();
            for (var index = 0; index < pins.Count; index++)
            {
                var pin = pins[index];
                if (!result.TryGetValue(pin.RowIndex, out var rowCount) || rowCount < pin.ColumnIndex + 1)
                {
                    result[pin.RowIndex] = pin.ColumnIndex + 1;
                }
            }

            return result;
        }

        private void ClearFieldViews()
        {
            for (var index = 0; index < _pinViews.Count; index++)
            {
                Destroy(_pinViews[index].gameObject);
            }

            for (var index = 0; index < _basketViews.Count; index++)
            {
                Destroy(_basketViews[index].gameObject);
            }

            _pinViews.Clear();
            _basketViews.Clear();
            _pinViewsByCell.Clear();
            _basketViewsById.Clear();
        }

        private void ClearTokens()
        {
            foreach (var pair in _tokenViewsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _tokenViewsByRuntimeId.Clear();
            _lastLandmarkByRuntimeId.Clear();
        }

        private static string BuildCellKey(int rowIndex, int columnIndex)
        {
            return $"{rowIndex}:{columnIndex}";
        }
    }
}
