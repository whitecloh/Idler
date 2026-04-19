using Plinko.Scripts.Models;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLinePlugWorldView : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [SerializeField] private LineRenderer wireRenderer;
        [SerializeField] private GameObject droppedStateRoot;
        [SerializeField] private GameObject carriedStateRoot;
        [SerializeField] private GameObject connectedStateRoot;
        [SerializeField] private float wireSag = 0.35f;
        [SerializeField] private int wirePointCount = 5;

        public Transform RootTransform => root != null ? root : transform;

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

        public void SetWorldPosition(Vector3 worldPosition)
        {
            RootTransform.position = worldPosition;
        }

        public void SetWire(Vector3 startWorldPosition, Vector3 endWorldPosition, bool isConnected)
        {
            if (wireRenderer == null)
            {
                return;
            }

            var pointCount = Mathf.Max(2, wirePointCount);
            wireRenderer.positionCount = pointCount;
            var sag = isConnected ? wireSag * 0.15f : wireSag;
            for (var index = 0; index < pointCount; index++)
            {
                var t = pointCount == 1 ? 0f : index / (float)(pointCount - 1);
                var position = Vector3.Lerp(startWorldPosition, endWorldPosition, t);
                position += Vector3.down * (Mathf.Sin(t * Mathf.PI) * sag);
                wireRenderer.SetPosition(index, position);
            }
        }
    }
}
