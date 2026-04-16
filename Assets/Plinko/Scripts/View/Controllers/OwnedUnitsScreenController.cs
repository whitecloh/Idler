using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class OwnedUnitsScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        public void Show(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
            }
        }

        public void Refresh(IReadOnlyList<OwnedUnitViewData> ownedUnits)
        {
            Debug.Log($"Owned units refresh count={ownedUnits.Count}");
        }
    }
}
