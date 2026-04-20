using System;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class StatDisplayViewData
    {
        public string StatTypeId;
        public string DisplayName;
        public Sprite Icon;
        public string ValueText;
    }
}
