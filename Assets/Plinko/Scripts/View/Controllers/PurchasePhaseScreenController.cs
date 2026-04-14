using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchasePhaseScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        public void Show(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        public void Refresh(PurchasePhaseViewData viewData)
        {
            Debug.Log($"PurchasePhaseScreenController.Refresh Gold={viewData.Gold} Offers={viewData.Offers.Count} HasStaged={viewData.HasStagedUnits}");
        }
    }
}