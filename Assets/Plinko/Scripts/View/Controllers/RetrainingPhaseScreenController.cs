using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class RetrainingPhaseScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        public void Show(bool isVisible)
        {
            if (root != null) root.SetActive(isVisible);
        }
        public void Refresh(RetrainingPhaseViewData viewData)
        {
            Debug.Log($"Retraining refresh selected={viewData.SelectedCount}/{viewData.SelectionLimit} activeTrainings={viewData.ActiveTrainingCount}");
        }
    }
}