using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PurchaseLevelTrackPanelController levelTrackPanel;
        [SerializeField] private BattleBoardPanelController boardPanel;
        [SerializeField] private BattleTurnPanelController turnPanel;

        private BattleBridge _battleBridge;
        private StandardBattleHudViewData _viewData = new();
        private bool _isVisible;

        public void Init(BattleBridge battleBridge)
        {
            _battleBridge = battleBridge;
            turnPanel.Init(_battleBridge, boardPanel);
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            if (!isVisible)
            {
                if (turnPanel != null)
                {
                    turnPanel.ResetState();
                }

                if (boardPanel != null)
                {
                    boardPanel.ResetState();
                }
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
                if (turnPanel != null)
                {
                    turnPanel.ResetState();
                }

                if (boardPanel != null)
                {
                    boardPanel.ResetState();
                }
            }

            root.SetActive(isVisible);
            if (isVisible)
            {
                ApplyViewData();
            }
        }

        public void Refresh(StandardBattleHudViewData viewData)
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
            boardPanel.Refresh(_viewData);
            turnPanel.Refresh(_viewData);
        }

        private void ResetVisualState()
        {
            if (levelTrackPanel != null)
            {
                levelTrackPanel.ResetState();
            }

            if (boardPanel != null)
            {
                boardPanel.ResetState();
            }

            if (turnPanel != null)
            {
                turnPanel.ResetState();
            }

        }
    }
}
