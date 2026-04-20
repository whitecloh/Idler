using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class SignalPurchaseFieldPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject generatorActiveRoot;
        [SerializeField] private GameObject generatorBrokenRoot;
        [SerializeField] private RectTransform generatorAnchor;
        [SerializeField] private RectTransform spawnPoint;
        [SerializeField] private RectTransform exitPoint;
        [SerializeField] private RectTransform pinsRoot;
        [SerializeField] private PurchaseTrainingPinView pinPrefab;
        [SerializeField] private RectTransform basketsRoot;
        [SerializeField] private PurchaseTrainingBasketView basketPrefab;
        [SerializeField] private RectTransform tokensRoot;
        [SerializeField] private PurchaseTrainingTokenView tokenPrefab;
        [SerializeField] private Sprite signalSprite;
        [SerializeField] private float pixelsPerFieldUnit = 120f;
        [Header("Signal Intro")]
        [SerializeField] private RectTransform lightningSourcePoint;
        [SerializeField] private RectTransform lightningStrikeRoot;
        [SerializeField] private CanvasGroup lightningStrikeCanvasGroup;
        [SerializeField] private RectTransform generatorChargeRoot;
        [SerializeField] private CanvasGroup generatorChargeCanvasGroup;
        [SerializeField] private float lightningIntroDuration = 1f;
        [SerializeField] private float landmarkPauseDuration = 0.08f;

        private readonly List<PurchaseTrainingPinView> _pinViews = new();
        private readonly List<PurchaseTrainingBasketView> _basketViews = new();
        private readonly Dictionary<string, PurchaseTrainingPinView> _pinViewsByCell = new();
        private readonly Dictionary<string, PurchaseTrainingBasketView> _basketViewsById = new();
        private readonly Dictionary<int, PurchaseTrainingTokenView> _tokenViewsByRuntimeId = new();
        private readonly Dictionary<int, int> _lastLandmarkByRuntimeId = new();
        private readonly Dictionary<int, Vector3> _pendingCardWorldTargets = new();
        private string _fieldSignature = string.Empty;
        private float _horizontalSpacing = 1f;
        private float _verticalSpacing = 1f;

        public float LightningIntroDuration => Mathf.Max(0f, lightningIntroDuration);

        public void ResetState()
        {
            _fieldSignature = string.Empty;
            ClearFieldViews();
            ClearTokens();
            _pendingCardWorldTargets.Clear();
            HideLaunchEffectsImmediate();
            SetGeneratorState(false);
        }

        public void SetPendingCardTargets(IReadOnlyDictionary<int, Vector3> pendingCardTargets)
        {
            _pendingCardWorldTargets.Clear();
            if (pendingCardTargets == null)
            {
                return;
            }

            foreach (var pair in pendingCardTargets)
            {
                _pendingCardWorldTargets[pair.Key] = pair.Value;
            }
        }

        public void PlayLaunchIntro()
        {
            PlayLightningStrike();
            PlayGeneratorCharge();
        }

        public void Refresh(SignalPurchasePhaseViewData viewData)
        {
            if (_fieldSignature != viewData.FieldSignature)
            {
                _fieldSignature = viewData.FieldSignature;
                _horizontalSpacing = viewData.FieldHorizontalSpacing;
                _verticalSpacing = viewData.FieldVerticalSpacing;
                RebuildField(viewData);
                ClearTokens();
            }

            SetGeneratorState(viewData.IsGeneratorBroken);
            SyncActiveTokens(viewData.ActiveSignals);
        }

        public List<PurchaseTrainingCompletionVisualPayload> ReleaseCompletedSignals(
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

        private void SetGeneratorState(bool isBroken)
        {
            if (generatorActiveRoot != null)
            {
                generatorActiveRoot.SetActive(!isBroken);
            }

            if (generatorBrokenRoot != null)
            {
                generatorBrokenRoot.SetActive(isBroken);
            }
        }

        private void RebuildField(SignalPurchasePhaseViewData viewData)
        {
            ClearFieldViews();

            var rowCounts = BuildRowCounts(viewData.Pins);
            for (var index = 0; index < viewData.Pins.Count; index++)
            {
                var pinData = viewData.Pins[index];
                var pinView = Instantiate(pinPrefab, pinsRoot);
                pinView.RectTransform.anchoredPosition = BuildPinPosition(pinData.RowIndex, pinData.ColumnIndex, rowCounts);
                pinView.Refresh(pinData);
                _pinViews.Add(pinView);
                _pinViewsByCell[BuildCellKey(pinData.RowIndex, pinData.ColumnIndex)] = pinView;
            }

            for (var index = 0; index < viewData.Baskets.Count; index++)
            {
                var basketData = viewData.Baskets[index];
                var basketView = Instantiate(basketPrefab, basketsRoot);
                basketView.RectTransform.anchoredPosition = BuildBasketPosition(basketData.BasketIndex, viewData.Baskets.Count, rowCounts);
                basketView.Refresh(basketData);
                _basketViews.Add(basketView);
                _basketViewsById[basketData.BasketId] = basketView;
            }
        }

        private void SyncActiveTokens(IReadOnlyList<PurchaseTrainingRunViewData> activeSignals)
        {
            var activeRuntimeIds = new HashSet<int>();
            for (var index = 0; index < activeSignals.Count; index++)
            {
                var run = activeSignals[index];
                activeRuntimeIds.Add(run.RuntimeId);
                if (!run.HasStarted)
                {
                    continue;
                }

                if (!_tokenViewsByRuntimeId.TryGetValue(run.RuntimeId, out var tokenView))
                {
                    tokenView = Instantiate(tokenPrefab, tokensRoot);
                    _tokenViewsByRuntimeId[run.RuntimeId] = tokenView;
                    _lastLandmarkByRuntimeId[run.RuntimeId] = -1;
                }

                tokenView.SetSprite(signalSprite != null ? signalSprite : run.TrainingFieldSprite);
                ApplyTokenPosition(run, tokenView);
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in _tokenViewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
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

            var routeProgress = EvaluateRouteProgress(run, points.Count);
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

            if (_pendingCardWorldTargets.TryGetValue(run.RuntimeId, out var cardTarget))
            {
                points.Add(cardTarget);
            }
            else
            {
                points.Add(UiRectTransformUtility.GetWorldCenter(exitPoint));
            }
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
                    AudioManager.Instance?.Play(GameAudioCueType.PinImpact);
                }

                return;
            }

            if (landmarkIndex == run.Nodes.Count + 1 &&
                !string.IsNullOrWhiteSpace(run.FinalBasketId) &&
                _basketViewsById.TryGetValue(run.FinalBasketId, out var basketView))
            {
                basketView.PlayPunch();
                AudioManager.Instance?.Play(GameAudioCueType.BasketImpact);
            }
        }

        private Vector2 BuildPinPosition(int rowIndex, int columnIndex, IReadOnlyDictionary<int, int> rowCounts)
        {
            var rowCount = rowCounts.TryGetValue(rowIndex, out var count) ? count : 1;
            var x = (columnIndex - (rowCount - 1) * 0.5f) * _horizontalSpacing * pixelsPerFieldUnit;
            var y = GetTopY(rowCounts) - rowIndex * _verticalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, y);
        }

        private Vector2 BuildBasketPosition(int basketIndex, int totalBasketCount, IReadOnlyDictionary<int, int> rowCounts)
        {
            var x = (basketIndex - (totalBasketCount - 1) * 0.5f) * _horizontalSpacing * pixelsPerFieldUnit;
            var y = GetTopY(rowCounts) - GetTotalRowCount(rowCounts) * _verticalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, y);
        }

        private float GetTopY(IReadOnlyDictionary<int, int> rowCounts)
        {
            var totalRowCount = Mathf.Max(1, GetTotalRowCount(rowCounts));
            return (totalRowCount - 1) * _verticalSpacing * pixelsPerFieldUnit * 0.5f;
        }

        private static int GetTotalRowCount(IReadOnlyDictionary<int, int> rowCounts)
        {
            var maxRowIndex = -1;
            foreach (var pair in rowCounts)
            {
                if (pair.Key > maxRowIndex)
                {
                    maxRowIndex = pair.Key;
                }
            }

            return maxRowIndex + 1;
        }

        private float EvaluateRouteProgress(PurchaseTrainingRunViewData run, int pointCount)
        {
            if (pointCount <= 1 || run.Duration <= 0f)
            {
                return 0f;
            }

            var segmentCount = pointCount - 1;
            var landmarkCount = Mathf.Max(0, pointCount - 2);
            var totalPauseDuration = landmarkCount * landmarkPauseDuration;
            var totalMoveDuration = Mathf.Max(0.0001f, run.Duration - totalPauseDuration);
            var segmentDuration = totalMoveDuration / segmentCount;
            var remaining = Mathf.Clamp(run.Elapsed, 0f, run.Duration);

            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                if (remaining <= segmentDuration)
                {
                    return segmentIndex + remaining / segmentDuration;
                }

                remaining -= segmentDuration;
                var hasPauseAfterSegment = segmentIndex < landmarkCount;
                if (!hasPauseAfterSegment)
                {
                    continue;
                }

                if (remaining <= landmarkPauseDuration)
                {
                    return segmentIndex + 1f;
                }

                remaining -= landmarkPauseDuration;
            }

            return pointCount - 1;
        }

        private void PlayLightningStrike()
        {
            if (lightningStrikeRoot == null)
            {
                return;
            }

            var start = lightningSourcePoint != null ? lightningSourcePoint.position : lightningStrikeRoot.position + Vector3.up * 320f;
            var end = generatorAnchor != null
                ? generatorAnchor.position
                : generatorActiveRoot != null
                    ? generatorActiveRoot.transform.position
                    : lightningStrikeRoot.position;

            lightningStrikeRoot.gameObject.SetActive(true);
            lightningStrikeRoot.position = (start + end) * 0.5f;
            var delta = end - start;
            lightningStrikeRoot.up = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.down;
            lightningStrikeRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, delta.magnitude);

            if (lightningStrikeCanvasGroup != null)
            {
                DOTween.Kill(lightningStrikeCanvasGroup);
                lightningStrikeCanvasGroup.alpha = 0f;
                DOTween.Sequence()
                    .SetLink(lightningStrikeCanvasGroup.gameObject)
                    .Append(lightningStrikeCanvasGroup.DOFade(1f, lightningIntroDuration * 0.15f))
                    .AppendInterval(lightningIntroDuration * 0.7f)
                    .Append(lightningStrikeCanvasGroup.DOFade(0f, lightningIntroDuration * 0.15f))
                    .OnComplete(() => lightningStrikeRoot.gameObject.SetActive(false));
                return;
            }

            DOVirtual.DelayedCall(lightningIntroDuration, () =>
            {
                if (lightningStrikeRoot != null)
                {
                    lightningStrikeRoot.gameObject.SetActive(false);
                }
            }).SetLink(lightningStrikeRoot.gameObject);
        }

        private void PlayGeneratorCharge()
        {
            if (generatorChargeRoot == null)
            {
                return;
            }

            generatorChargeRoot.gameObject.SetActive(true);
            generatorChargeRoot.localScale = Vector3.one * 0.25f;
            UiAnimationManager.Instance.PlayScaleTo(
                generatorChargeRoot,
                "signal-generator-charge",
                Vector3.one,
                lightningIntroDuration,
                Ease.OutQuad);
            UiAnimationManager.Instance.PlayPunch(generatorChargeRoot, 0.8f);

            if (generatorChargeCanvasGroup != null)
            {
                DOTween.Kill(generatorChargeCanvasGroup);
                generatorChargeCanvasGroup.alpha = 0f;
                DOTween.Sequence()
                    .SetLink(generatorChargeCanvasGroup.gameObject)
                    .Append(generatorChargeCanvasGroup.DOFade(1f, lightningIntroDuration * 0.35f))
                    .AppendInterval(lightningIntroDuration * 0.5f)
                    .Append(generatorChargeCanvasGroup.DOFade(0f, lightningIntroDuration * 0.15f))
                    .OnComplete(() => generatorChargeRoot.gameObject.SetActive(false));
                return;
            }

            DOVirtual.DelayedCall(lightningIntroDuration, () =>
            {
                if (generatorChargeRoot != null)
                {
                    generatorChargeRoot.gameObject.SetActive(false);
                }
            }).SetLink(generatorChargeRoot.gameObject);
        }

        private void HideLaunchEffectsImmediate()
        {
            if (lightningStrikeCanvasGroup != null)
            {
                DOTween.Kill(lightningStrikeCanvasGroup);
                lightningStrikeCanvasGroup.alpha = 0f;
            }

            if (lightningStrikeRoot != null)
            {
                lightningStrikeRoot.gameObject.SetActive(false);
            }

            if (generatorChargeCanvasGroup != null)
            {
                DOTween.Kill(generatorChargeCanvasGroup);
                generatorChargeCanvasGroup.alpha = 0f;
            }

            if (generatorChargeRoot != null)
            {
                generatorChargeRoot.gameObject.SetActive(false);
            }
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
