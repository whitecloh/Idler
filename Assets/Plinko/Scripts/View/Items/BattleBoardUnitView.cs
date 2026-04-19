using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using UnityEngine;
using CanvasGroup = UnityEngine.CanvasGroup;

namespace Plinko.Scripts.View.Items
{
    public sealed class BattleBoardUnitView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private SpriteFrameAnimationView spriteFrameAnimationView;

        public RectTransform RectTransform => root;
        public CanvasGroup CanvasGroup => canvasGroup;
        public int RuntimeId { get; private set; }

        public void Refresh(BattleBoardUnitViewData viewData)
        {
            RuntimeId = viewData.RuntimeId;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            root.localScale = Vector3.one;
            var idleFrames = viewData.BattleAnimations != null ? viewData.BattleAnimations.IdleFrames : null;
            if (idleFrames != null && idleFrames.Count > 0)
            {
                spriteFrameAnimationView.Play(idleFrames);
                return;
            }

            spriteFrameAnimationView.ShowStatic(viewData.PortraitSprite);
        }
    }
}
