using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class FieldUpgradePhaseScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        public void Show(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
        {
            Debug.Log($"FieldUpgradePhaseScreenController.Refresh Gold={viewData.Gold} Offers={viewData.Offers.Count} Slots={viewData.Slots.Count} CanReplace={viewData.CanReplace}");
        }
    }
}