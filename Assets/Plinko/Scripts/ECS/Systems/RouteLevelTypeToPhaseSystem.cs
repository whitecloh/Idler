using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RouteLevelTypeToPhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _levelLoadedFilter;
        private EcsPool<LevelLoadedEvent> _levelLoadedPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedPool;

        public RouteLevelTypeToPhaseSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _levelLoadedFilter = world.Filter<LevelLoadedEvent>().End();
            _levelLoadedPool = world.GetPool<LevelLoadedEvent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _phaseChangedPool = world.GetPool<PhaseChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _levelLoadedFilter)
            {
                var phaseType = GetPhaseFromLevelType(_levelLoadedPool.Get(eventEntity).LevelType);
                _phasePool.GetOrAdd(runEntity).Value = phaseType;
                _phaseChangedPool.Add(world.NewEntity()).Value = phaseType;
                world.DelEntity(eventEntity);
            }
        }

        private static Enums.PhaseType GetPhaseFromLevelType(Enums.LevelType levelType)
        {
            return levelType switch
            {
                Enums.LevelType.Purchase => Enums.PhaseType.PurchasePhase,
                Enums.LevelType.Upgrade => Enums.PhaseType.UpgradePhase,
                Enums.LevelType.FieldUpgrade => Enums.PhaseType.FieldUpgradePhase,
                _ => Enums.PhaseType.None
            };
        }
    }
}