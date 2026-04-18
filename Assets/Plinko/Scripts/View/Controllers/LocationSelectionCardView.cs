using System;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class LocationSelectionCardView : MonoBehaviour
    {
        [SerializeField] private Button cardButton;
        [SerializeField] private Image artImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text unlockText;
        [SerializeField] private GameObject selectedStateRoot;
        [SerializeField] private GameObject lockedStateRoot;
        [SerializeField] private GameObject completeStateRoot;

        private LocationEntryViewData _viewData;
        private Action<string> _onSelected;
        private bool _listenersBound;

        public string LocationId => _viewData?.LocationId;

        public void Bind(LocationEntryViewData viewData, Action<string> onSelected)
        {
            _viewData = viewData;
            _onSelected = onSelected;
            
            BindListeners();
            RefreshSelection(false);
        }

        public void RefreshSelection(bool isSelected)
        {
            titleText.text = string.IsNullOrWhiteSpace(_viewData.DisplayName) ? _viewData.LocationId : _viewData.DisplayName;
            unlockText.text = _viewData.UnlockDescription;
            unlockText.gameObject.SetActive(!_viewData.IsUnlocked && !string.IsNullOrWhiteSpace(_viewData.UnlockDescription));
            artImage.sprite = _viewData.Art;
            artImage.enabled = _viewData.Art != null;
            artImage.preserveAspect = true;
            selectedStateRoot.SetActive(isSelected);
            lockedStateRoot.SetActive(!_viewData.IsUnlocked);
            completeStateRoot.SetActive(_viewData.IsCompleted);

            var canSelect = _viewData.IsUnlocked && !isSelected;
            cardButton.interactable = canSelect;
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            cardButton.onClick.AddListener(HandleSelected);

            _listenersBound = true;
        }

        private void HandleSelected()
        {
            if (_viewData == null || !_viewData.IsUnlocked)
            {
                return;
            }

            UiAnimationManager.Instance.PlaySpringPunch(cardButton.transform as RectTransform);
            _onSelected.Invoke(_viewData.LocationId);
        }
    }
}
