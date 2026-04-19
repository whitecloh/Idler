using System;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BaseDefenseWavePreviewUnitModel
    {
        public string SpawnId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int MoveRange;
        public int AttackRange;
        public float MoveSpeed;
        public float AttackSpeed;
        public bool CanAttackOtherLines;
        public bool CanMoveBetweenLines;
        public int LaneIndex;
        public int EnemySideCellIndex;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
