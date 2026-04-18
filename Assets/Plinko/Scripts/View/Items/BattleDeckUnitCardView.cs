using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class BattleDeckUnitCardView : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text manaText;

        public void Refresh(BattleDeckUnitViewData viewData)
        {
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            attackText.text = viewData.Attack.ToString();
            healthText.text = viewData.Health.ToString();
            manaText.text = viewData.ManaCost.ToString();
        }
    }
}
