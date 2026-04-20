using System.Collections;
using Plinko.Scripts.View.Audio;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plinko.Scripts.View
{
    public sealed class UiWindowManager : MonoBehaviour
    {
        public enum WindowId
        {
            None = 0,
            MainMenu = 1,
            Purchase = 2,
            SignalPurchase = 3,
            Retraining = 4,
            FieldUpgrade = 5,
            StandardBattle = 6,
            DefenceBattle = 7,
            PowerLineBattle = 8,
            BattleResult = 9
        }

        [SerializeField] private UiLoadingWindow loadingWindow;
        [FormerlySerializedAs("transitionDuration")]
        [SerializeField] private float loadingMinShowDuration = 1f;

        private readonly System.Collections.Generic.Dictionary<WindowId, IUiWindow> _windows = new();
        private WindowId currentWindow = WindowId.None;
        private WindowId pendingWindow = WindowId.None;
        private Coroutine transitionRoutine;

        public void ClearRegistrations()
        {
            _windows.Clear();
        }

        public void Register(WindowId id, IUiWindow window)
        {
            _windows[id] = window;
        }

        public void OpenImmediate(WindowId targetWindow)
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            if (loadingWindow != null)
            {
                loadingWindow.HideImmediate();
            }

            pendingWindow = WindowId.None;
            currentWindow = targetWindow;
            ApplyPrimaryWindow(targetWindow, true);
            PlayOpenWindowSound(targetWindow);
        }

        public void Open(WindowId targetWindow)
        {
            if (currentWindow == targetWindow && transitionRoutine == null)
            {
                return;
            }

            if (transitionRoutine != null && pendingWindow == targetWindow)
            {
                return;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
                pendingWindow = WindowId.None;
            }

            if (!isActiveAndEnabled || loadingWindow == null || currentWindow == WindowId.None)
            {
                pendingWindow = WindowId.None;
                currentWindow = targetWindow;
                ApplyPrimaryWindow(targetWindow, false);
                PlayOpenWindowSound(targetWindow);
                return;
            }

            pendingWindow = targetWindow;
            transitionRoutine = StartCoroutine(PlayTransition(targetWindow));
        }

        public void Close(WindowId targetWindow, bool immediate = false)
        {
            if (!_windows.ContainsKey(targetWindow))
            {
                throw new System.InvalidOperationException($"Window '{targetWindow}' is not registered.");
            }

            if (transitionRoutine != null && pendingWindow == targetWindow)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
                pendingWindow = WindowId.None;
                if (loadingWindow != null)
                {
                    if (immediate)
                    {
                        loadingWindow.HideImmediate();
                    }
                    else
                    {
                        loadingWindow.Hide();
                    }
                }
            }

            SetVisible(_windows[targetWindow], false, immediate);
            if (currentWindow == targetWindow)
            {
                currentWindow = WindowId.None;
            }
        }

        public void CloseAll(bool immediate = false)
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
                pendingWindow = WindowId.None;
            }

            if (loadingWindow != null)
            {
                if (immediate)
                {
                    loadingWindow.HideImmediate();
                }
                else
                {
                    loadingWindow.Hide();
                }
            }

            foreach (var window in _windows.Values)
            {
                SetVisible(window, false, immediate);
            }

            currentWindow = WindowId.None;
        }

        private IEnumerator PlayTransition(WindowId targetWindow)
        {
            loadingWindow.Show();
            yield return null;
            yield return new WaitForSecondsRealtime(loadingMinShowDuration);
            currentWindow = targetWindow;
            ApplyPrimaryWindow(targetWindow, false);
            PlayOpenWindowSound(targetWindow);
            loadingWindow.Hide();
            pendingWindow = WindowId.None;
            transitionRoutine = null;
        }

        private void ApplyPrimaryWindow(WindowId targetWindow, bool immediate)
        {
            if (targetWindow != WindowId.None && !_windows.ContainsKey(targetWindow))
            {
                throw new System.InvalidOperationException($"Window '{targetWindow}' is not registered.");
            }

            foreach (var window in _windows)
            {
                SetVisible(window.Value, window.Key == targetWindow, immediate);
            }
        }

        private static void SetVisible(IUiWindow window, bool isVisible, bool immediate)
        {
            if (immediate)
            {
                window.SetVisibleImmediate(isVisible);
                return;
            }

            window.Show(isVisible);
        }

        private static void PlayOpenWindowSound(WindowId targetWindow)
        {
            if (targetWindow == WindowId.None)
            {
                return;
            }

            AudioManager.Instance?.Play(GameAudioCueType.WindowOpen);
        }
    }
}
