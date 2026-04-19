using System;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class BaseDefenseGridCellView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform unitAnchor;
        [SerializeField] private Button button;
        [SerializeField] private GameObject availableStateRoot;
        [SerializeField] private GameObject selectedStateRoot;
        [SerializeField] private GameObject blockedStateRoot;

        private Action<BaseDefenseGridCellView> _clicked;

        public RectTransform RectTransform => root;
        public RectTransform UnitAnchor => unitAnchor;

        public void Bind(Action<BaseDefenseGridCellView> clicked)
        {
            _clicked = clicked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClicked);
        }

        public void SetState(bool interactable, bool isAvailable, bool isSelected, bool isBlocked)
        {
            button.interactable = interactable;
            availableStateRoot.SetActive(isAvailable);
            selectedStateRoot.SetActive(isSelected);
            blockedStateRoot.SetActive(isBlocked);
        }

        private void HandleClicked()
        {
            _clicked?.Invoke(this);
        }
    }
}
