using DG.Tweening;
using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiFloatingTextManager : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private UiFloatingTextView floatingTextPrefab;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float travelDistance = 56f;
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private Vector3 endScale = Vector3.one * 0.92f;

        public void SpawnAtWorldPosition(string textValue, Color textColor, Vector3 worldPosition)
        {
            var view = Instantiate(floatingTextPrefab, root);
            view.Show(textValue, textColor);
            view.RectTransform.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(root, uiCamera, worldPosition);
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
