using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseTrainingPinView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseFieldPinViewData viewData)
        {
            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;
            nameText.text = viewData.DisplayName;
        }

        public void PlayPunch()
        {
            UiAnimationManager.Instance.PlaySpringPunch(root);
        }
    }
}
