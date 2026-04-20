using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.View.Audio
{
    [CreateAssetMenu(menuName = "Plinko/Audio/GameAudioLibrary", fileName = "GameAudioLibrary")]
    public sealed class GameAudioLibrary : ScriptableObject
    {
        [SerializeField] private List<GameAudioCueDefinition> cues = new();

        public IReadOnlyList<GameAudioCueDefinition> Cues => cues;
    }

    [Serializable]
    public sealed class GameAudioCueDefinition
    {
        [SerializeField] private GameAudioCueType cueType;
        [SerializeField] private AudioClip[] clips;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private Vector2 pitchRange = Vector2.one;
        [SerializeField] private bool loop;

        public GameAudioCueType CueType => cueType;
        public AudioClip[] Clips => clips;
        public float Volume => volume;
        public Vector2 PitchRange => pitchRange;
        public bool Loop => loop;
    }
}
