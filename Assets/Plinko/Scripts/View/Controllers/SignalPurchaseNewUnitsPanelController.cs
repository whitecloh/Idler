using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class SignalPurchaseNewUnitsPanelController : MonoBehaviour
    {
        [SerializeField] private SignalPurchasePendingUnitSlotView[] slots;
        [SerializeField] private Button launchSignalButton;
        [SerializeField] private TMP_Text launchSignalLabelText;

        private readonly Dictionary<int, CompletedSlotState> _completedStatesByRuntimeId = new();
        private readonly Dictionary<int, int> _slotByRuntimeId = new();
        private readonly HashSet<int> _pendingRuntimeIds = new();
        private readonly HashSet<int> _announcedPendingRuntimeIds = new();
        private SignalPurchaseBridge _signalPurchaseBridge;
        private bool _listenersBound;

        public bool HasCompletedCardsReadyForTransfer => _completedStatesByRuntimeId.Count > 0;

        public void Init(SignalPurchaseBridge signalPurchaseBridge)
        {
            _signalPurchaseBridge = signalPurchaseBridge;
            BindListeners();
        }

        public void ResetState()
        {
            _completedStatesByRuntimeId.Clear();
            _slotByRuntimeId.Clear();
            _pendingRuntimeIds.Clear();
            _announcedPendingRuntimeIds.Clear();

            if (slots == null)
            {
                return;
            }

            for (var index = 0; index < slots.Length; index++)
            {
                if (slots[index] != null)
                {
                    slots[index].ShowEmpty(true);
                }
            }
        }

        public void Refresh(SignalPurchasePhaseViewData viewData)
        {
            var pendingBySlot = new Dictionary<int, SignalPurchasePendingUnitCardViewData>();
            for (var index = 0; index < viewData.PendingUnits.Count; index++)
            {
                pendingBySlot[viewData.PendingUnits[index].SlotIndex] = viewData.PendingUnits[index];
            }

            _slotByRuntimeId.Clear();
            _pendingRuntimeIds.Clear();

            for (var slotIndex = 0; slotIndex < slots.Length; slotIndex++)
            {
                var slotView = slots[slotIndex];
                if (slotView == null)
                {
                    continue;
                }

                if (slotIndex >= viewData.PendingUnitSlotCount)
                {
                    slotView.Root.gameObject.SetActive(false);
                    continue;
                }

                if (TryGetCompletedState(slotIndex, out var completedState))
                {
                    slotView.Refresh(completedState.CardData);
                    _slotByRuntimeId[completedState.RuntimeId] = completedState.SlotIndex;
                    continue;
                }

                if (pendingBySlot.TryGetValue(slotIndex, out var pendingUnit))
                {
                    slotView.Refresh(pendingUnit);
                    _slotByRuntimeId[pendingUnit.RuntimeId] = slotIndex;
                    _pendingRuntimeIds.Add(pendingUnit.RuntimeId);
                    if (_announcedPendingRuntimeIds.Add(pendingUnit.RuntimeId))
                    {
                        AudioManager.Instance?.Play(GameAudioCueType.CardAppear);
                    }
                }
                else
                {
                    slotView.ShowEmpty(true);
                }
            }

            if (launchSignalLabelText != null)
            {
                launchSignalLabelText.text = viewData.IsGeneratorBroken ? "Generator Broken" : "Launch Signal";
            }

            launchSignalButton.interactable = viewData.CanLaunchSignal;
        }

        public Dictionary<int, Vector3> BuildPendingCardTargets()
        {
            var result = new Dictionary<int, Vector3>();
            foreach (var runtimeId in _pendingRuntimeIds)
            {
                if (!_slotByRuntimeId.TryGetValue(runtimeId, out var slotIndex) ||
                    slotIndex < 0 ||
                    slotIndex >= slots.Length ||
                    slots[slotIndex] == null)
                {
                    continue;
                }

                result[runtimeId] = slots[slotIndex].GetCardWorldCenter();
            }

            return result;
        }

        public void ApplyCompletedTrainings(IReadOnlyList<PurchaseTrainingCompletionVisualPayload> completions)
        {
            for (var index = 0; index < completions.Count; index++)
            {
                var completion = completions[index];
                if (!_slotByRuntimeId.TryGetValue(completion.RuntimeId, out var slotIndex) ||
                    slotIndex < 0 ||
                    slotIndex >= slots.Length ||
                    slots[slotIndex] == null)
                {
                    continue;
                }

                var state = new CompletedSlotState
                {
                    RuntimeId = completion.RuntimeId,
                    SlotIndex = slotIndex,
                    CardData = completion.CardData
                };
                _completedStatesByRuntimeId[completion.RuntimeId] = state;
                _pendingRuntimeIds.Remove(completion.RuntimeId);

                slots[slotIndex].Refresh(completion.CardData);
                slots[slotIndex].PlayCardPunch(0.8f);
                AudioManager.Instance?.Play(GameAudioCueType.Upgrade);
            }
        }

        public HashSet<int> GetCompletedRuntimeIds()
        {
            return new HashSet<int>(_completedStatesByRuntimeId.Keys);
        }

        public void ClearCompletedCards()
        {
            if (slots != null)
            {
                foreach (var state in _completedStatesByRuntimeId.Values)
                {
                    if (state.SlotIndex < 0 || state.SlotIndex >= slots.Length || slots[state.SlotIndex] == null)
                    {
                        continue;
                    }

                    slots[state.SlotIndex].ShowEmpty(true);
                }
            }

            _completedStatesByRuntimeId.Clear();
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            launchSignalButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(launchSignalButton.transform as RectTransform);
                AudioManager.Instance?.Play(GameAudioCueType.LaunchSignal);
                _signalPurchaseBridge.RequestLaunchSignal();
            });
            _listenersBound = true;
        }

        private bool TryGetCompletedState(int slotIndex, out CompletedSlotState state)
        {
            foreach (var pair in _completedStatesByRuntimeId)
            {
                if (pair.Value.SlotIndex == slotIndex)
                {
                    state = pair.Value;
                    return true;
                }
            }

            state = null;
            return false;
        }

        private sealed class CompletedSlotState
        {
            public int RuntimeId;
            public int SlotIndex;
            public PurchaseTrainedUnitCardViewData CardData;
        }
    }
}
