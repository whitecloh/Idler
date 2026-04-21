using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using CanvasGroup = UnityEngine.CanvasGroup;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiAnimationManager : MonoBehaviour
    {
        private const string FeedbackScaleChannel = "feedback-scale";
        private const string FeedbackRotationChannel = "feedback-rotation";
        private const string FeedbackPositionChannel = "feedback-position";

        public static UiAnimationManager Instance { get; private set; }

        [Header("Punch")]
        [SerializeField] private float punchScale = 0.15f;
        [SerializeField] private float punchDuration = 0.2f;

        [Header("Spring Punch")]
        [SerializeField] private float springPunchScale = 0.2f;
        [SerializeField] private float springPunchUpDuration = 0.12f;
        [SerializeField] private float springPunchDownDuration = 0.1f;
        [SerializeField] private float springPunchReturnDuration = 0.08f;

        [Header("Shake")]
        [SerializeField] private float shakeDuration = 0.4f;
        [SerializeField] private float shakeStrength = 20f;
        [SerializeField] private int shakeVibrato = 20;
        [SerializeField] private float shakeRandomness = 90f;

        private readonly Dictionary<string, Tween> activeTweens = new();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            var tweens = new List<Tween>(activeTweens.Values);
            for (var index = 0; index < tweens.Count; index++)
            {
                tweens[index]?.Kill();
            }

            activeTweens.Clear();
        }

        public void PlaySpringPunch(RectTransform target, float intensity = 1f)
        {
            var multiplier = Mathf.Max(0.01f, intensity);
            var initialScale = target.localScale;
            var upScale = initialScale * (1f + springPunchScale * multiplier);
            var downScale = initialScale * (1f - springPunchScale * 0.35f * multiplier);

            var sequence = DOTween.Sequence()
                .Append(target.DOScale(upScale, Mathf.Max(0.01f, springPunchUpDuration * multiplier)).SetEase(Ease.OutQuad))
                .Append(target.DOScale(downScale, Mathf.Max(0.01f, springPunchDownDuration * multiplier)).SetEase(Ease.InOutQuad))
                .Append(target.DOScale(initialScale, Mathf.Max(0.01f, springPunchReturnDuration * multiplier)).SetEase(Ease.OutBack))
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.localScale = initialScale;
                    }
                })
                .OnComplete(() =>
                {
                    if (target != null)
                    {
                        target.localScale = initialScale;
                    }
                });

            ReplaceTween(target, FeedbackScaleChannel, sequence);
        }

        public void PlayPunch(RectTransform target, float intensity = 1f)
        {
            var multiplier = Mathf.Max(0.01f, intensity);
            var initialScale = target.localScale;
            var tween = target
                .DOPunchScale(Vector3.one * punchScale * multiplier, Mathf.Max(0.01f, punchDuration * multiplier), 10, 0.8f)
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.localScale = initialScale;
                    }
                })
                .OnComplete(() =>
                {
                    if (target != null)
                    {
                        target.localScale = initialScale;
                    }
                });

            ReplaceTween(target, FeedbackScaleChannel, tween);
        }

        public void PlayTransformPunch(Transform target, float intensity = 1f)
        {
            var multiplier = Mathf.Max(0.01f, intensity);
            var initialScale = target.localScale;
            var tween = target
                .DOPunchScale(Vector3.one * punchScale * multiplier, Mathf.Max(0.01f, punchDuration * multiplier), 10, 0.8f)
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.localScale = initialScale;
                    }
                })
                .OnComplete(() =>
                {
                    if (target != null)
                    {
                        target.localScale = initialScale;
                    }
                });

            ReplaceTween(target, FeedbackScaleChannel, tween);
        }

        public void PlayTransformShake(Transform target, float intensity = 1f)
        {
            var multiplier = Mathf.Max(0.01f, intensity);
            var initialPosition = target.localPosition;
            var tween = target
                .DOShakePosition(Mathf.Max(0.01f, shakeDuration * multiplier), shakeStrength * multiplier, shakeVibrato, 90f, false, true)
                .SetEase(Ease.OutQuad)
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.localPosition = initialPosition;
                    }
                })
                .OnComplete(() =>
                {
                    if (target != null)
                    {
                        target.localPosition = initialPosition;
                    }
                });

            ReplaceTween(target, FeedbackPositionChannel, tween);
        }

        public void PlayShake(RectTransform target, float intensity = 1f)
        {
            var multiplier = Mathf.Max(0.01f, intensity);
            var initialRotation = target.localEulerAngles;
            var tween = target
                .DOShakeRotation(Mathf.Max(0.01f, shakeDuration * multiplier), new Vector3(0f, 0f, shakeStrength * multiplier), shakeVibrato, shakeRandomness)
                .SetEase(Ease.OutQuad)
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.localEulerAngles = initialRotation;
                    }
                })
                .OnComplete(() =>
                {
                    if (target != null)
                    {
                        target.localEulerAngles = initialRotation;
                    }
                });

            ReplaceTween(target, FeedbackRotationChannel, tween);
        }

        public void PlayBounceLoopY(RectTransform target, string channel, float amplitude, float halfDuration, bool useUnscaledTime = true)
        {
            var basePosition = target.anchoredPosition;
            var sequence = DOTween.Sequence()
                .Append(target.DOAnchorPosY(basePosition.y + amplitude, halfDuration).SetEase(Ease.OutQuad))
                .Append(target.DOAnchorPosY(basePosition.y, halfDuration).SetEase(Ease.InQuad))
                .SetLoops(-1)
                .SetUpdate(useUnscaledTime)
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.anchoredPosition = basePosition;
                    }
                });

            ReplaceTween(target, channel, sequence);
        }

        public void Stop(RectTransform target, string channel)
        {
            KillTween(target, channel);
        }

        public void Stop(Transform target, string channel)
        {
            KillTween(target, channel);
        }

        public void StopFeedback(Transform target)
        {
            KillTween(target, FeedbackScaleChannel);
            KillTween(target, FeedbackRotationChannel);
            KillTween(target, FeedbackPositionChannel);
        }

        public void PlayMoveAndScale(
            RectTransform target,
            string channel,
            Vector2 endAnchoredPosition,
            Vector3 endScale,
            float duration,
            Ease moveEase,
            Ease scaleEase,
            Action onComplete = null)
        {
            var sequence = DOTween.Sequence()
                .Append(target.DOAnchorPos(endAnchoredPosition, duration).SetEase(moveEase))
                .Join(target.DOScale(endScale, duration).SetEase(scaleEase))
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, sequence);
        }

        public void PlayWorldMoveAndScale(
            Transform target,
            string channel,
            Vector3 endPosition,
            Vector3 endScale,
            float duration,
            Ease moveEase,
            Ease scaleEase,
            Action onComplete = null)
        {
            var sequence = DOTween.Sequence()
                .Append(target.DOMove(endPosition, duration).SetEase(moveEase))
                .Join(target.DOScale(endScale, duration).SetEase(scaleEase))
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, sequence);
        }

        public void PlayWorldMove(
            Transform target,
            string channel,
            Vector3 endPosition,
            float duration,
            Ease moveEase,
            Action onComplete = null)
        {
            var tween = target
                .DOMove(endPosition, duration)
                .SetEase(moveEase)
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, tween);
        }

        public void PlayScaleTo(
            RectTransform target,
            string channel,
            Vector3 endScale,
            float duration,
            Ease ease,
            Action onComplete = null)
        {
            var tween = target
                .DOScale(endScale, duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, tween);
        }

        public void PlayWorldScaleTo(
            Transform target,
            string channel,
            Vector3 endScale,
            float duration,
            Ease ease,
            Action onComplete = null)
        {
            var tween = target
                .DOScale(endScale, duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, tween);
        }

        public void PlayGraphicColorFlash(Graphic target, string channel, Color flashColor, float duration)
        {
            var initialColor = target.color;
            var sequence = DOTween.Sequence()
                .Append(target.DOColor(flashColor, duration * 0.5f).SetEase(Ease.OutQuad))
                .Append(target.DOColor(initialColor, duration * 0.5f).SetEase(Ease.InQuad))
                .OnKill(() =>
                {
                    if (target != null)
                    {
                        target.color = initialColor;
                    }
                })
                .OnComplete(() =>
                {
                    if (target != null)
                    {
                        target.color = initialColor;
                    }
                });

            ReplaceTween(target, channel, sequence);
        }

        public void PlaySpriteFade(
            SpriteRenderer target,
            string channel,
            float endAlpha,
            float duration,
            Ease ease,
            Action onComplete = null)
        {
            var tween = target
                .DOFade(endAlpha, duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, tween);
        }

        public void PlayFloatAndFade(
            RectTransform target,
            CanvasGroup canvasGroup,
            string channel,
            Vector2 endAnchoredPosition,
            Vector3 endScale,
            float duration,
            Ease moveEase,
            Ease scaleEase,
            Action onComplete = null)
        {
            var sequence = DOTween.Sequence()
                .Append(target.DOAnchorPos(endAnchoredPosition, duration).SetEase(moveEase))
                .Join(target.DOScale(endScale, duration).SetEase(scaleEase))
                .Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.OutQuad))
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, channel, sequence);
        }

        public void PlayCanvasVisibility(
            CanvasGroup canvasGroup,
            RectTransform target,
            Vector2 shownPosition,
            Vector3 shownScale,
            bool isVisible,
            float duration,
            float hiddenYOffset,
            float hiddenScale,
            Action onComplete = null)
        {
            var hiddenPosition = shownPosition + Vector2.down * hiddenYOffset;
            var hiddenLocalScale = shownScale * hiddenScale;

            if (isVisible)
            {
                target.anchoredPosition = hiddenPosition;
                target.localScale = hiddenLocalScale;
                canvasGroup.alpha = 0f;
            }
            else
            {
                target.anchoredPosition = shownPosition;
                target.localScale = shownScale;
                canvasGroup.alpha = 1f;
            }

            var sequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(isVisible ? 1f : 0f, duration).SetEase(Ease.OutQuad))
                .Join(target.DOAnchorPos(isVisible ? shownPosition : hiddenPosition, duration).SetEase(isVisible ? Ease.OutCubic : Ease.OutQuad))
                .Join(target.DOScale(isVisible ? shownScale : hiddenLocalScale, duration).SetEase(isVisible ? Ease.OutCubic : Ease.OutQuad))
                .OnComplete(() => onComplete?.Invoke());

            ReplaceTween(target, "visibility", sequence);
        }

        private void ReplaceTween(Component target, string channel, Tween tween)
        {
            KillTween(target, channel);

            var key = BuildKey(target, channel);
            tween.SetLink(target.gameObject, LinkBehaviour.KillOnDestroy);
            activeTweens[key] = tween;
            tween.OnKill(() =>
            {
                if (activeTweens.TryGetValue(key, out var current) && current == tween)
                {
                    activeTweens.Remove(key);
                }
            });
        }

        private void KillTween(Component target, string channel)
        {
            var key = BuildKey(target, channel);
            if (activeTweens.TryGetValue(key, out var tween))
            {
                activeTweens.Remove(key);
                if (tween.IsActive())
                {
                    tween.Kill();
                }
            }
        }

        private static string BuildKey(Component target, string channel)
        {
            return $"{target.GetInstanceID()}:{channel}";
        }
    }
}
