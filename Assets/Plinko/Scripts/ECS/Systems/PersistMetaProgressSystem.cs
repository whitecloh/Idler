using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class PersistMetaProgressSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnlocksService _unlocksService;
        private readonly MetaSaveService _metaSaveService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _levelCompletedFilter;
        private EcsFilter _runCompletedFilter;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;

        public PersistMetaProgressSystem(
            UnlocksService unlocksService,
            MetaSaveService metaSaveService,
            RunEntityIndex runEntityIndex)
        {
            _unlocksService = unlocksService;
            _metaSaveService = metaSaveService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _levelCompletedFilter = world.Filter<LevelCompletedEvent>().End();
            _runCompletedFilter = world.Filter<RunCompletedEvent>().End();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var hasChanges = false;
            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _levelPool.Get(runEntity).LevelIndex;

            foreach (var _ in _levelCompletedFilter)
            {
                _unlocksService.MarkLevelCompleted(locationId, levelIndex);
                hasChanges = true;
            }

            foreach (var _ in _runCompletedFilter)
            {
                _unlocksService.MarkLocationCompleted(locationId);
                hasChanges = true;
            }

            if (hasChanges)
            {
                _metaSaveService.Save(_unlocksService.ExportProgress());
            }
        }
    }
}
