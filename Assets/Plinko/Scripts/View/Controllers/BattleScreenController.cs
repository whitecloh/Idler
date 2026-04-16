using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        public void Show(bool isVisible)
        {
            if (root != null) root.SetActive(isVisible);
        }
        public void Refresh(BattleHudViewData viewData)
        {
            Debug.Log($"Battle refresh mana={viewData.CurrentMana} turn={viewData.CurrentTurn} hand={viewData.HandCards.Count} wave={viewData.ActiveEnemyWaveDebug}");
        }
    }
}