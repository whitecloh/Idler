using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ConfirmRetrainingSelectionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _selectedFilter;
        private EcsPool<ConfirmRetrainingSelectionRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<RetrainingSelectionChangedEvent> _selectionChangedEventPool;
        private EcsPool<RetrainingSelectionConfirmedEvent> _selectionConfirmedEventPool;

        public ConfirmRetrainingSelectionSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ConfirmRetrainingSelectionRequest>().End();
            _selectedFilter = world.Filter<SelectedForRetrainingComponent>().End();
            _requestPool = world.GetPool<ConfirmRetrainingSelectionRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _selectionChangedEventPool = world.GetPool<RetrainingSelectionChangedEvent>();
            _selectionConfirmedEventPool = world.GetPool<RetrainingSelectionConfirmedEvent>();
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
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var retrainingState = ref _retrainingStatePool.Get(runEntity);
                if (retrainingState.IsSelectionLocked || retrainingState.ActiveTrainingCount > 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var selectedCount = 0;
                foreach (var _ in _selectedFilter)
                {
                    selectedCount++;
                }

                if (retrainingState.SelectedCount != selectedCount)
                {
                    retrainingState.SelectedCount = selectedCount;
                    _selectionChangedEventPool.Add(world.NewEntity()).SelectedCount = selectedCount;
                }

                if (selectedCount <= 0 || retrainingState.SelectionLimit <= 0 || selectedCount > retrainingState.SelectionLimit)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                retrainingState.IsSelectionLocked = true;
                retrainingState.ActiveTrainingCount = selectedCount;
                _selectionConfirmedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}
