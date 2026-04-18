using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class RetrainingNextLevelPanelController : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image playerBaseImage;
        [SerializeField] private TMP_Text playerBaseHealthText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private RectTransform pendingUnitsRoot;
        [SerializeField] private PurchaseArmyPreviewUnitView pendingUnitPrefab;
        [SerializeField] private List<RectTransform> pendingUnitAnchors = new();
        [SerializeField] private RectTransform retrainedUnitsRoot;
        [SerializeField] private PurchaseArmyPreviewUnitView retrainedUnitPrefab;
        [SerializeField] private List<RectTransform> retrainedUnitAnchors = new();
        [SerializeField] private float spawnScaleDuration = 0.2f;
        [SerializeField] private float hideDuration = 0.18f;

        private readonly Dictionary<int, PurchaseArmyPreviewUnitView> _pendingViewsByRuntimeId = new();
        private readonly Dictionary<int, PurchaseArmyPreviewUnitView> _retrainedViewsByRuntimeId = new();
        private readonly Dictionary<int, Vector3> _lastKnownPendingWorldPositions = new();
        private LocationBridge _locationBridge;
        private bool _listenersBound;

        public void Init(LocationBridge locationBridge)
        {
            _locationBridge = locationBridge;
            if (_listenersBound)
            {
                return;
            }

            nextLevelButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(nextLevelButton.transform as RectTransform);
                _locationBridge.RequestAdvanceToNextLevel();
            });
            _listenersBound = true;
        }

        public void ResetState()
        {
            DestroyViews(_pendingViewsByRuntimeId);
            DestroyViews(_retrainedViewsByRuntimeId);
            _lastKnownPendingWorldPositions.Clear();
        }

        public void ShowIntroState(RetrainingPhaseViewData viewData)
        {
            ApplyBaseState(viewData);
            SyncPendingGroup(viewData.AllOwnedArmyPreviewUnits, true);
            SyncRetrainedGroup(new List<PurchaseArmyPreviewUnitViewData>(), true);
        }

        public void Refresh(RetrainingPhaseViewData viewData)
        {
            ApplyBaseState(viewData);
            SyncPendingGroup(viewData.PendingArmyPreviewUnits, false);
            SyncRetrainedGroup(viewData.RetrainedArmyPreviewUnits, false);
        }

        public bool TryGetLastKnownPendingWorldPosition(int runtimeId, out Vector3 worldPosition)
        {
            if (_pendingViewsByRuntimeId.TryGetValue(runtimeId, out var liveView))
            {
                worldPosition = UiRectTransformUtility.GetWorldCenter(liveView.RectTransform);
                _lastKnownPendingWorldPositions[runtimeId] = worldPosition;
                return true;
            }

            return _lastKnownPendingWorldPositions.TryGetValue(runtimeId, out worldPosition);
        }

        private void ApplyBaseState(RetrainingPhaseViewData viewData)
        {
            backgroundImage.sprite = viewData.NextBattleBackgroundSprite;
            backgroundImage.enabled = viewData.NextBattleBackgroundSprite != null;
            playerBaseImage.sprite = viewData.PlayerBaseSprite;
            playerBaseImage.enabled = viewData.PlayerBaseSprite != null;
            playerBaseHealthText.text = $"{viewData.PlayerBaseHealth}/{viewData.PlayerBaseMaxHealth}";
            nextLevelButton.interactable = viewData.CanAdvance;
        }

        private void SyncPendingGroup(IReadOnlyList<PurchaseArmyPreviewUnitViewData> units, bool immediate)
        {
            var activeRuntimeIds = new HashSet<int>();
            var visibleCount = Mathf.Min(units.Count, pendingUnitAnchors.Count);
            for (var index = 0; index < visibleCount; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);

                var isNew = false;
                if (!_pendingViewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(pendingUnitPrefab, pendingUnitsRoot);
                    _pendingViewsByRuntimeId[unit.RuntimeId] = view;
                    isNew = true;
                }

                var rect = view.RectTransform;
                rect.anchoredPosition = pendingUnitAnchors[index].anchoredPosition;
                view.Refresh(unit);
                _lastKnownPendingWorldPositions[unit.RuntimeId] = UiRectTransformUtility.GetWorldCenter(rect);

                if (isNew)
                {
                    if (immediate)
                    {
                        rect.localScale = Vector3.one;
                    }
                    else
                    {
                        rect.localScale = Vector3.zero;
                        UiAnimationManager.Instance.PlayScaleTo(rect, $"retraining-pending-spawn-{unit.RuntimeId}", Vector3.one, spawnScaleDuration, Ease.OutBack);
                    }
                }
                else
                {
                    rect.localScale = Vector3.one;
                }
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in _pendingViewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
                {
                    _lastKnownPendingWorldPositions[pair.Key] = UiRectTransformUtility.GetWorldCenter(pair.Value.RectTransform);
                    staleRuntimeIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleRuntimeIds.Count; index++)
            {
                var runtimeId = staleRuntimeIds[index];
                var view = _pendingViewsByRuntimeId[runtimeId];
                _pendingViewsByRuntimeId.Remove(runtimeId);
                if (immediate)
                {
                    Destroy(view.gameObject);
                }
                else
                {
                    UiAnimationManager.Instance.PlayScaleTo(view.RectTransform, $"retraining-pending-hide-{runtimeId}", Vector3.zero, hideDuration, Ease.InBack, () =>
                    {
                        if (view != null)
                        {
                            Destroy(view.gameObject);
                        }
                    });
                }
            }
        }

        private void SyncRetrainedGroup(IReadOnlyList<PurchaseArmyPreviewUnitViewData> units, bool immediate)
        {
            var activeRuntimeIds = new HashSet<int>();
            var visibleCount = Mathf.Min(units.Count, retrainedUnitAnchors.Count);
            for (var index = 0; index < visibleCount; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);

                var isNew = false;
                if (!_retrainedViewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(retrainedUnitPrefab, retrainedUnitsRoot);
                    _retrainedViewsByRuntimeId[unit.RuntimeId] = view;
                    isNew = true;
                }

                var rect = view.RectTransform;
                rect.anchoredPosition = retrainedUnitAnchors[index].anchoredPosition;
                view.Refresh(unit);

                if (isNew)
                {
                    if (immediate)
                    {
                        rect.localScale = Vector3.one;
                    }
                    else
                    {
                        rect.localScale = Vector3.zero;
                        UiAnimationManager.Instance.PlayScaleTo(rect, $"retraining-retrained-spawn-{unit.RuntimeId}", Vector3.one, spawnScaleDuration, Ease.OutBack);
                    }
                }
                else
                {
                    rect.localScale = Vector3.one;
                }
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in _retrainedViewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
                {
                    staleRuntimeIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleRuntimeIds.Count; index++)
            {
                var runtimeId = staleRuntimeIds[index];
                Destroy(_retrainedViewsByRuntimeId[runtimeId].gameObject);
                _retrainedViewsByRuntimeId.Remove(runtimeId);
            }
        }

        private static void DestroyViews(Dictionary<int, PurchaseArmyPreviewUnitView> views)
        {
            foreach (var pair in views)
            {
                Destroy(pair.Value.gameObject);
            }

            views.Clear();
        }
    }
}
