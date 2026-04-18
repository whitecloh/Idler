using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class FieldUpgradeBoardPinView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject availableStateRoot;
        [SerializeField] private RectTransform availableIndicatorRoot;
        [SerializeField] private GameObject selectedStateRoot;
        [SerializeField] private GameObject notSelectedStateRoot;

        private int _slotIndex;
        private bool _isAvailableLoopPlaying;
        private Vector2 _availableBasePosition;

        public RectTransform RectTransform => root;
        public int SlotIndex => _slotIndex;

        private void Awake()
        {
            _availableBasePosition = availableIndicatorRoot.anchoredPosition;
        }

        public void Bind(int slotIndex, System.Action<int> onSelected)
        {
            _slotIndex = slotIndex;
            selectButton.onClick.AddListener(() => onSelected.Invoke(_slotIndex));
        }

        public void Refresh(BoardSlotViewData viewData)
        {
            _slotIndex = viewData.SlotIndex;
            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;

            var showAvailable = viewData.IsAvailableForReplacement;
            var showSelected = viewData.IsSelectedForReplacement;
            var showNotSelected = viewData.IsNotSelectedForReplacement;

            availableStateRoot.SetActive(showAvailable);
            selectedStateRoot.SetActive(showSelected);
            notSelectedStateRoot.SetActive(showNotSelected);
            selectButton.interactable = showAvailable || showSelected || showNotSelected;

            if (showAvailable && !_isAvailableLoopPlaying)
            {
                PlayAvailableLoop();
            }
            else if (!showAvailable && _isAvailableLoopPlaying)
            {
                StopAvailableLoop();
            }
        }

        private void OnDisable()
        {
            StopAvailableLoop();
        }

        private void PlayAvailableLoop()
        {
            _isAvailableLoopPlaying = true;
            availableIndicatorRoot.anchoredPosition = _availableBasePosition;
            UiAnimationManager.Instance.PlayBounceLoopY(availableIndicatorRoot, "available", 12f, 0.35f);
        }

        private void StopAvailableLoop()
        {
            _isAvailableLoopPlaying = false;
            UiAnimationManager.Instance.Stop(availableIndicatorRoot, "available");
            availableIndicatorRoot.anchoredPosition = _availableBasePosition;
        }
    }
}
