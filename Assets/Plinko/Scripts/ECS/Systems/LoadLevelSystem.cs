using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using System.Collections.Generic;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class LoadLevelSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsFilter _deployedFilter;
        private EcsPool<StartLevelRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<CurrentEnemyWaveComponent> _currentEnemyWavePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<LevelLoadedEvent> _levelLoadedEventPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;

        public LoadLevelSystem(
            LevelConfigService levelConfigService,
            GameSettingsService gameSettingsService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _levelConfigService = levelConfigService;
            _gameSettingsService = gameSettingsService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartLevelRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().End();
            _requestPool = world.GetPool<StartLevelRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _currentEnemyWavePool = world.GetPool<CurrentEnemyWaveComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _levelLoadedEventPool = world.GetPool<LevelLoadedEvent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
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

                _levelPool.Get(runEntity).LevelIndex = request.LevelIndex;
                _levelTypePool.Get(runEntity).Value = levelData.LevelType;
                _enemyBasePool.Get(runEntity) = new EnemyBaseHealthComponent
                {
                    Value = levelData.EnemyBaseMaxHealth,
                    MaxValue = levelData.EnemyBaseMaxHealth
                };
                _manaPool.Get(runEntity).Value = _gameSettingsService.GetManaPerTurn();
                _battleRuntimeService.Clear();
                if (_handStatePool.Has(runEntity))
                {
                    _handStatePool.Get(runEntity).CardCount = 0;
                }

                if (_currentEnemyWavePool.Has(runEntity))
                {
                    _currentEnemyWavePool.Del(runEntity);
                }

                if (_battleStatePool.Has(runEntity))
                {
                    _battleStatePool.Get(runEntity) = new BattleStateComponent
                    {
                        CurrentTurn = 0,
                        IsResolved = false,
                        NextDeploymentOrder = 0,
                        IsPlayerTurnActive = false,
                        HasGeneratedHandThisTurn = false
                    };
                }

                ClearHandAndDeployment(world);

                ref var levelLoadedEvent = ref _levelLoadedEventPool.Add(world.NewEntity());
                levelLoadedEvent.LevelIndex = request.LevelIndex;
                levelLoadedEvent.LevelType = levelData.LevelType;
                _manaChangedEventPool.Add(world.NewEntity()).Value = _manaPool.Get(runEntity).Value;
                world.DelEntity(requestEntity);
            }
        }

        private void ClearHandAndDeployment(EcsWorld world)
        {
            var entitiesToDelete = new List<int>();
            foreach (var handCardEntity in _handCardFilter)
            {
                entitiesToDelete.Add(handCardEntity);
            }

            foreach (var deployedEntity in _deployedFilter)
            {
                if (!entitiesToDelete.Contains(deployedEntity))
                {
                    entitiesToDelete.Add(deployedEntity);
                }
            }

            foreach (var entity in entitiesToDelete)
            {
                world.DelEntity(entity);
            }
        }
    }
}
