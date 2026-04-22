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
            SetFieldUiVisible(true);
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

            ApplyBoardFeedback();
            RefreshLaneStates();
        }

        public void SetSelectedCard(HandCardViewData selectedCard, int currentMana)
        {
            _selectedCard = selectedCard;
            _currentMana = currentMana;
            RefreshLaneStates();
        }

        public void PlayVictoryBanner()
        {
            titleBannerView.ShowText("SVYAZ!");
        }

        public void PrepareVictorySequence()
        {
            SetFieldUiVisible(false);
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

        private void SetFieldUiVisible(bool isVisible)
        {
            if (playerBaseView != null)
            {
                playerBaseView.gameObject.SetActive(isVisible);
            }

            if (enemyBaseView != null)
            {
                enemyBaseView.gameObject.SetActive(isVisible);
            }

            if (laneViews == null)
            {
                return;
            }

            for (var index = 0; index < laneViews.Length; index++)
            {
                var laneView = laneViews[index];
                if (laneView == null || laneView.Root == null)
                {
                    continue;
                }

                laneView.Root.gameObject.SetActive(isVisible);
            }
        }

        private void ApplyBoardFeedback()
        {
            if (boardViewport == null || _viewData.DamageEvents == null)
            {
                return;
            }

            for (var index = 0; index < _viewData.DamageEvents.Count; index++)
            {
                if (_viewData.DamageEvents[index].TargetIsBase)
                {
                    UiAnimationManager.Instance.PlaySpringPunch(boardViewport.rectTransform, 0.35f);
                    break;
                }
            }
        }

        private void Update()
        {
            if (_selectedCard != null || _viewData.IsInteractionLocked || boardViewport == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleScreenClick(Input.mousePosition);
            }

            if (Input.touchCount <= 0)
            {
                return;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleScreenClick(touch.position);
            }
        }

        private void HandleScreenClick(Vector2 screenPosition)
        {
            if (_worldPresenter == null || boardViewport == null)
            {
                return;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(boardViewport.rectTransform, screenPosition, null))
            {
                return;
            }

            _worldPresenter.TrySelectEnemyAtScreenPoint(screenPosition);
        }
    }
}
