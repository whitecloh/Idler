using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLinePlayerBaseView : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
        [SerializeField] private TMP_Text healthText;

        public void Refresh(BattleBaseViewData baseViewData)
        {
            baseImage.sprite = baseViewData.Sprite;
            baseImage.enabled = baseViewData.Sprite != null;
            healthText.text = $"{baseViewData.CurrentHealth}/{baseViewData.MaxHealth}";
        }
    }
}
