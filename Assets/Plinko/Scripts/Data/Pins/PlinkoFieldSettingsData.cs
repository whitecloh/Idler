using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [CreateAssetMenu(menuName = "Session/PlinkoFieldSettings", fileName = "PlinkoFieldSettingsData")]
    public sealed class PlinkoFieldSettingsData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private List<PlinkoBoardRowData> rows = new();
        [SerializeField] private List<BasketTypeData> baskets = new();
        [SerializeField] private float horizontalSpacing = 1f;
        [SerializeField] private float verticalSpacing = 1f;

        public string Id => id;
        public IReadOnlyList<PlinkoBoardRowData> Rows => rows;
        public IReadOnlyList<BasketTypeData> Baskets => baskets;
        public float HorizontalSpacing => horizontalSpacing;
        public float VerticalSpacing => verticalSpacing;
    }
}