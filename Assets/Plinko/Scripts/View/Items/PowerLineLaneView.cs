using System;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.View.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PowerLineLaneView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform lineStartAnchor;
        [SerializeField] private RectTransform lineEndAnchor;
        [SerializeField] private Button spawnButton;
        [SerializeField] private GameObject availableStateRoot;
        [SerializeField] private GameObject selectedStateRoot;
        [SerializeField] private GameObject connectedStateRoot;
        [SerializeField] private GameObject disabledStateRoot;

        private Action<PowerLineLaneView> _clicked;

        public RectTransform Root => root;
        public RectTransform ContentRoot => contentRoot;

        public void Bind(Action<PowerLineLaneView> clicked)
        {
            _clicked = clicked;
            spawnButton.onClick.RemoveAllListeners();
            spawnButton.onClick.AddListener(() => _clicked?.Invoke(this));
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

            spawnButton.interactable = isAvailable;
        }

        public Vector2 GetAnchoredPosition(RectTransform targetRoot, Camera uiCamera, float normalizedPosition, float yOffset = 0f)
        {
            var clamped = Mathf.Clamp01(normalizedPosition);
            var worldPosition = Vector3.Lerp(lineStartAnchor.position, lineEndAnchor.position, clamped);
            return UiRectTransformUtility.WorldToAnchoredPosition(targetRoot, uiCamera, worldPosition) + new Vector2(0f, yOffset);
        }

        public float GetNormalizedDistance(Vector3 worldPosition)
        {
            var start = lineStartAnchor.position;
            var end = lineEndAnchor.position;
            var fullDistance = Vector3.Distance(start, end);
            if (fullDistance <= 0.001f)
            {
                return 0f;
            }

            return Mathf.Clamp01(Vector3.Distance(start, worldPosition) / fullDistance);
        }
    }
}
