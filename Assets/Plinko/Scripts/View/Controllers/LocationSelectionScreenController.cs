using System;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class LocationSelectionScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        private MainMenuBridge _mainMenuBridge;
        private Action _returnToMainMenu;
        private LocationSelectionViewData _viewData = new();
        private bool _isVisible;
        private Vector2 _scrollPosition;

        public void Init(MainMenuBridge mainMenuBridge, Action returnToMainMenu)
        {
            _mainMenuBridge = mainMenuBridge;
            _returnToMainMenu = returnToMainMenu;
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        public void Refresh(LocationSelectionViewData viewData)
        {
            _viewData = viewData ?? new LocationSelectionViewData();
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            var width = 560f;
            var height = Mathf.Min(Screen.height - 40f, 520f);
            var area = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(area, GUI.skin.window);
            GUILayout.Label("Select Location");
            GUILayout.Space(8f);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            foreach (var entry in _viewData.Locations)
            {
                DrawLocationEntry(entry);
                GUILayout.Space(10f);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(6f);
            if (GUILayout.Button("Back", GUILayout.Height(32f)))
            {
                _returnToMainMenu?.Invoke();
            }

            GUILayout.EndArea();
        }

        private void DrawLocationEntry(LocationEntryViewData entry)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(entry.DisplayName);
            if (!string.IsNullOrWhiteSpace(entry.StatusText))
            {
                GUILayout.Label(entry.StatusText);
            }

            if (!entry.IsUnlocked && !string.IsNullOrWhiteSpace(entry.UnlockDescription))
            {
                GUILayout.Label(entry.UnlockDescription);
            }

            using (new GuiEnabledScope(entry.IsUnlocked))
            {
                if (GUILayout.Button(entry.IsUnlocked ? "Start" : "Locked", GUILayout.Height(30f)))
                {
                    _mainMenuBridge?.RequestStartNewRun(entry.LocationId);
                }
            }

            GUILayout.EndVertical();
        }

        private readonly struct GuiEnabledScope : IDisposable
        {
            private readonly bool _previousValue;

            public GuiEnabledScope(bool isEnabled)
            {
                _previousValue = GUI.enabled;
                GUI.enabled = isEnabled;
            }

            public void Dispose()
            {
                GUI.enabled = _previousValue;
            }
        }
    }
}
