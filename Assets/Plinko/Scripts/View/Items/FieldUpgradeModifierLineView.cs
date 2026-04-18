using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class FieldUpgradeModifierLineView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        public void Refresh(PinModifierLineViewData viewData)
        {
            labelText.text = viewData.Label;
            valueText.text = viewData.Value > 0
                ? $"+{viewData.Value}"
                : viewData.Value.ToString();
        }

        public PinModifierLineViewData CaptureSnapshot()
        {
            return new PinModifierLineViewData
            {
                Label = labelText.text,
                Value = ParseValue(valueText.text)
            };
        }

        private static int ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            var sanitized = value.Replace("+", string.Empty);
            return int.TryParse(sanitized, out var parsed) ? parsed : 0;
        }
    }
}