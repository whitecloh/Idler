using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseUnitCardView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private TMP_Text moveSpeedText;
        [SerializeField] private TMP_Text attackRangeText;
        [SerializeField] private TMP_Text attackSpeedText;

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseTrainedUnitCardViewData viewData)
        {
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            attackText.text = viewData.Attack.ToString();
            healthText.text = viewData.Health.ToString();
            manaText.text = viewData.ManaCost.ToString();
            if (moveSpeedText != null)
            {
                moveSpeedText.text = viewData.MoveSpeed.ToString("0.##");
            }

            if (attackRangeText != null)
            {
                attackRangeText.text = viewData.AttackRange.ToString();
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text = viewData.AttackSpeed.ToString("0.##");
            }
        }

        public void Refresh(SignalPurchasePendingUnitCardViewData viewData)
        {
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            attackText.text = viewData.Attack.ToString();
            healthText.text = viewData.Health.ToString();
            manaText.text = viewData.ManaCost.ToString();
            if (moveSpeedText != null)
            {
                moveSpeedText.text = viewData.MoveSpeed.ToString("0.##");
            }

            if (attackRangeText != null)
            {
                attackRangeText.text = viewData.AttackRange.ToString();
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text = viewData.AttackSpeed.ToString("0.##");
            }
        }
    }
}
