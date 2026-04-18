using System;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class EnemyUnitSnapshotModel
    {
        public string SpawnId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int BoardX;
        public int BoardY;
        public int MoveRange;
        public int AttackRange;
        public Sprite PortraitSprite;
        public Sprite TrainingFieldSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
