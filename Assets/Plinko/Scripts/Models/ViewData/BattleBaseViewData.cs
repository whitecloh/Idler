using System;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleBaseViewData
    {
        public Sprite Sprite;
        public int CurrentHealth;
        public int MaxHealth;
    }
}
