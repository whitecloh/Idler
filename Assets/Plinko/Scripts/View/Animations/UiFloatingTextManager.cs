using DG.Tweening;
using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiFloatingTextManager : MonoBehaviour
    {
        public static UiFloatingTextManager Instance { get; private set; }

        [SerializeField] private RectTransform root;
        [SerializeField] private UiFloatingTextView floatingTextPrefab;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float travelDistance = 56f;
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private Vector3 endScale = Vector3.one * 0.92f;

        private RectTransform _worldViewportRect;
        private Camera _worldProjectionCamera;

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
        }

        public void ConfigureWorldViewport(RectTransform viewportRect, Camera projectionCamera)
        {
            _worldViewportRect = viewportRect;
            _worldProjectionCamera = projectionCamera;
        }

        public void SpawnAtWorldPosition(string textValue, Color textColor, Vector3 worldPosition)
        {
            var view = Instantiate(floatingTextPrefab, root);
            view.Show(textValue, textColor);
            view.RectTransform.anchoredPosition = _worldViewportRect != null && _worldProjectionCamera != null
                ? UiRectTransformUtility.WorldToAnchoredPositionInViewport(root, _worldViewportRect, _worldProjectionCamera, worldPosition)
                : UiRectTransformUtility.WorldToAnchoredPosition(root, uiCamera, worldPosition);
            UiAnimationManager.Instance.PlayFloatAndFade(
                view.RectTransform,
                view.CanvasGroup,
                $"floating-text-{view.GetInstanceID()}",
                view.RectTransform.anchoredPosition + Vector2.up * travelDistance,
                endScale,
                duration,
                Ease.OutCubic,
                Ease.OutQuad,
                () => Destroy(view.gameObject));
        }

        public void SpawnAtRectTransform(string textValue, Color textColor, RectTransform anchor)
        {
            SpawnAtWorldPosition(textValue, textColor, UiRectTransformUtility.GetWorldCenter(anchor));
        }
    }
}
