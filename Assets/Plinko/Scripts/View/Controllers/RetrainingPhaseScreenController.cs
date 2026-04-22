using System.Collections;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class RetrainingPhaseScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private PurchasePlinkoFieldPanelController plinkoFieldPanel;
        [SerializeField] private PurchaseNewUnitsPanelController trainedUnitsPanel;
        [SerializeField] private RetrainingNextLevelPanelController nextLevelPanel;
        [SerializeField] private RetrainingShopPanelController shopPanel;
        [SerializeField] private float introDelay = 0.12f;

        private RetrainingPhaseBridge _retrainingPhaseBridge;
        private LocationBridge _locationBridge;
        private RetrainingPhaseViewData _viewData = new();
        private bool _isVisible;
        private string _introLevelKey = string.Empty;
        private Coroutine _introRoutine;

        public void Init(RetrainingPhaseBridge retrainingPhaseBridge, LocationBridge locationBridge)
        {
            _retrainingPhaseBridge = retrainingPhaseBridge;
            _locationBridge = locationBridge;

            levelTrackPanel.Init(_locationBridge.RequestReturnToMenu);
            nextLevelPanel.Init(_locationBridge);
            shopPanel.Init(_retrainingPhaseBridge);
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            if (!isVisible)
            {
                StopIntroRoutine();
            }

            root.SetActive(isVisible);
            if (isVisible)
            {
                ApplyViewData();
            }
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            _isVisible = isVisible;
            if (!isVisible)
            {
                StopIntroRoutine();
            }

            root.SetActive(isVisible);
            if (isVisible)
            {
                ApplyViewData();
            }
        }

        public void Refresh(RetrainingPhaseViewData viewData)
        {
            var levelChanged = _viewData.LevelKey != viewData.LevelKey;
            _viewData = viewData;

            if (levelChanged)
            {
                ResetVisualState();
            }

            if (_isVisible)
            {
                ApplyViewData();
            }
        }

        private void ApplyViewData()
        {
            levelTrackPanel.Refresh(_viewData);
            var completions = plinkoFieldPanel.Refresh(_viewData);
            trainedUnitsPanel.ApplyCompletedTrainings(completions);

            if (ShouldPlayIntro())
            {
                PlayIntro();
                return;
            }

            ApplyMainState();
        }

        private void PlayIntro()
        {
            if (_introRoutine != null || _introLevelKey == _viewData.LevelKey)
            {
                return;
            }

            _introLevelKey = _viewData.LevelKey;
            nextLevelPanel.ShowIntroState(_viewData);
            shopPanel.ShowIntroState(_viewData);
            _introRoutine = StartCoroutine(PlayIntroRoutine());
        }

        private IEnumerator PlayIntroRoutine()
        {
            yield return new WaitForSecondsRealtime(introDelay);
            _introRoutine = null;
            if (_isVisible)
            {
                ApplyMainState();
            }
        }

        private void ApplyMainState()
        {
            nextLevelPanel.Refresh(_viewData);
            shopPanel.Refresh(_viewData, nextLevelPanel);
        }

        private bool ShouldPlayIntro()
        {
            return !string.IsNullOrWhiteSpace(_viewData.LevelKey) &&
                   _introLevelKey != _viewData.LevelKey &&
                   _viewData.ActiveTrainingCount <= 0 &&
                   _viewData.RetrainedArmyPreviewUnits.Count == 0 &&
                   _viewData.PendingArmyPreviewUnits.Count < _viewData.AllOwnedArmyPreviewUnits.Count &&
                   _viewData.Offers.Count > 0;
        }

        private void ResetVisualState()
        {
            StopIntroRoutine();
            _introLevelKey = string.Empty;
            levelTrackPanel.ResetState();
            plinkoFieldPanel.ResetState();
            trainedUnitsPanel.ResetState();
            nextLevelPanel.ResetState();
            shopPanel.ResetState();
        }

        private void StopIntroRoutine()
        {
            if (_introRoutine == null)
            {
                return;
            }

            StopCoroutine(_introRoutine);
            _introRoutine = null;
        }
    }
}
