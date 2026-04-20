using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineProjectileWorldView : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Transform RootTransform => root != null ? root : transform;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        public void Refresh(Sprite sprite, bool isFacingRight)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.flipX = !isFacingRight;
            var color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
}
