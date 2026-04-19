using System;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BaseDefenseUnitStateModel
    {
        public int RuntimeId;
        public int SourceOwnedUnitRuntimeId;
        public string SpawnId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int ManaCost;
        public int MoveRange;
        public int AttackRange;
        public float MoveSpeed;
        public float AttackSpeed;
        public bool CanAttackOtherLines;
        public bool CanMoveBetweenLines;
        public int LaneIndex;
        public int CellIndex;
        public bool IsEnemy;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
