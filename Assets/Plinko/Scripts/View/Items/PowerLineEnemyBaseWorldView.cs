using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineEnemyBaseWorldView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject[] connectedSocketStateRoots;

        private bool _configuredRendererStateCaptured;
        private SpriteDrawMode _configuredDrawMode = SpriteDrawMode.Simple;
        private Vector2 _configuredSize = Vector2.one;

        public Transform RootTransform => spriteRenderer != null ? spriteRenderer.transform : transform;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private void Awake()
        {
            CaptureConfiguredRendererState();
        }

        public void Refresh(Sprite sprite, IReadOnlyList<PowerLineLaneViewData> lanes)
        {
            CaptureConfiguredRendererState();
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = sprite != null;
            RestoreConfiguredRendererState();

            if (connectedSocketStateRoots == null)
            {
                return;
            }

            for (var index = 0; index < connectedSocketStateRoots.Length; index++)
            {
                var isConnected = lanes != null && index < lanes.Count && lanes[index].IsConnected;
                if (connectedSocketStateRoots[index] != null)
                {
                    connectedSocketStateRoots[index].SetActive(isConnected);
                }
            }
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
