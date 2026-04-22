using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiGraphicFadeLoop : MonoBehaviour
    {
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private float minAlpha = 0.25f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float halfCycleDuration = 1f;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool resetToMinOnEnable = true;

        private Tween _loopTween;

        private void OnEnable()
        {
            Restart();
        }

        private void OnDisable()
        {
            Stop();
            ApplyAlpha(minAlpha);
        }

        private void OnValidate()
        {
            minAlpha = Mathf.Clamp01(minAlpha);
            maxAlpha = Mathf.Clamp01(maxAlpha);
            halfCycleDuration = Mathf.Max(0.01f, halfCycleDuration);
            if (maxAlpha < minAlpha)
            {
                maxAlpha = minAlpha;
            }
        }

        public void Restart()
        {
            Stop();
            if (targetGraphic == null)
            {
                return;
            }

            if (resetToMinOnEnable)
            {
                ApplyAlpha(minAlpha);
            }

            _loopTween = targetGraphic
                .DOFade(maxAlpha, halfCycleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(useUnscaledTime);
        }

        public void Stop()
        {
            _loopTween?.Kill();
            _loopTween = null;
        }

        private void ApplyAlpha(float alpha)
        {
            if (targetGraphic == null)
            {
                return;
            }

            var color = targetGraphic.color;
            color.a = alpha;
            targetGraphic.color = color;
        }
    }
}
