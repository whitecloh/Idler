using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.View.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private GameAudioLibrary library;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource[] sfxSources;
        [SerializeField] private bool playMusicOnAwake = true;
        [SerializeField] private GameAudioCueType startupMusicCue = GameAudioCueType.BackgroundMusic;

        private readonly Dictionary<GameAudioCueType, GameAudioCueDefinition> _definitionsByType = new();
        private int _nextSfxSourceIndex;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            AudioSettingsStore.Apply();
            RebuildLookup();

            if (playMusicOnAwake)
            {
                PlayMusic(startupMusicCue);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Play(GameAudioCueType cueType, float volumeScale = 1f, float pitchScale = 1f)
        {
            if (cueType == GameAudioCueType.None || !_definitionsByType.TryGetValue(cueType, out var definition))
            {
                return;
            }

            if (definition.Loop || cueType == GameAudioCueType.BackgroundMusic)
            {
                PlayMusic(cueType, volumeScale, pitchScale);
                return;
            }

            var clip = PickClip(definition);
            var source = GetSfxSource();
            if (clip == null || source == null)
            {
                return;
            }

            source.loop = false;
            source.clip = clip;
            source.volume = Mathf.Clamp01(definition.Volume * volumeScale);
            source.pitch = Random.Range(
                Mathf.Min(definition.PitchRange.x, definition.PitchRange.y),
                Mathf.Max(definition.PitchRange.x, definition.PitchRange.y)) * pitchScale;
            source.Play();
        }

        public void PlayMusic(GameAudioCueType cueType, float volumeScale = 1f, float pitchScale = 1f)
        {
            if (musicSource == null || !_definitionsByType.TryGetValue(cueType, out var definition))
            {
                return;
            }

            var clip = PickClip(definition);
            if (clip == null)
            {
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.loop = true;
            musicSource.clip = clip;
            musicSource.volume = Mathf.Clamp01(definition.Volume * volumeScale);
            musicSource.pitch = Random.Range(
                Mathf.Min(definition.PitchRange.x, definition.PitchRange.y),
                Mathf.Max(definition.PitchRange.x, definition.PitchRange.y)) * pitchScale;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        private void RebuildLookup()
        {
            _definitionsByType.Clear();
            if (library == null)
            {
                return;
            }

            for (var index = 0; index < library.Cues.Count; index++)
            {
                var definition = library.Cues[index];
                if (definition == null)
                {
                    continue;
                }

                _definitionsByType[definition.CueType] = definition;
            }
        }

        private AudioSource GetSfxSource()
        {
            if (sfxSources == null || sfxSources.Length == 0)
            {
                return null;
            }

            for (var index = 0; index < sfxSources.Length; index++)
            {
                var sourceIndex = (_nextSfxSourceIndex + index) % sfxSources.Length;
                var source = sfxSources[sourceIndex];
                if (source == null)
                {
                    continue;
                }

                if (!source.isPlaying)
                {
                    _nextSfxSourceIndex = (sourceIndex + 1) % sfxSources.Length;
                    return source;
                }
            }

            var fallbackIndex = _nextSfxSourceIndex % sfxSources.Length;
            _nextSfxSourceIndex = (fallbackIndex + 1) % sfxSources.Length;
            return sfxSources[fallbackIndex];
        }

        private static AudioClip PickClip(GameAudioCueDefinition definition)
        {
            if (definition.Clips == null || definition.Clips.Length == 0)
            {
                return null;
            }

            if (definition.Clips.Length == 1)
            {
                return definition.Clips[0];
            }

            return definition.Clips[Random.Range(0, definition.Clips.Length)];
        }
    }
}
