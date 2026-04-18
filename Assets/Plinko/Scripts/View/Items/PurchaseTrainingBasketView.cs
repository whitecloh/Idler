using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseTrainingBasketView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text manaText;

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseFieldBasketViewData viewData)
        {
            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;
            manaText.text = viewData.ManaValue.ToString();
        }

        public void PlayPunch()
        {
            UiAnimationManager.Instance.PlaySpringPunch(root);
        }
    }
}
