using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class SignalPurchasePhaseScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject generatorBrokenOverlayRoot;
        [SerializeField] private GameObject animationLockOverlayRoot;
        [SerializeField] private SignalPurchaseFieldPanelController plinkoFieldPanel;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private SignalPurchaseShopPanelController shopPanel;
        [SerializeField] private PurchaseNextLevelPanelController nextLevelPanel;
        [SerializeField] private SignalPurchaseNewUnitsPanelController newUnitsPanel;

        private readonly HashSet<int> _deferredArmyRevealRuntimeIds = new();
        private readonly HashSet<int> _processedCompletedRuntimeIds = new();
        private SignalPurchaseBridge _signalPurchaseBridge;
        private LocationBridge _locationBridge;
        private SignalPurchasePhaseViewData _viewData = new();
        private bool _isVisible;
        private bool _isTransferAnimating;

        public void Init(SignalPurchaseBridge signalPurchaseBridge, LocationBridge locationBridge)
        {
            _signalPurchaseBridge = signalPurchaseBridge;
            _locationBridge = locationBridge;
            levelTrackPanel.Init(_locationBridge.RequestReturnToMenu);
            shopPanel.Init(_signalPurchaseBridge);
            newUnitsPanel.Init(_signalPurchaseBridge);
            nextLevelPanel.Init(_locationBridge);
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
            if (isVisible)
            {
                ApplyViewData();
            }
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
        }

        public void Refresh(SignalPurchasePhaseViewData viewData)
        {
            var levelChanged = _viewData.LevelKey != viewData.LevelKey;
            var signalStarted = !_viewData.IsSignalRunning && viewData.IsSignalRunning;
            _viewData = viewData;

            if (levelChanged)
            {
                ResetVisualState();
            }

            if (signalStarted)
            {
                plinkoFieldPanel.PlayLaunchIntro();
            }

            if (_isVisible)
            {
                ApplyViewData();
            }
        }

        private void ApplyViewData()
        {
            levelTrackPanel.Refresh(_viewData);
            shopPanel.Refresh(_viewData);
            plinkoFieldPanel.SetPendingCardTargets(newUnitsPanel.BuildPendingCardTargets());
            var newCompletedTrainings = BuildUnprocessedCompletedTrainings(_viewData.CompletedTrainings);
            var completions = plinkoFieldPanel.ReleaseCompletedSignals(newCompletedTrainings);
            for (var index = 0; index < newCompletedTrainings.Count; index++)
            {
                _processedCompletedRuntimeIds.Add(newCompletedTrainings[index].RuntimeId);
            }

            plinkoFieldPanel.Refresh(_viewData);
            newUnitsPanel.ApplyCompletedTrainings(completions);
            newUnitsPanel.Refresh(_viewData);
            nextLevelPanel.Refresh(_viewData, _deferredArmyRevealRuntimeIds);

            if (!_viewData.IsSignalRunning && !_isTransferAnimating && newUnitsPanel.HasCompletedCardsReadyForTransfer)
            {
                StartTransferToArmyPreview();
                return;
            }

            ApplyOverlayState();
        }

        private void StartTransferToArmyPreview()
        {
            _isTransferAnimating = true;
            _deferredArmyRevealRuntimeIds.Clear();

            foreach (var runtimeId in newUnitsPanel.GetCompletedRuntimeIds())
            {
                _deferredArmyRevealRuntimeIds.Add(runtimeId);
            }

            nextLevelPanel.Refresh(_viewData, _deferredArmyRevealRuntimeIds);
            ApplyOverlayState();

            newUnitsPanel.ClearCompletedCards();
            _deferredArmyRevealRuntimeIds.Clear();
            _isTransferAnimating = false;
            newUnitsPanel.Refresh(_viewData);
            nextLevelPanel.Refresh(_viewData);
            ApplyOverlayState();
        }

        private void ApplyOverlayState()
        {
            if (animationLockOverlayRoot != null)
            {
                animationLockOverlayRoot.SetActive(_viewData.IsSignalRunning || _isTransferAnimating);
            }

            if (generatorBrokenOverlayRoot != null)
            {
                generatorBrokenOverlayRoot.SetActive(_viewData.IsGeneratorBroken && !_viewData.IsSignalRunning && !_isTransferAnimating);
            }
        }

        private void ResetVisualState()
        {
            _isTransferAnimating = false;
            _deferredArmyRevealRuntimeIds.Clear();
            _processedCompletedRuntimeIds.Clear();
            plinkoFieldPanel.ResetState();
            levelTrackPanel.ResetState();
            shopPanel.ResetState();
            nextLevelPanel.ResetState();
            newUnitsPanel.ResetState();
            ApplyOverlayState();
        }

        private List<PurchaseTrainedUnitCardViewData> BuildUnprocessedCompletedTrainings(
            IReadOnlyList<PurchaseTrainedUnitCardViewData> completedTrainings)
        {
            var result = new List<PurchaseTrainedUnitCardViewData>();
            for (var index = 0; index < completedTrainings.Count; index++)
            {
                var completedTraining = completedTrainings[index];
                if (_processedCompletedRuntimeIds.Contains(completedTraining.RuntimeId))
                {
                    continue;
                }

                result.Add(completedTraining);
            }

            return result;
        }
    }
}
