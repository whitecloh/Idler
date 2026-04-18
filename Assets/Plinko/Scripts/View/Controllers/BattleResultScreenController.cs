using System;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleResultScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        private LocationBridge _locationBridge;
        private BattleBridge _battleBridge;
        private BattleResultViewData _viewData = new();
        private bool _isVisible;

        public void Init(LocationBridge locationBridge, BattleBridge battleBridge)
        {
            _locationBridge = locationBridge;
            _battleBridge = battleBridge;
        }

        public void Show(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            _isVisible = isVisible;
            root.SetActive(isVisible);
        }

        public void Refresh(BattleResultViewData viewData)
        {
            _viewData = viewData;
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            var width = 440f;
            var height = 280f;
            var area = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(area, GUI.skin.window);
            GUILayout.Label(string.IsNullOrWhiteSpace(_viewData.Title) ? "Battle Result" : _viewData.Title);
            GUILayout.Space(8f);

            if (!string.IsNullOrWhiteSpace(_viewData.Description))
            {
                GUILayout.Label(_viewData.Description);
                GUILayout.Space(6f);
            }

            GUILayout.Label($"Player Base: {_viewData.PlayerBaseHealthAfter}");
            GUILayout.Label($"Enemy Base: {_viewData.EnemyBaseHealthAfter}");

            if (!string.IsNullOrWhiteSpace(_viewData.RewardText))
            {
                GUILayout.Space(6f);
                GUILayout.Label(_viewData.RewardText);
            }

            if (!string.IsNullOrWhiteSpace(_viewData.RewardBreakdownText))
            {
                GUILayout.Label(_viewData.RewardBreakdownText);
            }

            GUILayout.FlexibleSpace();
            using (new GuiEnabledScope(_viewData.CanAdvance || _viewData.CanReturnToMenu))
            {
                if (GUILayout.Button(GetPrimaryActionLabel(), GUILayout.Height(34f)))
                {
                    if (_viewData.CanAdvance)
                    {
                        _locationBridge.RequestAdvanceToNextLevel();
                    }
                    else if (_viewData.CanReturnToMenu)
                    {
                        _battleBridge.RequestReturnToMenu();
                    }
                }
            }

            GUILayout.EndArea();
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
