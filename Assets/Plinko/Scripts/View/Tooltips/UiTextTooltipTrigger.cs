using UnityEngine;
using UnityEngine.EventSystems;

namespace Plinko.Scripts.View.Tooltips
{
    public sealed class UiTextTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea]
        [SerializeField] private string tooltipText = string.Empty;

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowText(this, tooltipText);
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
