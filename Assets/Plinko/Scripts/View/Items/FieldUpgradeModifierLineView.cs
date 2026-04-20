using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class FieldUpgradeModifierLineView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;
        private StatDisplayViewData _currentViewData;

        public void Refresh(StatDisplayViewData viewData)
        {
            _currentViewData = viewData;

            if (iconImage != null)
            {
                iconImage.sprite = viewData != null ? viewData.Icon : null;
                iconImage.enabled = iconImage.sprite != null;
            }

            labelText.text = viewData != null ? viewData.DisplayName : string.Empty;
            valueText.text = viewData != null ? viewData.ValueText : string.Empty;
        }

        public StatDisplayViewData CaptureSnapshot()
        {
            if (_currentViewData == null)
            {
                return new StatDisplayViewData
                {
                    DisplayName = labelText != null ? labelText.text : string.Empty,
                    Icon = iconImage != null ? iconImage.sprite : null,
                    ValueText = valueText != null ? valueText.text : string.Empty
                };
            }

            return new StatDisplayViewData
            {
                StatTypeId = _currentViewData.StatTypeId,
                DisplayName = _currentViewData.DisplayName,
                Icon = _currentViewData.Icon,
                ValueText = _currentViewData.ValueText
            };
        }
    }
}
