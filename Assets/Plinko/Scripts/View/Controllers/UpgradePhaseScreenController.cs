using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class UpgradePhaseScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        public void Show(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        public void Refresh(UpgradePhaseViewData viewData)
        {
            Debug.Log($"UpgradePhaseScreenController.Refresh Selected={viewData.SelectedCount} CanConfirm={viewData.CanConfirm} Units={viewData.OwnedUnits.Count}");
        }
    }
}