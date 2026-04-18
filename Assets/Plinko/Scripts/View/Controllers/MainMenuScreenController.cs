using System;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Bridges;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Plinko.Scripts.View.Controllers
{
    public sealed class MainMenuScreenController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private UiCanvasGroupVisibility visibility;

        [Header("Actions")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button exitButton;

        [Header("Audio")]
        [SerializeField] private Toggle audioToggle;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeValueText;

        private MainMenuBridge _mainMenuBridge;
        private Action _openLocationSelection;
        private MainMenuViewData _viewData = new();
        private bool _isVisible;
        private bool _listenersBound;
        private bool _suppressAudioCallbacks;

        public void Init(MainMenuBridge mainMenuBridge, Action openLocationSelection)
        {
            _mainMenuBridge = mainMenuBridge;
            _openLocationSelection = openLocationSelection;

            BindListeners();

            AudioSettingsStore.Apply();
            ApplyAudioState();
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
                    AudioSettingsStore.Apply();
                    ApplyAudioState();
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

            if (isVisible)
            {
                AudioSettingsStore.Apply();
                ApplyAudioState();
            }
        }

        public void Refresh(MainMenuViewData viewData)
        {
            _viewData = viewData;
            
            ApplyViewData();
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
            RegisterAnimatedClick(continueButton, OnContinueClicked);
            RegisterAnimatedClick(exitButton, OnExitClicked);

            audioToggle.onValueChanged.AddListener(OnAudioToggleChanged);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            _listenersBound = true;
        }

        private void ApplyViewData()
        {
            continueButton.interactable = _viewData.CanContinue;
        }

        private void ApplyAudioState()
        {
            _suppressAudioCallbacks = true;

            audioToggle.isOn = !AudioSettingsStore.IsMuted;
            volumeSlider.value = AudioSettingsStore.Volume;
            volumeValueText.text = $"{Mathf.RoundToInt(AudioSettingsStore.Volume * 100f)}%";

            _suppressAudioCallbacks = false;
        }

        private void OnStartClicked()
        { 
            _openLocationSelection.Invoke();
        }

        private void OnContinueClicked()
        {
            if (!_viewData.CanContinue)
            {
                return;
            }

            _mainMenuBridge.RequestContinueRun();
        }

        private static void OnExitClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnAudioToggleChanged(bool isEnabled)
        {
            if (_suppressAudioCallbacks)
            {
                return;
            }

            AudioSettingsStore.SetMuted(!isEnabled);
            ApplyAudioState();
        }

        private void OnVolumeChanged(float value)
        {
            if (_suppressAudioCallbacks)
            {
                return;
            }

            AudioSettingsStore.SetVolume(value);
            volumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
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
    }
}
