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

        public static Vector3 GetWorldCenter(RectTransform rectTransform)
        {
            return rectTransform.TransformPoint(rectTransform.rect.center);
        }
    }
}
