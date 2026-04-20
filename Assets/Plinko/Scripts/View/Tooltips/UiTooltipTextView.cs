using TMPro;
using UnityEngine;

namespace Plinko.Scripts.View.Tooltips
{
    public sealed class UiTooltipTextView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private TMP_Text bodyText;

        public RectTransform RectTransform => root;

        public void Refresh(string text)
        {
            if (bodyText != null)
            {
                bodyText.text = text;
            }
        }
    }
}
