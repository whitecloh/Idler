using System;
using System.Collections.Generic;

namespace Plinko.Scripts.View
{
    public sealed class UiPopupManager
    {
        public enum PopupId
        {
            None = 0,
            LocationSelection = 1
        }

        private readonly Dictionary<PopupId, IUiWindow> _popups = new();
        private readonly HashSet<PopupId> _openPopups = new();

        public void ClearRegistrations()
        {
            _popups.Clear();
            _openPopups.Clear();
        }

        public void Register(PopupId id, IUiWindow popup)
        {
            _popups[id] = popup;
        }

        public void Open(PopupId id, bool immediate = false)
        {
            SetVisible(id, true, immediate);
            _openPopups.Add(id);
        }

        public void Close(PopupId id, bool immediate = false)
        {
            if (!_popups.ContainsKey(id))
            {
                return;
            }

            SetVisible(id, false, immediate);
            _openPopups.Remove(id);
        }

        public void CloseAll(bool immediate = false)
        {
            foreach (var popup in _popups)
            {
                SetVisible(popup.Value, false, immediate);
            }

            _openPopups.Clear();
        }

        private void SetVisible(PopupId id, bool isVisible, bool immediate)
        {
            if (!_popups.TryGetValue(id, out var popup))
            {
                throw new InvalidOperationException($"Popup '{id}' is not registered.");
            }

            SetVisible(popup, isVisible, immediate);
        }

        private static void SetVisible(IUiWindow popup, bool isVisible, bool immediate)
        {
            if (immediate)
            {
                popup.SetVisibleImmediate(isVisible);
                return;
            }

            popup.Show(isVisible);
        }
    }
}
