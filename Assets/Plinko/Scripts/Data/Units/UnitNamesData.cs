using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Data.Units
{
    [CreateAssetMenu(menuName = "Session/UnitNames", fileName = "UnitNamesData")]
    public sealed class UnitNamesData : ScriptableObject
    {
        [SerializeField] private List<string> names = new();

        public IReadOnlyList<string> Names => names;
    }
}