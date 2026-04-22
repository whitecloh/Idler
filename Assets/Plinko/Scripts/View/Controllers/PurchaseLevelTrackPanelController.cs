using System;
using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchaseLevelTrackPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text locationTitleText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private PurchaseLevelProgressItemView itemPrefab;
        [SerializeField] private Button exitToMenuButton;
        [SerializeField] private Button pauseButton;

        private readonly List<PurchaseLevelProgressItemView> _items = new();
        private string _levelKey = string.Empty;
        private Action _returnToMenuAction;
        private bool _listenersBound;
        private bool _isPaused;

        public void Init(Action returnToMenuAction)
        {
            _returnToMenuAction = returnToMenuAction;
            BindListeners();
        }

        public void ResetState()
        {
            SetPaused(false);
            _levelKey = string.Empty;
            for (var index = 0; index < _items.Count; index++)
            {
                Destroy(_items[index].gameObject);
            }

            _items.Clear();
        }

        private void OnDisable()
        {
            SetPaused(false);
        }

        private void OnDestroy()
        {
            SetPaused(false);
        }

        public void Refresh(PurchasePhaseViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        public void Refresh(RetrainingPhaseViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        public void Refresh(StandardBattleHudViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        public void Refresh(DefenceBattleHudViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        public void Refresh(PowerLineBattleHudViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        public void Refresh(SignalPurchasePhaseViewData viewData)
        {
            locationTitleText.text = viewData.LocationDisplayName;

            if (_levelKey != viewData.LevelKey)
            {
                _levelKey = viewData.LevelKey;
                Rebuild(viewData.Levels);
            }

            Apply(viewData.Levels);
            CenterCurrentItem(viewData.Levels);
        }

        private void Rebuild(IReadOnlyList<PurchaseLevelProgressEntryViewData> levels)
        {
            for (var index = 0; index < _items.Count; index++)
            {
                Destroy(_items[index].gameObject);
            }

            _items.Clear();
            for (var index = 0; index < levels.Count; index++)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                _items.Add(item);
            }
        }

        private void Apply(IReadOnlyList<PurchaseLevelProgressEntryViewData> levels)
        {
            for (var index = 0; index < _items.Count && index < levels.Count; index++)
            {
                _items[index].Refresh(levels[index]);
            }
        }

        private void CenterCurrentItem(IReadOnlyList<PurchaseLevelProgressEntryViewData> levels)
        {
            if (levels == null || levels.Count == 0 || scrollRect.viewport == null)
            {
                return;
            }

            var currentIndex = 0;
            for (var index = 0; index < levels.Count; index++)
            {
                if (levels[index].IsCurrent)
                {
                    currentIndex = index;
                    break;
                }
            }

            Canvas.ForceUpdateCanvases();

            var contentWidth = contentRoot.rect.width;
            var viewportWidth = scrollRect.viewport.rect.width;
            var itemRect = (RectTransform)_items[currentIndex].transform;
            var itemCenter = itemRect.anchoredPosition.x + itemRect.rect.width * 0.5f;
            var targetOffset = Mathf.Clamp(itemCenter - viewportWidth * 0.5f, 0f, Mathf.Max(0f, contentWidth - viewportWidth));
            scrollRect.horizontalNormalizedPosition = contentWidth <= viewportWidth
                ? 0f
                : Mathf.Clamp01(targetOffset / (contentWidth - viewportWidth));
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            if (exitToMenuButton != null)
            {
                exitToMenuButton.onClick.AddListener(HandleExitToMenuClicked);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(HandlePauseClicked);
            }

            _listenersBound = true;
        }

        private void HandleExitToMenuClicked()
        {
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
            SetPaused(false);
            _returnToMenuAction?.Invoke();
        }

        private void HandlePauseClicked()
        {
            AudioManager.Instance?.Play(GameAudioCueType.ButtonClick);
            SetPaused(!_isPaused);
        }

        private void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }
}
