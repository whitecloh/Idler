using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiCanvasGroupVisibility : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform target;
        [SerializeField] private float fadeDuration = 0.22f;
        [SerializeField] private float hiddenYOffset = 18f;
        [SerializeField] private float hiddenScale = 0.985f;
        private Vector2 _shownPosition;
        private Vector3 _shownScale;
        private bool _isInitialized;

        private void Awake()
        {
            CacheShownState();
        }

        public void ShowImmediate()
        {
            CacheShownState();
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            target.anchoredPosition = _shownPosition;
            target.localScale = _shownScale;
        }

        public void HideImmediate()
        {
            CacheShownState();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            target.anchoredPosition = _shownPosition + Vector2.down * hiddenYOffset;
            target.localScale = _shownScale * hiddenScale;
            gameObject.SetActive(false);
        }

        public void ShowAnimated()
        {
            CacheShownState();
            gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            UiAnimationManager.Instance.PlayCanvasVisibility(canvasGroup, target, true, fadeDuration, hiddenYOffset, hiddenScale);
        }

        public void HideAnimated()
        {
            CacheShownState();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            UiAnimationManager.Instance.PlayCanvasVisibility(
                canvasGroup,
                target,
                false,
                fadeDuration,
                hiddenYOffset,
                hiddenScale,
                () => gameObject.SetActive(false));
        }

        private void CacheShownState()
        {
            if (_isInitialized)
            {
                return;
            }

            _shownPosition = target.anchoredPosition;
            _shownScale = target.localScale;
            _isInitialized = true;
        }
    }
}
