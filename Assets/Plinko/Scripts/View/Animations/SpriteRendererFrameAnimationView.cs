using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class SpriteRendererFrameAnimationView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float framesPerSecond = 10f;

        private IReadOnlyList<Sprite> _frames;
        private IReadOnlyList<Sprite> _loopFrames;
        private float _elapsed;
        private bool _isPlaying;
        private bool _isLoop;
        private System.Action _completed;

        private void Update()
        {
            if (!_isPlaying || _frames == null || _frames.Count == 0)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            var frameIndex = Mathf.FloorToInt(_elapsed * Mathf.Max(1f, framesPerSecond));
            if (!_isLoop && frameIndex >= _frames.Count)
            {
                var completed = _completed;
                _completed = null;
                if (_loopFrames != null && _loopFrames.Count > 0)
                {
                    PlayLoop(_loopFrames);
                }
                else
                {
                    _isPlaying = false;
                    frameIndex = _frames.Count - 1;
                    targetRenderer.sprite = _frames[frameIndex];
                }

                completed?.Invoke();
                return;
            }

            if (_isLoop)
            {
                frameIndex %= _frames.Count;
            }
            else
            {
                frameIndex = Mathf.Clamp(frameIndex, 0, _frames.Count - 1);
            }

            targetRenderer.sprite = _frames[frameIndex];
        }

        public void Play(IReadOnlyList<Sprite> frames)
        {
            PlayLoop(frames);
        }

        public void PlayLoop(IReadOnlyList<Sprite> frames)
        {
            _frames = frames;
            _loopFrames = frames;
            _elapsed = 0f;
            _isPlaying = _frames != null && _frames.Count > 0;
            _isLoop = true;
            _completed = null;

            if (_isPlaying)
            {
                targetRenderer.sprite = _frames[0];
            }
        }

        public void PlayOneShot(IReadOnlyList<Sprite> frames, System.Action completed = null)
        {
            if (frames == null || frames.Count == 0)
            {
                completed?.Invoke();
                return;
            }

            _frames = frames;
            _elapsed = 0f;
            _isPlaying = true;
            _isLoop = false;
            _completed = completed;
            targetRenderer.sprite = _frames[0];
        }

        public void ShowStatic(Sprite sprite)
        {
            _frames = null;
            _loopFrames = null;
            _elapsed = 0f;
            _isPlaying = false;
            _isLoop = false;
            _completed = null;
            targetRenderer.sprite = sprite;
        }
    }
}
