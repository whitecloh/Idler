using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class EnterUpgradePhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsPool<PhaseChangedEvent> _phaseChangedPool;
        private EcsPool<UpgradePhaseStateComponent> _upgradePhaseStatePool;
        private EcsPool<UpgradePhaseEnteredEvent> _upgradePhaseEnteredEventPool;
        private EcsPool<UpgradeSelectionChangedEvent> _upgradeSelectionChangedEventPool;

        public EnterUpgradePhaseSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _phaseChangedPool = world.GetPool<PhaseChangedEvent>();
            _upgradePhaseStatePool = world.GetPool<UpgradePhaseStateComponent>();
            _upgradePhaseEnteredEventPool = world.GetPool<UpgradePhaseEnteredEvent>();
            _upgradeSelectionChangedEventPool = world.GetPool<UpgradeSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _phaseChangedFilter)
            {
                if (_phaseChangedPool.Get(eventEntity).Value != Enums.PhaseType.UpgradePhase)
                {
                    continue;
                }

                ref var state = ref _upgradePhaseStatePool.GetOrAdd(runEntity);
                state.SelectedCount = 0;
                state.IsSelectionLocked = false;

                _upgradePhaseEnteredEventPool.Add(world.NewEntity());
                _upgradeSelectionChangedEventPool.Add(world.NewEntity()).SelectedCount = 0;
            }
        }
    }
}