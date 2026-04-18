using System;
using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Widgets
{
    public sealed class MenuPlinkoPreviewController : MonoBehaviour
    {
        [Serializable]
        private sealed class PreviewRow
        {
            [SerializeField] private RectTransform[] anchors;

            public RectTransform GetRandomAnchor()
            {
                if (anchors == null || anchors.Length == 0)
                {
                    return null;
                }

                return anchors[UnityEngine.Random.Range(0, anchors.Length)];
            }
        }

        [Header("Token Setup")]
        [SerializeField] private RectTransform tokenContainer;
        [SerializeField] private MenuPlinkoPreviewTokenView tokenPrefab;
        [SerializeField] private RectTransform spawnAnchor;
        [SerializeField] private RectTransform exitAnchor;
        [SerializeField] private PreviewRow[] rows;

        [Header("Controls")]
        [SerializeField] private Button speedButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private TMP_Text speedLabel;
        [SerializeField] private TMP_Text pauseLabel;

        [Header("Timing")]
        [SerializeField] private float spawnInterval = 1.4f;
        [SerializeField] private float[] speedMultipliers = { 1f, 2f, 4f };
        [SerializeField] private float rowDurationMin = 0.22f;
        [SerializeField] private float rowDurationMax = 0.34f;
        [SerializeField] private float exitDuration = 0.42f;

        [Header("Motion")]
        [SerializeField] private Vector2 spawnJitter = new(80f, 0f);
        [SerializeField] private Vector2 rowJitter = new(28f, 0f);
        [SerializeField] private Vector2 exitJitter = new(18f, 0f);
        [SerializeField] private Vector3 punchScale = new(0.06f, 0.06f, 0f);

        [Header("Visuals")]
        [SerializeField] private Color[] tokenColors =
        {
            new(0.93f, 0.70f, 0.37f, 1f),
            new(0.70f, 0.85f, 0.94f, 1f),
            new(0.56f, 0.90f, 0.66f, 1f),
            new(0.93f, 0.56f, 0.42f, 1f)
        };

        private readonly List<Sequence> _activeSequences = new();
        private readonly List<MenuPlinkoPreviewTokenView> _tokenPool = new();
        private bool _listenersBound;
        private bool _menuVisible = true;
        private bool _isPaused;
        private float _spawnTimer;
        private int _speedIndex;

        public void Initialize()
        {
            BindListeners();
            UpdateButtonLabels();
        }

        public void SetMenuVisible(bool isVisible)
        {
            _menuVisible = isVisible;
            _spawnTimer = 0f;

            if (!isVisible)
            {
                PauseActiveSequences();
                return;
            }

            if (!_isPaused)
            {
                ResumeActiveSequences();
            }
        }

        public void CycleSpeed()
        {
            if (speedMultipliers == null || speedMultipliers.Length == 0)
            {
                return;
            }

            _speedIndex = (_speedIndex + 1) % speedMultipliers.Length;
            var multiplier = GetCurrentSpeedMultiplier();
            for (var index = _activeSequences.Count - 1; index >= 0; index--)
            {
                var sequence = _activeSequences[index];
                if (sequence == null || !sequence.IsActive())
                {
                    _activeSequences.RemoveAt(index);
                    continue;
                }

                sequence.timeScale = multiplier;
            }

            if (speedButton != null)
            {
                UiAnimationManager.Instance.PlaySpringPunch(speedButton.transform as RectTransform);
            }
            UpdateButtonLabels();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                PauseActiveSequences();
            }
            else if (_menuVisible)
            {
                ResumeActiveSequences();
            }

            if (pauseButton != null)
            {
                UiAnimationManager.Instance.PlaySpringPunch(pauseButton.transform as RectTransform);
            }
            UpdateButtonLabels();
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_menuVisible || _isPaused || tokenContainer == null || tokenPrefab == null || spawnAnchor == null || exitAnchor == null || rows == null || rows.Length == 0)
            {
                return;
            }

            _spawnTimer += Time.unscaledDeltaTime * GetCurrentSpeedMultiplier();
            if (_spawnTimer < spawnInterval)
            {
                return;
            }

            _spawnTimer = 0f;
            SpawnToken();
        }

        private void OnDestroy()
        {
            for (var index = _activeSequences.Count - 1; index >= 0; index--)
            {
                _activeSequences[index]?.Kill();
            }
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            if (speedButton != null)
            {
                speedButton.onClick.AddListener(CycleSpeed);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(TogglePause);
            }

            _listenersBound = true;
        }

        private void SpawnToken()
        {
            var token = GetToken();
            if (token == null)
            {
                return;
            }

            if (tokenColors != null && tokenColors.Length > 0)
            {
                token.SetColor(tokenColors[UnityEngine.Random.Range(0, tokenColors.Length)]);
            }

            var tokenRect = token.RectTransform;
            token.gameObject.SetActive(true);
            tokenRect.localScale = Vector3.one;
            tokenRect.anchoredPosition = ToContainerPoint(spawnAnchor) + RandomOffset(spawnJitter);

            Sequence sequence = null;
            sequence = DOTween.Sequence().SetUpdate(true);

            for (var index = 0; index < rows.Length; index++)
            {
                var anchor = rows[index]?.GetRandomAnchor();
                if (anchor == null)
                {
                    continue;
                }

                var nextPosition = ToContainerPoint(anchor) + RandomOffset(rowJitter);
                var segmentDuration = UnityEngine.Random.Range(rowDurationMin, rowDurationMax);
                sequence.Append(tokenRect.DOAnchorPos(nextPosition, segmentDuration).SetEase(Ease.InOutSine));
                sequence.Join(tokenRect.DOPunchScale(punchScale, segmentDuration * 0.8f, 4, 0.7f));
            }

            sequence.Append(tokenRect.DOAnchorPos(ToContainerPoint(exitAnchor) + RandomOffset(exitJitter), exitDuration).SetEase(Ease.InQuad));
            sequence.timeScale = GetCurrentSpeedMultiplier();
            if (_isPaused || !_menuVisible)
            {
                sequence.Pause();
            }

            sequence.OnKill(() =>
            {
                token.gameObject.SetActive(false);
                _activeSequences.Remove(sequence);
            });
            sequence.OnComplete(() => token.gameObject.SetActive(false));
            _activeSequences.Add(sequence);
        }

        private MenuPlinkoPreviewTokenView GetToken()
        {
            for (var index = 0; index < _tokenPool.Count; index++)
            {
                if (_tokenPool[index] != null && !_tokenPool[index].gameObject.activeSelf)
                {
                    return _tokenPool[index];
                }
            }

            if (tokenPrefab == null || tokenContainer == null)
            {
                return null;
            }

            var instance = Instantiate(tokenPrefab, tokenContainer);
            instance.gameObject.SetActive(false);
            _tokenPool.Add(instance);
            return instance;
        }

        private void PauseActiveSequences()
        {
            for (var index = _activeSequences.Count - 1; index >= 0; index--)
            {
                var sequence = _activeSequences[index];
                if (sequence == null || !sequence.IsActive())
                {
                    _activeSequences.RemoveAt(index);
                    continue;
                }

                sequence.Pause();
            }
        }

        private void ResumeActiveSequences()
        {
            var multiplier = GetCurrentSpeedMultiplier();
            for (var index = _activeSequences.Count - 1; index >= 0; index--)
            {
                var sequence = _activeSequences[index];
                if (sequence == null || !sequence.IsActive())
                {
                    _activeSequences.RemoveAt(index);
                    continue;
                }

                sequence.timeScale = multiplier;
                sequence.Play();
            }
        }

        private void UpdateButtonLabels()
        {
            if (speedLabel != null)
            {
                speedLabel.text = $"Speed x{GetCurrentSpeedMultiplier():0}";
            }

            if (pauseLabel != null)
            {
                pauseLabel.text = _isPaused ? "Resume" : "Pause";
            }
        }

        private float GetCurrentSpeedMultiplier()
        {
            if (speedMultipliers == null || speedMultipliers.Length == 0)
            {
                return 1f;
            }

            _speedIndex = Mathf.Clamp(_speedIndex, 0, speedMultipliers.Length - 1);
            return Mathf.Max(0.1f, speedMultipliers[_speedIndex]);
        }

        private Vector2 ToContainerPoint(RectTransform source)
        {
            if (source == null || tokenContainer == null)
            {
                return Vector2.zero;
            }

            var worldPoint = source.TransformPoint(source.rect.center);
            var localPoint = tokenContainer.InverseTransformPoint(worldPoint);
            return localPoint;
        }

        private static Vector2 RandomOffset(Vector2 jitter)
        {
            return new Vector2(
                UnityEngine.Random.Range(-jitter.x, jitter.x),
                UnityEngine.Random.Range(-jitter.y, jitter.y));
        }
    }
}
