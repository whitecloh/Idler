using System;
using System.Collections.Generic;
using System.Text;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Bridges;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class LocationSelectionScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private UiCanvasGroupVisibility visibility;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private LocationSelectionCardView locationCardPrefab;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        private MainMenuBridge _mainMenuBridge;
        private Action _returnToMainMenu;
        private LocationSelectionViewData _viewData = new();
        private readonly List<LocationSelectionCardView> _spawnedCards = new();
        private string _selectedLocationId;
        private string _viewSignature = string.Empty;
        private bool _isVisible;
        private bool _listenersBound;

        public void Init(MainMenuBridge mainMenuBridge, Action returnToMainMenu)
        {
            _mainMenuBridge = mainMenuBridge;
            _returnToMainMenu = returnToMainMenu;
            BindListeners();
        }

        public void Show(bool isVisible)
        {
            if (_isVisible == isVisible)
            {
                return;
            }

            _isVisible = isVisible;
            if (isVisible)
            {
                SelectDefaultLocationIfNeeded(force: true);
                ApplySelectionState();
            }

            if (visibility != null)
            {
                if (isVisible)
                {
                    visibility.ShowAnimated();
                }
                else
                {
                    visibility.HideAnimated();
                }

                return;
            }

            var target = ResolveRoot();
            target.SetActive(isVisible);
        }

        public void Refresh(LocationSelectionViewData viewData)
        {
            _viewData = viewData;
            
            var signature = BuildViewSignature(_viewData);
            if (_viewSignature != signature)
            {
                _viewSignature = signature;
                RebuildCards();
            }

            SelectDefaultLocationIfNeeded(force: false);
            ApplySelectionState();
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            _isVisible = isVisible;
            
            var target = ResolveRoot();
            
            if (visibility != null)
            {
                if (isVisible)
                {
                    visibility.ShowImmediate();
                }
                else
                {
                    visibility.HideImmediate();
                }

                return;
            }

            target.SetActive(isVisible);
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            RegisterAnimatedClick(startButton, OnStartClicked);
            RegisterAnimatedClick(backButton, OnBackClicked);
            _listenersBound = true;
        }

        private void RebuildCards()
        {
            for (var index = 0; index < _spawnedCards.Count; index++)
            {
                if (_spawnedCards[index] != null)
                {
                    Destroy(_spawnedCards[index].gameObject);
                }
            }

            _spawnedCards.Clear();
            if (_viewData.Locations == null)
            {
                return;
            }

            for (var index = 0; index < _viewData.Locations.Count; index++)
            {
                var entry = _viewData.Locations[index];
                if (entry == null)
                {
                    continue;
                }

                var card = Instantiate(locationCardPrefab, contentRoot);
                card.Bind(entry, OnLocationSelected);
                _spawnedCards.Add(card);
            }
        }

        private void OnLocationSelected(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return;
            }

            _selectedLocationId = locationId;
            ApplySelectionState();
        }

        private void SelectDefaultLocationIfNeeded(bool force)
        {
            if (_viewData?.Locations == null || _viewData.Locations.Count == 0)
            {
                _selectedLocationId = null;
                return;
            }

            if (!force && !string.IsNullOrWhiteSpace(_selectedLocationId) && TryGetSelectedEntry(out _))
            {
                return;
            }

            for (var index = _viewData.Locations.Count - 1; index >= 0; index--)
            {
                var entry = _viewData.Locations[index];
                if (entry != null && entry.IsUnlocked)
                {
                    _selectedLocationId = entry.LocationId;
                    return;
                }
            }

            _selectedLocationId = null;
        }

        private void ApplySelectionState()
        {
            for (var index = 0; index < _spawnedCards.Count; index++)
            {
                var card = _spawnedCards[index];
                if (card == null)
                {
                    continue;
                }

                card.RefreshSelection(card.LocationId == _selectedLocationId);
            }

            if (TryGetSelectedEntry(out var selectedEntry))
            {
                startButton.interactable = selectedEntry.IsUnlocked;
            }
            else
            {
                startButton.interactable = false;
            }
        }

        private bool TryGetSelectedEntry(out LocationEntryViewData entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(_selectedLocationId) || _viewData?.Locations == null)
            {
                return false;
            }

            for (var index = 0; index < _viewData.Locations.Count; index++)
            {
                var candidate = _viewData.Locations[index];
                if (candidate != null && candidate.LocationId == _selectedLocationId)
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        private void OnStartClicked()
        {
            if (!TryGetSelectedEntry(out var selectedEntry) || !selectedEntry.IsUnlocked)
            {
                return;
            }

            _mainMenuBridge.RequestStartNewRun(selectedEntry.LocationId);
        }

        private void OnBackClicked()
        {
            _returnToMainMenu.Invoke();
        }

        private static void RegisterAnimatedClick(Button button, Action callback)
        {
            button.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(button.transform as RectTransform);
                callback.Invoke();
            });
        }

        private GameObject ResolveRoot()
        {
            return root;
        }

        private static string BuildViewSignature(LocationSelectionViewData viewData)
        {
            if (viewData?.Locations == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (var index = 0; index < viewData.Locations.Count; index++)
            {
                var entry = viewData.Locations[index];
                if (entry == null)
                {
                    continue;
                }

                builder.Append(entry.LocationId)
                    .Append('|')
                    .Append(entry.DisplayName)
                    .Append('|')
                    .Append(entry.IsUnlocked)
                    .Append('|')
                    .Append(entry.IsCompleted)
                    .Append('|')
                    .Append(entry.MaxCompletedLevelIndex)
                    .Append('|')
                    .Append(entry.TotalLevelCount)
                    .Append('|')
                    .Append(entry.StatusText)
                    .Append('|')
                    .Append(entry.UnlockDescription)
                    .Append('|')
                    .Append(entry.Art != null ? entry.Art.GetInstanceID() : 0)
                    .Append(';');
            }

            return builder.ToString();
        }
    }
}
