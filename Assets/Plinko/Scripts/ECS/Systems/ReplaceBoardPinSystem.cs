using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ReplaceBoardPinSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<ReplaceBoardPinRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<PendingPurchasedPinComponent> _pendingPinPool;
        private EcsPool<BoardSlotSelectionChangedEvent> _boardSlotSelectionChangedEventPool;
        private EcsPool<PlinkoBoardChangedEvent> _plinkoBoardChangedEventPool;

        public ReplaceBoardPinSystem(RunEntityIndex runEntityIndex, InstalledPinIndex installedPinIndex)
        {
            _runEntityIndex = runEntityIndex;
            _installedPinIndex = installedPinIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ReplaceBoardPinRequest>().End();
            _pendingFilter = world.Filter<PendingPurchasedPinComponent>().End();
            _requestPool = world.GetPool<ReplaceBoardPinRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _pendingPinPool = world.GetPool<PendingPurchasedPinComponent>();
            _boardSlotSelectionChangedEventPool = world.GetPool<BoardSlotSelectionChangedEvent>();
            _plinkoBoardChangedEventPool = world.GetPool<PlinkoBoardChangedEvent>();
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
                _requestPool.Get(requestEntity);
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.FieldUpgradePhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var fieldState = ref _fieldUpgradeStatePool.Get(runEntity);
                if (fieldState.SelectedSlotIndex < 0 || !_installedPinIndex.TryGet(fieldState.SelectedSlotIndex, out var installedPinEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var pendingEntities = new List<int>();
                foreach (var pendingEntity in _pendingFilter)
                {
                    pendingEntities.Add(pendingEntity);
                }

                if (pendingEntities.Count == 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var pendingPin = _pendingPinPool.Get(pendingEntities[0]);
                _installedPinPool.Get(installedPinEntity).PinTypeId = pendingPin.PinTypeId;

                foreach (var pendingEntity in pendingEntities)
                {
                    _pendingPinPool.Del(pendingEntity);
                    world.DelEntity(pendingEntity);
                }

                fieldState.SelectedSlotIndex = -1;
                fieldState.IsPlacementHighlighted = false;
                _boardSlotSelectionChangedEventPool.Add(world.NewEntity()).SlotIndex = -1;
                _plinkoBoardChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}
