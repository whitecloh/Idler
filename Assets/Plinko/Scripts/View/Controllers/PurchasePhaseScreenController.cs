using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchasePhaseScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PurchasePlinkoFieldPanelController plinkoFieldPanel;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private PurchaseShopPanelController shopPanel;
        [SerializeField] private PurchaseNextLevelPanelController nextLevelPanel;
        [SerializeField] private PurchaseNewUnitsPanelController newUnitsPanel;

        private PurchasePhaseBridge _purchasePhaseBridge;
        private LocationBridge _locationBridge;
        private PurchasePhaseViewData _viewData = new();
        private bool _isVisible;

        public void Init(PurchasePhaseBridge purchasePhaseBridge, LocationBridge locationBridge)
        {
            _purchasePhaseBridge = purchasePhaseBridge;
            _locationBridge = locationBridge;

            shopPanel.Init(_purchasePhaseBridge);
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

        public void Refresh(PurchasePhaseViewData viewData)
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
            shopPanel.Refresh(_viewData);
            nextLevelPanel.Refresh(_viewData);
            var completions = plinkoFieldPanel.Refresh(_viewData);
            newUnitsPanel.ApplyCompletedTrainings(completions);
        }

        private void ResetVisualState()
        {
            plinkoFieldPanel.ResetState();
            levelTrackPanel.ResetState();
            shopPanel.ResetState();
            nextLevelPanel.ResetState();
            newUnitsPanel.ResetState();
        }
    }
}
