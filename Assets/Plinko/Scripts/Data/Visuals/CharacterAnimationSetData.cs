using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Data.Visuals
{
    [Serializable]
    public sealed class CharacterAnimationSetData
    {
        [SerializeField] private Sprite[] idleFrames = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] runFrames = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] attackFrames = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] hitFrames = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] castFrames = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] deathFrames = Array.Empty<Sprite>();

        public IReadOnlyList<Sprite> IdleFrames => idleFrames;
        public IReadOnlyList<Sprite> RunFrames => runFrames;
        public IReadOnlyList<Sprite> AttackFrames => attackFrames;
        public IReadOnlyList<Sprite> HitFrames => hitFrames;
        public IReadOnlyList<Sprite> CastFrames => castFrames;
        public IReadOnlyList<Sprite> DeathFrames => deathFrames;
    }
}
