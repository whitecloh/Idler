using System;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Bridges;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleResultScreenController : MonoBehaviour, Plinko.Scripts.View.IUiWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private UiCanvasGroupVisibility visibility;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text playerBaseHealthText;
        [SerializeField] private TMP_Text enemyBaseHealthText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text rewardBreakdownText;
        [SerializeField] private Button primaryActionButton;
        [SerializeField] private TMP_Text primaryActionButtonText;

        private LocationBridge _locationBridge;
        private BattleBridge _battleBridge;
        private BattleResultViewData _viewData = new();
        private bool _isVisible;
        private bool _listenersBound;
        private string _lastOutcomeAudioKey = string.Empty;

        public void Init(LocationBridge locationBridge, BattleBridge battleBridge)
        {
            _locationBridge = locationBridge;
            _battleBridge = battleBridge;
            BindListeners();
            ApplyViewData();
        }

        public void Show(bool isVisible)
        {
            if (_isVisible == isVisible)
            {
                return;
            }

            _isVisible = isVisible;
            if (visibility != null)
            {
                if (isVisible)
                {
                    visibility.ShowAnimated();
                    TryPlayOutcomeAudio();
                }
                else
                {
                    visibility.HideAnimated();
                    _lastOutcomeAudioKey = string.Empty;
                }

                return;
            }

            ResolveRoot().SetActive(isVisible);
            if (isVisible)
            {
                TryPlayOutcomeAudio();
            }
            else
            {
                _lastOutcomeAudioKey = string.Empty;
            }
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            _isVisible = isVisible;
            if (visibility != null)
            {
                if (isVisible)
                {
                    visibility.ShowImmediate();
                }
                else
                {
                    visibility.HideImmediate();
                    _lastOutcomeAudioKey = string.Empty;
                }

                return;
            }

            ResolveRoot().SetActive(isVisible);
            if (isVisible)
            {
                TryPlayOutcomeAudio();
            }
            else
            {
                _lastOutcomeAudioKey = string.Empty;
            }
        }

        public void Refresh(BattleResultViewData viewData)
        {
            _viewData = viewData;
            ApplyViewData();
            if (_isVisible)
            {
                TryPlayOutcomeAudio();
            }
        }

        private string GetPrimaryActionLabel()
        {
            if (!string.IsNullOrWhiteSpace(_viewData.PrimaryActionLabel))
            {
                return _viewData.PrimaryActionLabel;
            }

            if (_viewData.CanAdvance)
            {
                return "Next Level";
            }

            if (_viewData.CanReturnToMenu)
            {
                return "Return to Menu";
            }

            return "Close";
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            primaryActionButton.onClick.AddListener(OnPrimaryActionClicked);
            _listenersBound = true;
        }

        private void ApplyViewData()
        {
            titleText.text = string.IsNullOrWhiteSpace(_viewData.Title) ? "Battle Result" : _viewData.Title;
            descriptionText.text = _viewData.Description ?? string.Empty;
            descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(_viewData.Description));

            playerBaseHealthText.text = _viewData.PlayerBaseHealthAfter.ToString();
            enemyBaseHealthText.text = _viewData.EnemyBaseHealthAfter.ToString();

            rewardText.text = _viewData.RewardText ?? string.Empty;
            rewardText.gameObject.SetActive(!string.IsNullOrWhiteSpace(_viewData.RewardText));

            rewardBreakdownText.text = _viewData.RewardBreakdownText ?? string.Empty;
            rewardBreakdownText.gameObject.SetActive(!string.IsNullOrWhiteSpace(_viewData.RewardBreakdownText));

            primaryActionButtonText.text = GetPrimaryActionLabel();
            primaryActionButton.interactable = _viewData.CanAdvance || _viewData.CanReturnToMenu;
        }

        private void OnPrimaryActionClicked()
        {
            UiAnimationManager.Instance.PlaySpringPunch(primaryActionButton.transform as RectTransform);
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);

            if (_viewData.CanAdvance)
            {
                _locationBridge.RequestAdvanceToNextLevel();
                return;
            }

            if (_viewData.CanReturnToMenu)
            {
                _battleBridge.RequestReturnToMenu();
            }
        }

        private GameObject ResolveRoot()
        {
            return root;
        }

        private void TryPlayOutcomeAudio()
        {
            if (!_viewData.IsVictory && !_viewData.IsDefeat)
            {
                return;
            }

            var audioKey = $"{_viewData.IsVictory}:{_viewData.IsDefeat}:{_viewData.IsRunCompleted}:{_viewData.PlayerBaseHealthAfter}:{_viewData.EnemyBaseHealthAfter}";
            if (_lastOutcomeAudioKey == audioKey)
            {
                return;
            }

            _lastOutcomeAudioKey = audioKey;
            AudioManager.Instance?.Play(_viewData.IsDefeat ? GameAudioCueType.Defeat : GameAudioCueType.Victory);
        }
    }
}
