using System;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleBoardUnitViewData
    {
        public int RuntimeId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int ManaCost;
        public int BoardIndex;
        public int LaneIndex;
        public int CellIndex;
        public bool IsEnemy;
        public bool IsPreview;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
