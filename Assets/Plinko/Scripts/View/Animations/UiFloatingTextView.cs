using TMPro;
using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiFloatingTextView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text valueText;

        public RectTransform RectTransform => root;
        public CanvasGroup CanvasGroup => canvasGroup;

        public void Show(string textValue, Color textColor)
        {
            valueText.text = textValue;
            valueText.color = textColor;
            canvasGroup.alpha = 1f;
            root.localScale = Vector3.one;
        }
    }
}
