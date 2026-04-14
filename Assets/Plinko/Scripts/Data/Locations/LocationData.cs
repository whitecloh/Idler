using System.Collections.Generic;
using Plinko.Scripts.Data.Levels;
using UnityEngine;

namespace Plinko.Scripts.Data.Locations
{
    [CreateAssetMenu(menuName = "Session/Location", fileName = "LocationData")]
    public sealed class LocationData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private List<LevelData> levels = new();

        public string Id => id;
        public string DisplayName => displayName;
        public IReadOnlyList<LevelData> Levels => levels;
    }
}