using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLinePlayerBaseWorldView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Transform RootTransform => spriteRenderer != null ? spriteRenderer.transform : transform;

        public void Refresh(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = sprite != null;
        }
    }
}
