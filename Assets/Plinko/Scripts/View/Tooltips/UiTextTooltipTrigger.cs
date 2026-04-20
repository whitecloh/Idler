using UnityEngine;
using UnityEngine.EventSystems;

namespace Plinko.Scripts.View.Tooltips
{
    public sealed class UiTextTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private UiTooltipPlacement placement = UiTooltipPlacement.Top;
        [SerializeField] private Vector2 offset = new(0f, 12f);
        [TextArea]
        [SerializeField] private string tooltipText = string.Empty;

        private RectTransform TargetRect => targetRect != null ? targetRect : transform as RectTransform;

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowText(this, TargetRect, tooltipText, placement, offset);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.Hide(this);
        }

        private void OnDisable()
        {
            UiTooltipManager.Instance?.Hide(this);
        }
    }
}
