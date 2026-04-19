using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public static class UiRectTransformUtility
    {
        public static Vector2 WorldToAnchoredPosition(
            RectTransform targetSpace,
            Camera uiCamera,
            Vector3 worldPosition)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetSpace,
                screenPoint,
                uiCamera,
                out var localPoint);
            return localPoint;
        }

        public static Vector2 WorldToAnchoredPositionOverlay(
            RectTransform targetSpace,
            Camera worldCamera,
            Vector3 worldPosition)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetSpace,
                screenPoint,
                null,
                out var localPoint);
            return localPoint;
        }

        public static Vector2 ViewportToAnchoredPositionOverlay(
            RectTransform targetSpace,
            RectTransform viewportRect,
            Vector3 viewportPoint)
        {
            var rect = viewportRect.rect;
            var localViewportPoint = new Vector3(
                rect.xMin + Mathf.Clamp01(viewportPoint.x) * rect.width,
                rect.yMin + Mathf.Clamp01(viewportPoint.y) * rect.height,
                0f);
            var worldPoint = viewportRect.TransformPoint(localViewportPoint);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetSpace,
                screenPoint,
                null,
                out var localPoint);
            return localPoint;
        }

        public static Vector2 WorldToAnchoredPositionInViewport(
            RectTransform targetSpace,
            RectTransform viewportRect,
            Camera worldCamera,
            Vector3 worldPosition)
        {
            var viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);
            return ViewportToAnchoredPositionOverlay(targetSpace, viewportRect, viewportPoint);
        }

        public static Vector3 GetWorldCenter(RectTransform rectTransform)
        {
            return rectTransform.TransformPoint(rectTransform.rect.center);
        }

        public static Vector2 ScreenToAnchoredPosition(
            RectTransform targetSpace,
            Vector2 screenPosition,
            Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetSpace,
                screenPosition,
                eventCamera,
                out var localPoint);
            return localPoint;
        }
    }
}
