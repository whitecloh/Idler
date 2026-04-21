using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class FieldUpgradeBoardPinView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        private BoardSlotViewData _viewData = new();

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
            _viewData = viewData;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowPin(this, _viewData.TooltipText, new FieldUpgradeSelectedPinViewData
            {
                PinTypeId = _viewData.PinTypeId,
                DisplayName = _viewData.DisplayName,
                Sprite = _viewData.Sprite,
                ModifierLines = _viewData.ModifierLines
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.Hide(this);
        }

        private void OnDisable()
        {
            StopAvailableLoop();
            UiTooltipManager.Instance?.Hide(this);
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
