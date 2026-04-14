using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class OwnedUnitsScreenController : MonoBehaviour
    {
        public void Refresh(IReadOnlyList<OwnedUnitViewData> ownedUnits)
        {
            Debug.Log($"OwnedUnitsScreenController.Refresh Count={ownedUnits.Count}");
        }
    }
}