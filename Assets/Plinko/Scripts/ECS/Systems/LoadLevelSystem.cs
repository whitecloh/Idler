using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class LoadLevelSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartLevelRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBaseHealthPool;
        private EcsPool<LevelLoadedEvent> _levelLoadedPool;

        public LoadLevelSystem(LevelConfigService levelConfigService, RunEntityIndex runEntityIndex)
        {
            _levelConfigService = levelConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartLevelRequest>().End();
            _requestPool = world.GetPool<StartLevelRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _enemyBaseHealthPool = world.GetPool<EnemyBaseHealthComponent>();
            _levelLoadedPool = world.GetPool<LevelLoadedEvent>();
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
                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelData = _levelConfigService.GetLevel(locationId, request.LevelIndex);
                if (levelData == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                _levelPool.GetOrAdd(runEntity).LevelIndex = request.LevelIndex;
                _levelTypePool.GetOrAdd(runEntity).Value = levelData.LevelType;
                _enemyBaseHealthPool.GetOrAdd(runEntity).Value = levelData.EnemyBaseHealth;

                ref var levelLoadedEvent = ref _levelLoadedPool.Add(world.NewEntity());
                levelLoadedEvent.LevelIndex = request.LevelIndex;
                levelLoadedEvent.LevelType = levelData.LevelType;

                world.DelEntity(requestEntity);
            }
        }
    }
}