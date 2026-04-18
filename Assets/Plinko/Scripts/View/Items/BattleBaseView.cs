using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class BattleBaseView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image baseImage;
        [SerializeField] private TMP_Text healthText;

        public RectTransform RectTransform => root;

        public void Refresh(BattleBaseViewData viewData)
        {
            baseImage.sprite = viewData.Sprite;
            baseImage.enabled = viewData.Sprite != null;
            healthText.text = $"{viewData.CurrentHealth}/{viewData.MaxHealth}";
        }
    }
}
