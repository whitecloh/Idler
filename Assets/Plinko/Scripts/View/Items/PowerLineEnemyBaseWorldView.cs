using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineEnemyBaseWorldView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject[] connectedSocketStateRoots;

        public Transform RootTransform => spriteRenderer != null ? spriteRenderer.transform : transform;

        public void Refresh(Sprite sprite, IReadOnlyList<PowerLineLaneViewData> lanes)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = sprite != null;

            if (connectedSocketStateRoots == null)
            {
                return;
            }

            for (var index = 0; index < connectedSocketStateRoots.Length; index++)
            {
                var isConnected = lanes != null && index < lanes.Count && lanes[index].IsConnected;
                if (connectedSocketStateRoots[index] != null)
                {
                    connectedSocketStateRoots[index].SetActive(isConnected);
                }
            }
        }
    }
}
