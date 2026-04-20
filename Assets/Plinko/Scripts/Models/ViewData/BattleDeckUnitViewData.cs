using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleDeckUnitViewData
    {
        public int RuntimeId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int MaxHealth;
        public int ManaCost;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
        public List<StatDisplayViewData> Stats = new();
    }
}
