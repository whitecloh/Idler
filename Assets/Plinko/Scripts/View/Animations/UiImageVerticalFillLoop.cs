using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiImageVerticalFillLoop : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Color minColor = new(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color maxColor = Color.white;
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
            ApplyColor(minColor);
        }

        private void OnValidate()
        {
            halfCycleDuration = Mathf.Max(0.01f, halfCycleDuration);
        }

        public void Restart()
        {
            Stop();
            if (targetImage == null)
            {
                return;
            }

            if (resetToMinOnEnable)
            {
                ApplyColor(minColor);
            }

            _loopTween = targetImage
                .DOColor(maxColor, halfCycleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(useUnscaledTime);
        }

        public void Stop()
        {
            _loopTween?.Kill();
            _loopTween = null;
        }

        private void ApplyColor(Color color)
        {
            if (targetImage == null)
            {
                return;
            }

            targetImage.color = color;
        }
    }
}
