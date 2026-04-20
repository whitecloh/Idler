using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class UnitStatEntryView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text valueText;

        public void Refresh(StatDisplayViewData viewData)
        {
            if (iconImage != null)
            {
                iconImage.sprite = viewData != null ? viewData.Icon : null;
                iconImage.enabled = iconImage.sprite != null;
            }

            if (valueText != null)
            {
                valueText.text = viewData != null ? viewData.ValueText : string.Empty;
            }
        }
    }
}
