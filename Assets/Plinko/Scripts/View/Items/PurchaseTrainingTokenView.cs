using Plinko.Scripts.View.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseTrainingTokenView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image iconImage;

        public RectTransform RectTransform => root;

        public void SetSprite(Sprite sprite)
        {
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        public void PlayPunch()
        {
            UiAnimationManager.Instance.PlayPunch(root);
        }
    }
}
