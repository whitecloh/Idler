using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchasePhaseScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        public void Show(bool isVisible)
        {
            if (root != null) root.SetActive(isVisible);
        }
        public void Refresh(PurchasePhaseViewData viewData)
        {
            Debug.Log($"Purchase refresh gold={viewData.Gold} offers={viewData.Offers.Count} activeTrainings={viewData.ActiveTrainingCount} canStart={viewData.CanStartBattle}");
        }
    }
}