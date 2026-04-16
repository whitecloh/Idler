using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleResultScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        public void Show(bool isVisible)
        {
            if (root != null) root.SetActive(isVisible);
        }
        public void Refresh(BattleResultViewData viewData)
        {
            Debug.Log($"Result refresh victory={viewData.IsVictory} defeat={viewData.IsDefeat} completed={viewData.IsRunCompleted}");
        }
    }
}