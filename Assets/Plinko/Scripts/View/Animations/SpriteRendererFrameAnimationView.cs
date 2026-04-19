using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class SpriteRendererFrameAnimationView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float framesPerSecond = 10f;

        private IReadOnlyList<Sprite> _frames;
        private float _elapsed;
        private bool _isPlaying;

        private void Update()
        {
            if (!_isPlaying || _frames == null || _frames.Count == 0)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            var frameIndex = Mathf.FloorToInt(_elapsed * Mathf.Max(1f, framesPerSecond)) % _frames.Count;
            targetRenderer.sprite = _frames[frameIndex];
        }

        public void Play(IReadOnlyList<Sprite> frames)
        {
            _frames = frames;
            _elapsed = 0f;
            _isPlaying = _frames != null && _frames.Count > 0;

            if (_isPlaying)
            {
                targetRenderer.sprite = _frames[0];
            }
        }

        public void ShowStatic(Sprite sprite)
        {
            _frames = null;
            _elapsed = 0f;
            _isPlaying = false;
            targetRenderer.sprite = sprite;
        }
    }
}
