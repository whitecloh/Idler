using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineUnitWorldView : MonoBehaviour
    {
        private enum AnimationState
        {
            Idle = 0,
            Run = 1
        }

        [SerializeField] private Transform root;
        [SerializeField] private SpriteRenderer primaryRenderer;
        [SerializeField] private SpriteRendererFrameAnimationView animationView;
        [SerializeField] private GameObject healthBarRoot;
        [SerializeField] private Transform healthFillTransform;

        private Vector3 _healthFillBaseScale = Vector3.one;
        private BattleBoardUnitViewData _viewData = new();
        private AnimationState _animationState;
        private bool _isOneShotPlaying;

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
            _viewData = viewData;
            RuntimeId = viewData.RuntimeId;
            if (primaryRenderer != null)
            {
                var color = primaryRenderer.color;
                color.a = 1f;
                primaryRenderer.color = color;
            }

            RootTransform.localScale = Vector3.one;
            RefreshHealthBar(viewData);
            if (!_isOneShotPlaying)
            {
                PlayIdle();
            }
        }

        public void SetFacing(bool isFacingRight)
        {
            if (primaryRenderer != null)
            {
                primaryRenderer.flipX = isFacingRight;
            }
        }

        public void SetMoving(bool isMoving)
        {
            if (_isOneShotPlaying)
            {
                _animationState = isMoving ? AnimationState.Run : AnimationState.Idle;
                return;
            }

            if (isMoving)
            {
                PlayRun();
                return;
            }

            PlayIdle();
        }

        public void PlayAttack()
        {
            var frames = _viewData.AttackType == Data.Common.Enums.AttackType.Ranged &&
                         _viewData.BattleAnimations != null &&
                         _viewData.BattleAnimations.CastFrames != null &&
                         _viewData.BattleAnimations.CastFrames.Count > 0
                ? _viewData.BattleAnimations.CastFrames
                : _viewData.BattleAnimations != null ? _viewData.BattleAnimations.AttackFrames : null;

            PlayOneShot(frames);
        }

        public void PlayHit()
        {
            var frames = _viewData.BattleAnimations != null ? _viewData.BattleAnimations.HitFrames : null;
            PlayOneShot(frames);
        }

        private void PlayIdle()
        {
            _animationState = AnimationState.Idle;
            var idleFrames = _viewData.BattleAnimations != null ? _viewData.BattleAnimations.IdleFrames : null;
            if (idleFrames != null && idleFrames.Count > 0)
            {
                animationView.PlayLoop(idleFrames);
                return;
            }

            animationView.ShowStatic(_viewData.PortraitSprite);
        }

        private void PlayRun()
        {
            _animationState = AnimationState.Run;
            var runFrames = _viewData.BattleAnimations != null ? _viewData.BattleAnimations.RunFrames : null;
            if (runFrames != null && runFrames.Count > 0)
            {
                animationView.PlayLoop(runFrames);
                return;
            }

            PlayIdle();
        }

        private void PlayOneShot(System.Collections.Generic.IReadOnlyList<Sprite> frames)
        {
            if (frames == null || frames.Count == 0)
            {
                if (_animationState == AnimationState.Run)
                {
                    PlayRun();
                    return;
                }

                PlayIdle();
                return;
            }

            _isOneShotPlaying = true;
            animationView.PlayOneShot(frames, () =>
            {
                _isOneShotPlaying = false;
                if (_animationState == AnimationState.Run)
                {
                    PlayRun();
                    return;
                }

                PlayIdle();
            });
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
