using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PowerLineBattleScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private PowerLineBattleBoardPanelController boardPanel;
        [SerializeField] private PowerLineBattleTurnPanelController turnPanel;

        private PowerLineBattleHudViewData _viewData = new();
        private bool _isVisible;

        public void Init(BattleBridge battleBridge)
        {
            turnPanel.Init(battleBridge);
            boardPanel.Init(HandleLaneClicked);
            turnPanel.SetLaneSelectionHandler(HandleSelectedCardChanged);
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
            if (!isVisible)
            {
                ResetState();
                return;
            }

            ApplyViewData();
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
            if (!isVisible)
            {
                ResetState();
                return;
            }

            ApplyViewData();
        }

        public void Refresh(PowerLineBattleHudViewData viewData)
        {
            _viewData = viewData;
            if (!_isVisible)
            {
                return;
            }

            ApplyViewData();
        }

        public void ResetState()
        {
            levelTrackPanel.ResetState();
            boardPanel.ResetState();
            turnPanel.ResetState();
        }

        private void ApplyViewData()
        {
            levelTrackPanel.Refresh(_viewData);
            boardPanel.Refresh(_viewData);
            turnPanel.Refresh(_viewData);
            boardPanel.SetSelectedCard(turnPanel.SelectedCard, _viewData.CurrentMana);
        }

        private void HandleSelectedCardChanged(HandCardViewData selectedCard)
        {
            boardPanel.SetSelectedCard(selectedCard, _viewData.CurrentMana);
        }

        private void HandleLaneClicked(int laneIndex)
        {
            if (!turnPanel.TryDeploySelectedCard(laneIndex))
            {
                return;
            }

            boardPanel.SetSelectedCard(null, _viewData.CurrentMana);
        }
    }
}
