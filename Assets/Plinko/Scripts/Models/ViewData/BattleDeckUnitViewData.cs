using System;
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
        public int ManaCost;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
