using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class FieldUpgradeReplacementPanelController : MonoBehaviour
    {
        [SerializeField] private FieldUpgradeSelectedPinCardView pendingPinView;
        [SerializeField] private FieldUpgradeSelectedPinCardView selectedPinView;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private FieldUpgradeBridge _fieldUpgradeBridge;
        private FieldUpgradePhaseViewData _viewData = new();
        private bool _listenersBound;

        public void Init(FieldUpgradeBridge fieldUpgradeBridge)
        {
            _fieldUpgradeBridge = fieldUpgradeBridge;
            BindListeners();
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
        {
            _viewData = viewData;
            pendingPinView.Refresh(viewData.PendingPin);
            selectedPinView.Refresh(viewData.SelectedPin);
            confirmButton.interactable = viewData.CanReplace;
            cancelButton.interactable = viewData.CanCancelSelection;
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            confirmButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(confirmButton.transform as RectTransform);
                _fieldUpgradeBridge.RequestReplaceBoardPin();
            });

            cancelButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(cancelButton.transform as RectTransform);
                if (_viewData.SelectedSlotIndex >= 0)
                {
                    _fieldUpgradeBridge.RequestCancelBoardSlotSelection(_viewData.SelectedSlotIndex);
                }
            });

            _listenersBound = true;
        }
    }
}
