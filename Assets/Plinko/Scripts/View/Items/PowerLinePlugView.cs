using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLinePlugView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform wireBody;
        [SerializeField] private Image wireImage;
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

        public void SetWire(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition)
        {
            if (wireBody == null)
            {
                return;
            }

            var delta = endAnchoredPosition - startAnchoredPosition;
            var distance = delta.magnitude;
            wireBody.anchoredPosition = startAnchoredPosition + delta * 0.5f;
            wireBody.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            var size = wireBody.sizeDelta;
            size.x = distance;
            wireBody.sizeDelta = size;
            wireBody.gameObject.SetActive(distance > 0.01f);

            if (wireImage != null)
            {
                wireImage.enabled = distance > 0.01f;
            }
        }
    }
}
