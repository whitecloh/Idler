using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLinePlugView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private GameObject droppedStateRoot;
        [SerializeField] private GameObject carriedStateRoot;
        [SerializeField] private GameObject connectedStateRoot;

        public RectTransform RectTransform => root;

        public void Refresh(PowerLinePlugViewData viewData)
        {
            if (droppedStateRoot != null)
            {
                droppedStateRoot.SetActive(viewData.Status == PowerLinePlugStatus.AtSpawn || viewData.Status == PowerLinePlugStatus.Dropped);
            }

            if (carriedStateRoot != null)
            {
                carriedStateRoot.SetActive(viewData.Status == PowerLinePlugStatus.Carried);
            }

            if (connectedStateRoot != null)
            {
                connectedStateRoot.SetActive(viewData.Status == PowerLinePlugStatus.Connected);
            }
        }
    }
}
