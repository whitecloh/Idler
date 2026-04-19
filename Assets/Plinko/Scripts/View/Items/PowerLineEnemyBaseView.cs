using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineEnemyBaseView : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private GameObject[] connectedSocketStateRoots;

        public void Refresh(Sprite sprite, int connectedCount, int requiredCount, IReadOnlyList<PowerLineLaneViewData> lanes)
        {
            baseImage.sprite = sprite;
            baseImage.enabled = sprite != null;
            if (progressText != null)
            {
                progressText.text = $"{connectedCount}/{requiredCount}";
            }

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
