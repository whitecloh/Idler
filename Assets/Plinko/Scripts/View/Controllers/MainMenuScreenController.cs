using System;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class MainMenuScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        private MainMenuBridge _mainMenuBridge;
        private Action _openLocationSelection;
        private MainMenuViewData _viewData = new();
        private bool _isVisible;

        public void Init(MainMenuBridge mainMenuBridge, Action openLocationSelection)
        {
            _mainMenuBridge = mainMenuBridge;
            _openLocationSelection = openLocationSelection;
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        public void Refresh(MainMenuViewData viewData)
        {
            _viewData = viewData ?? new MainMenuViewData();
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            var width = 360f;
            var height = 220f;
            var area = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(area, GUI.skin.window);
            GUILayout.Label("Session");
            GUILayout.Space(8f);

            if (GUILayout.Button("Play", GUILayout.Height(36f)))
            {
                _openLocationSelection?.Invoke();
            }

            GUILayout.Space(8f);
            using (new GuiEnabledScope(_viewData.CanContinue))
            {
                if (GUILayout.Button("Continue", GUILayout.Height(36f)))
                {
                    _mainMenuBridge?.RequestContinueRun();
                }
            }

            GUILayout.Space(8f);
            GUILayout.Label(_viewData.CanContinue
                ? string.IsNullOrWhiteSpace(_viewData.ContinueTitle) ? "Continue available" : _viewData.ContinueTitle
                : "No unfinished run found.");

            if (_viewData.CanContinue && !string.IsNullOrWhiteSpace(_viewData.ContinueSubtitle))
            {
                GUILayout.Label(_viewData.ContinueSubtitle);
            }

            GUILayout.EndArea();
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
