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
        [SerializeField] private PowerLineBattleWorldPresenter worldPresenter;
        [SerializeField] private PowerLineBattleTurnPanelController turnPanel;

        private PowerLineBattleHudViewData _viewData = new();
        private bool _isVisible;
        private bool _isVictorySequencePlaying;
        private string _playedVictorySequenceLevelKey = string.Empty;
        private BattleBridge _battleBridge;

        public void Init(BattleBridge battleBridge)
        {
            _battleBridge = battleBridge;
            turnPanel.Init(battleBridge);
            boardPanel.Init(HandleLaneClicked);
            boardPanel.BindWorldPresenter(worldPresenter);
            turnPanel.SetLaneSelectionHandler(HandleSelectedCardChanged);
            worldPresenter.SetEnemySelectionHandler(turnPanel.SetSelectedEnemy);
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
            worldPresenter.SetVisible(isVisible);
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
            worldPresenter.SetVisible(isVisible);
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
            _isVictorySequencePlaying = false;
            _playedVictorySequenceLevelKey = string.Empty;
            levelTrackPanel.ResetState();
            boardPanel.ResetState();
            worldPresenter.ResetState();
            turnPanel.ResetState();
        }

        private void ApplyViewData()
        {
            levelTrackPanel.Refresh(_viewData);
            boardPanel.Refresh(_viewData);
            worldPresenter.Refresh(_viewData);
            turnPanel.Refresh(_viewData);
            boardPanel.SetSelectedCard(turnPanel.SelectedCard, _viewData.CurrentMana);
            worldPresenter.SetSelectedCard(turnPanel.SelectedCard, _viewData.CurrentMana);
            TryStartVictorySequence();
        }

        private void HandleSelectedCardChanged(HandCardViewData selectedCard)
        {
            boardPanel.SetSelectedCard(selectedCard, _viewData.CurrentMana);
            worldPresenter.SetSelectedCard(selectedCard, _viewData.CurrentMana);
        }

        private void HandleLaneClicked(int laneIndex)
        {
            if (!turnPanel.TryDeploySelectedCard(laneIndex))
            {
                return;
            }

            boardPanel.SetSelectedCard(null, _viewData.CurrentMana);
            worldPresenter.SetSelectedCard(null, _viewData.CurrentMana);
        }

        private void TryStartVictorySequence()
        {
            if (!_viewData.IsVictorySequencePending ||
                _isVictorySequencePlaying ||
                _playedVictorySequenceLevelKey == _viewData.LevelKey)
            {
                return;
            }

            _isVictorySequencePlaying = true;
            _playedVictorySequenceLevelKey = _viewData.LevelKey;
            worldPresenter.PlayVictorySequence(
                boardPanel.PlayVictoryBanner,
                () =>
                {
                    _isVictorySequencePlaying = false;
                    _battleBridge?.RequestFinalizePowerLineBattleResult();
                });
        }
    }
}
