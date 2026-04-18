using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseArmyPreviewUnitView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private SpriteFrameAnimationView spriteFrameAnimationView;

        public RectTransform RectTransform => root;

        public void Refresh(PurchaseArmyPreviewUnitViewData viewData)
        {
            var idleFrames = viewData.BattleAnimations != null ? viewData.BattleAnimations.IdleFrames : null;
            if (idleFrames != null && idleFrames.Count > 0)
            {
                spriteFrameAnimationView.Play(idleFrames);
            }
            else
            {
                spriteFrameAnimationView.ShowStatic(viewData.PortraitSprite);
            }
        }
    }
}