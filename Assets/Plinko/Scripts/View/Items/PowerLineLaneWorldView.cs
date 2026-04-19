using Plinko.Scripts.Data.Common;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineLaneWorldView : MonoBehaviour
    {
        [SerializeField] private Enums.PowerLineLane lane;
        [SerializeField] private Transform root;
        [SerializeField] private Transform unitStartAnchor;
        [SerializeField] private Transform unitEndAnchor;
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private Transform wireStartAnchor;
        [SerializeField] private Transform plugSocketAnchor;
        [SerializeField] private GameObject availableStateRoot;
        [SerializeField] private GameObject selectedStateRoot;
        [SerializeField] private GameObject connectedStateRoot;
        [SerializeField] private GameObject disabledStateRoot;

        public Enums.PowerLineLane Lane => lane;
        public Transform RootTransform => root != null ? root : transform;

        public Vector3 GetSpawnWorldPosition()
        {
            return spawnAnchor != null ? spawnAnchor.position : unitStartAnchor.position;
        }

        public Vector3 GetWireStartWorldPosition()
        {
            return wireStartAnchor != null ? wireStartAnchor.position : GetSpawnWorldPosition();
        }

        public Vector3 GetPlugSocketWorldPosition()
        {
            return plugSocketAnchor != null ? plugSocketAnchor.position : unitEndAnchor.position;
        }

        public Vector3 GetWorldPosition(float normalizedPosition, float lateralOffset = 0f)
        {
            var start = unitStartAnchor.position;
            var end = unitEndAnchor.position;
            var clamped = Mathf.Clamp01(normalizedPosition);
            var basePosition = Vector3.Lerp(start, end, clamped);
            if (Mathf.Abs(lateralOffset) <= 0.001f)
            {
                return basePosition;
            }

            var direction = (end - start).normalized;
            var perpendicular = new Vector3(-direction.y, direction.x, 0f);
            return basePosition + perpendicular * lateralOffset;
        }

        public void SetState(bool isSelected, bool isAvailable, bool isConnected, bool isDisabled)
        {
            if (availableStateRoot != null)
            {
                availableStateRoot.SetActive(isAvailable);
            }

            if (selectedStateRoot != null)
            {
                selectedStateRoot.SetActive(isSelected);
            }

            if (connectedStateRoot != null)
            {
                connectedStateRoot.SetActive(isConnected);
            }

            if (disabledStateRoot != null)
            {
                disabledStateRoot.SetActive(isDisabled);
            }
        }
    }
}
