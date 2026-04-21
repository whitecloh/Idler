using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLinePlayerBaseWorldView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool _configuredRendererStateCaptured;
        private SpriteDrawMode _configuredDrawMode = SpriteDrawMode.Simple;
        private Vector2 _configuredSize = Vector2.one;

        public Transform RootTransform => spriteRenderer != null ? spriteRenderer.transform : transform;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private void Awake()
        {
            CaptureConfiguredRendererState();
        }

        public void Refresh(Sprite sprite)
        {
            CaptureConfiguredRendererState();
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = sprite != null;
            RestoreConfiguredRendererState();
        }

        public void RestoreConfiguredRendererState()
        {
            if (!_configuredRendererStateCaptured || spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.drawMode = _configuredDrawMode;
            spriteRenderer.size = _configuredSize;
        }

        private void CaptureConfiguredRendererState()
        {
            if (_configuredRendererStateCaptured || spriteRenderer == null)
            {
                return;
            }

            _configuredDrawMode = spriteRenderer.drawMode;
            _configuredSize = spriteRenderer.size;
            _configuredRendererStateCaptured = true;
        }
    }
}
