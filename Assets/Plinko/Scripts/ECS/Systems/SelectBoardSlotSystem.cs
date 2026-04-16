using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class SelectBoardSlotSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<SelectBoardSlotRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<BoardSlotSelectionChangedEvent> _boardSlotSelectionChangedEventPool;

        public SelectBoardSlotSystem(RunEntityIndex runEntityIndex, InstalledPinIndex installedPinIndex)
        {
            _runEntityIndex = runEntityIndex;
            _installedPinIndex = installedPinIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<SelectBoardSlotRequest>().End();
            _pendingFilter = world.Filter<PendingPurchasedPinComponent>().End();
            _requestPool = world.GetPool<SelectBoardSlotRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _boardSlotSelectionChangedEventPool = world.GetPool<BoardSlotSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.FieldUpgradePhase || !_installedPinIndex.TryGet(request.SlotIndex, out _))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var hasPendingPin = false;
                foreach (var _ in _pendingFilter)
                {
                    hasPendingPin = true;
                    break;
                }

                ref var fieldState = ref _fieldUpgradeStatePool.Get(runEntity);
                var nextSelectedSlot = fieldState.SelectedSlotIndex == request.SlotIndex ? -1 : request.SlotIndex;
                fieldState.SelectedSlotIndex = nextSelectedSlot;
                fieldState.IsPlacementHighlighted = hasPendingPin && nextSelectedSlot >= 0;
                _boardSlotSelectionChangedEventPool.Add(world.NewEntity()).SlotIndex = nextSelectedSlot;
                world.DelEntity(requestEntity);
            }
        }
    }
}
