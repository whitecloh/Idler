using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ReplaceBoardPinSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _candidateFilter;
        private EcsFilter _installedPinFilter;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePhaseStatePool;
        private EcsPool<BoughtPinCandidateComponent> _candidatePool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<PlinkoBoardChangedEvent> _plinkoBoardChangedEventPool;
        private EcsPool<BoardSlotSelectionChangedEvent> _boardSlotSelectionChangedEventPool;

        public ReplaceBoardPinSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ReplaceBoardPinRequest>().End();
            _candidateFilter = world.Filter<BoughtPinCandidateComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _fieldUpgradePhaseStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _candidatePool = world.GetPool<BoughtPinCandidateComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _plinkoBoardChangedEventPool = world.GetPool<PlinkoBoardChangedEvent>();
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
                if (_candidateFilter.GetEntitiesCount() <= 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var selectedSlotIndex = _fieldUpgradePhaseStatePool.GetOrAdd(runEntity).SelectedSlotIndex;
                if (selectedSlotIndex < 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                string pinTypeId = string.Empty;
                var candidateEntityToDelete = -1;
                foreach (var candidateEntity in _candidateFilter)
                {
                    pinTypeId = _candidatePool.Get(candidateEntity).PinTypeId;
                    candidateEntityToDelete = candidateEntity;
                    break;
                }

                foreach (var installedPinEntity in _installedPinFilter)
                {
                    ref var installedPin = ref _installedPinPool.Get(installedPinEntity);
                    if (installedPin.GlobalIndex != selectedSlotIndex)
                    {
                        continue;
                    }

                    installedPin.PinTypeId = pinTypeId;
                    break;
                }

                if (candidateEntityToDelete >= 0)
                {
                    world.DelEntity(candidateEntityToDelete);
                }

                _fieldUpgradePhaseStatePool.Get(runEntity).SelectedSlotIndex = -1;
                _plinkoBoardChangedEventPool.Add(world.NewEntity());
                _boardSlotSelectionChangedEventPool.Add(world.NewEntity()).SlotIndex = -1;
                world.DelEntity(requestEntity);
            }
        }
    }
}