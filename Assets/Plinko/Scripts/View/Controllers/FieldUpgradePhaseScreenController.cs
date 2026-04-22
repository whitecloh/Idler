using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class FieldUpgradePhaseScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private UiCanvasGroupVisibility selectionOverlayVisibility;
        [SerializeField] private FieldUpgradePlinkoFieldPanelController plinkoFieldPanel;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private FieldUpgradeShopPanelController shopPanel;
        [SerializeField] private PurchaseNextLevelPanelController nextLevelPanel;
        [SerializeField] private FieldUpgradeReplacementPanelController replacementPanel;

        private FieldUpgradeBridge _fieldUpgradeBridge;
        private LocationBridge _locationBridge;
        private FieldUpgradePhaseViewData _viewData = new();
        private bool _isVisible;
        private bool _overlayStateInitialized;
        private bool _isOverlayVisible;

        public void Init(FieldUpgradeBridge fieldUpgradeBridge, LocationBridge locationBridge)
        {
            _fieldUpgradeBridge = fieldUpgradeBridge;
            _locationBridge = locationBridge;
            _overlayStateInitialized = false;
            _isOverlayVisible = false;

            levelTrackPanel.Init(_locationBridge.RequestReturnToMenu);
            plinkoFieldPanel.Init(_fieldUpgradeBridge);
            shopPanel.Init(_fieldUpgradeBridge);
            nextLevelPanel.Init(_locationBridge);
            replacementPanel.Init(_fieldUpgradeBridge);
            selectionOverlayVisibility.HideImmediate();
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            if (!isVisible)
            {
                _overlayStateInitialized = false;
                _isOverlayVisible = false;
                selectionOverlayVisibility.HideImmediate();
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
                _overlayStateInitialized = false;
                _isOverlayVisible = false;
                selectionOverlayVisibility.HideImmediate();
            }

            root.SetActive(isVisible);
            if (isVisible)
            {
                ApplyViewData();
            }
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
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
            ApplyOverlayState(_viewData.IsSelectionOverlayActive);
            plinkoFieldPanel.Refresh(_viewData);
            levelTrackPanel.Refresh(_viewData);
            shopPanel.Refresh(_viewData);
            nextLevelPanel.Refresh(_viewData);
            replacementPanel.Refresh(_viewData);
        }

        private void ResetVisualState()
        {
            _overlayStateInitialized = false;
            _isOverlayVisible = false;
            plinkoFieldPanel.ResetState();
            levelTrackPanel.ResetState();
            shopPanel.ResetState();
            nextLevelPanel.ResetState();
        }

        private void ApplyOverlayState(bool isVisible)
        {
            if (!_overlayStateInitialized)
            {
                _overlayStateInitialized = true;
                _isOverlayVisible = isVisible;
                if (isVisible)
                {
                    selectionOverlayVisibility.ShowImmediate();
                }
                else
                {
                    selectionOverlayVisibility.HideImmediate();
                }

                return;
            }

            if (_isOverlayVisible == isVisible)
            {
                return;
            }

            _isOverlayVisible = isVisible;
            if (isVisible)
            {
                selectionOverlayVisibility.ShowAnimated();
            }
            else
            {
                selectionOverlayVisibility.HideAnimated();
            }
        }
    }
}
