using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BaseDefenseScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private BaseDefenseBoardPanelController boardPanel;
        [SerializeField] private BaseDefenseTurnPanelController turnPanel;

        private DefenceBattleHudViewData _viewData = new();
        private bool _isVisible;

        public void Init(BattleBridge battleBridge)
        {
            levelTrackPanel.Init(battleBridge.RequestReturnToMenu);
            turnPanel.Init(battleBridge);
            boardPanel.Init(HandleBoardCellClicked);
            turnPanel.SetBoardSelectionHandler(HandleSelectedCardChanged);
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

        public void Refresh(DefenceBattleHudViewData viewData)
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
            boardPanel.SetSelectedCard(turnPanel.SelectedCard, _viewData.CanDeploy, _viewData.CurrentMana);
        }

        private void HandleSelectedCardChanged(HandCardViewData selectedCard)
        {
            boardPanel.SetSelectedCard(selectedCard, _viewData.CanDeploy, _viewData.CurrentMana);
        }

        private void HandleBoardCellClicked(int laneIndex, int cellIndex)
        {
            if (!turnPanel.TryDeploySelectedCard(laneIndex, cellIndex))
            {
                return;
            }

            boardPanel.ClearSelectedCard();
        }
    }
}
