using System;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    [ExecuteAlways]
    public sealed class PowerLineBattleBoardPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform overlayRoot;
        [SerializeField] private RawImage boardViewport;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private PowerLinePlayerBaseView playerBaseView;
        [SerializeField] private PowerLineEnemyBaseView enemyBaseView;
        [SerializeField] private PowerLineLaneView[] laneViews;
        [SerializeField] private BattleTurnBannerView titleBannerView;
        [SerializeField] private bool autoPositionLaneViews;
        [SerializeField] private RenderTexture sharedRenderTexture;

        private PowerLineBattleHudViewData _viewData = new();
        private Action<int> _laneClicked;
        private HandCardViewData _selectedCard;
        private int _currentMana;
        private string _presentedLevelKey = string.Empty;
        private PowerLineBattleWorldPresenter _worldPresenter;

        public void Init(Action<int> laneClicked)
        {
            _laneClicked = laneClicked;
            for (var index = 0; index < laneViews.Length; index++)
            {
                laneViews[index].Bind(HandleLaneClicked);
            }
        }

        public void BindWorldPresenter(PowerLineBattleWorldPresenter worldPresenter)
        {
            _worldPresenter = worldPresenter;
            if (_worldPresenter != null && boardViewport != null)
            {
                _worldPresenter.BindViewport(boardViewport.rectTransform);
            }
        }

        public void ResetState()
        {
            _selectedCard = null;
            _currentMana = 0;
            _presentedLevelKey = string.Empty;
            titleBannerView.HideImmediate();
            RefreshLaneStates();
        }

        public void Refresh(PowerLineBattleHudViewData viewData)
        {
            _viewData = viewData;
            EnsureBoardViewport();
            playerBaseView.Refresh(new BattleBaseViewData
            {
                Sprite = null,
                CurrentHealth = viewData.PlayerBase.CurrentHealth,
                MaxHealth = viewData.PlayerBase.MaxHealth
            });
            enemyBaseView.Refresh(null, viewData.ConnectedLaneCount, viewData.RequiredLaneCount, viewData.Lanes);

            if (_presentedLevelKey != viewData.LevelKey && !string.IsNullOrWhiteSpace(viewData.LevelKey))
            {
                _presentedLevelKey = viewData.LevelKey;
                titleBannerView.ShowText(viewData.LevelTitle);
            }

            if (autoPositionLaneViews)
            {
                PositionLaneViews();
            }

            RefreshLaneStates();
        }

        public void SetSelectedCard(HandCardViewData selectedCard, int currentMana)
        {
            _selectedCard = selectedCard;
            _currentMana = currentMana;
            RefreshLaneStates();
        }

        private void HandleLaneClicked(PowerLineLaneView laneView)
        {
            if (_selectedCard == null || _currentMana < _selectedCard.ManaCost || _viewData.IsInteractionLocked)
            {
                return;
            }

            _laneClicked?.Invoke((int)laneView.Lane);
        }

        private void PositionLaneViews()
        {
            if (_worldPresenter == null || boardViewport == null)
            {
                return;
            }

            for (var index = 0; index < laneViews.Length; index++)
            {
                if (!_worldPresenter.TryGetLaneSpawnWorldPosition(laneViews[index].Lane, out var worldPosition))
                {
                    continue;
                }

                laneViews[index].Root.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPositionInViewport(
                    overlayRoot,
                    boardViewport.rectTransform,
                    worldCamera,
                    worldPosition);
            }
        }

        private void RefreshLaneStates()
        {
            for (var index = 0; index < laneViews.Length; index++)
            {
                var laneData = FindLaneData(laneViews[index].Lane);
                var isAvailable = laneData != null &&
                                  laneData.IsSpawnAvailable &&
                                  _selectedCard != null &&
                                  _currentMana >= _selectedCard.ManaCost &&
                                  !_viewData.IsInteractionLocked;
                var isSelected = isAvailable;
                var isConnected = laneData != null && laneData.IsConnected;
                var isDisabled = laneData == null || laneData.IsConnected;
                laneViews[index].SetState(isSelected, isAvailable, isConnected, isDisabled);
            }
        }

        private PowerLineLaneViewData FindLaneData(Enums.PowerLineLane lane)
        {
            for (var index = 0; index < _viewData.Lanes.Count; index++)
            {
                if (_viewData.Lanes[index].Lane == lane)
                {
                    return _viewData.Lanes[index];
                }
            }

            return null;
        }

        private void EnsureBoardViewport()
        {
            if (boardViewport == null || worldCamera == null)
            {
                return;
            }

            if (worldCamera.targetTexture != sharedRenderTexture)
            {
                worldCamera.targetTexture = sharedRenderTexture;
            }

            if (boardViewport.texture != sharedRenderTexture)
            {
                boardViewport.texture = sharedRenderTexture;
            }

            if (_worldPresenter != null)
            {
                _worldPresenter.BindViewport(boardViewport.rectTransform);
            }
        }
    }
}
