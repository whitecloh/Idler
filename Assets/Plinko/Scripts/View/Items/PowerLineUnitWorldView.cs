using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineUnitWorldView : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [SerializeField] private SpriteRenderer primaryRenderer;
        [SerializeField] private SpriteRendererFrameAnimationView animationView;
        [SerializeField] private GameObject healthBarRoot;
        [SerializeField] private Transform healthFillTransform;

        private Vector3 _healthFillBaseScale = Vector3.one;

        private void Awake()
        {
            if (healthFillTransform != null)
            {
                _healthFillBaseScale = healthFillTransform.localScale;
            }
        }

        public Transform RootTransform => root != null ? root : transform;
        public SpriteRenderer PrimaryRenderer => primaryRenderer;
        public int RuntimeId { get; private set; }

        public void Refresh(BattleBoardUnitViewData viewData)
        {
            RuntimeId = viewData.RuntimeId;
            if (primaryRenderer != null)
            {
                var color = primaryRenderer.color;
                color.a = 1f;
                primaryRenderer.color = color;
            }

            RootTransform.localScale = Vector3.one;
            RefreshHealthBar(viewData);
            var idleFrames = viewData.BattleAnimations != null ? viewData.BattleAnimations.IdleFrames : null;
            if (idleFrames != null && idleFrames.Count > 0)
            {
                animationView.Play(idleFrames);
                return;
            }

            animationView.ShowStatic(viewData.PortraitSprite);
        }

        private void RefreshHealthBar(BattleBoardUnitViewData viewData)
        {
            if (healthBarRoot != null)
            {
                healthBarRoot.SetActive(viewData.MaxHealth > 0);
            }

            if (healthFillTransform == null)
            {
                return;
            }

            var maxHealth = Mathf.Max(1, viewData.MaxHealth > 0 ? viewData.MaxHealth : viewData.Health);
            var normalizedHealth = Mathf.Clamp01(viewData.Health / (float)maxHealth);
            healthFillTransform.localScale = new Vector3(
                _healthFillBaseScale.x * normalizedHealth,
                _healthFillBaseScale.y,
                _healthFillBaseScale.z);
        }
    }
}
