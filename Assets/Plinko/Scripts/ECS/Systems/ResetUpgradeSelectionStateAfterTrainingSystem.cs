using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ResetUpgradeSelectionStateAfterTrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _trainingCompletedFilter;
        private EcsFilter _selectedOwnedUnitFilter;
        private EcsPool<SelectedForUpgradeComponent> _selectedPool;
        private EcsPool<UpgradePhaseStateComponent> _upgradePhaseStatePool;
        private EcsPool<UpgradeSelectionChangedEvent> _selectionChangedEventPool;

        public ResetUpgradeSelectionStateAfterTrainingSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _trainingCompletedFilter = world.Filter<TrainingCompletedEvent>().End();
            _selectedOwnedUnitFilter = world.Filter<OwnedUnitComponent>().Inc<SelectedForUpgradeComponent>().End();
            _selectedPool = world.GetPool<SelectedForUpgradeComponent>();
            _upgradePhaseStatePool = world.GetPool<UpgradePhaseStateComponent>();
            _selectionChangedEventPool = world.GetPool<UpgradeSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (_trainingCompletedFilter.GetEntitiesCount() <= 0)
            {
                return;
            }

            var hadSelection = false;
            foreach (var ownedUnitEntity in _selectedOwnedUnitFilter)
            {
                _selectedPool.Del(ownedUnitEntity);
                hadSelection = true;
            }

            if (!hadSelection || !_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            ref var state = ref _upgradePhaseStatePool.GetOrAdd(runEntity);
            state.SelectedCount = 0;
            state.IsSelectionLocked = false;
            _selectionChangedEventPool.Add(world.NewEntity()).SelectedCount = 0;
        }
    }
}