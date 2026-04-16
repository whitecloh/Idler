using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class SelectEnemyWaveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EnemyWaveSelectionService _enemyWaveSelectionService;
        private readonly LevelConfigService _levelConfigService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartBattleRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<CurrentEnemyWaveComponent> _currentEnemyWavePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<EnemyWaveSelectedEvent> _enemyWaveSelectedEventPool;

        public SelectEnemyWaveSystem(
            EnemyWaveSelectionService enemyWaveSelectionService,
            LevelConfigService levelConfigService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _enemyWaveSelectionService = enemyWaveSelectionService;
            _levelConfigService = levelConfigService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartBattleRequest>().End();
            _requestPool = world.GetPool<StartBattleRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _currentEnemyWavePool = world.GetPool<CurrentEnemyWaveComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _enemyWaveSelectedEventPool = world.GetPool<EnemyWaveSelectedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var currentPhase = _phasePool.Get(runEntity).Value;
                if (currentPhase != Enums.PhaseType.BattlePreparation)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (!_battleStatePool.Has(runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var battleState = ref _battleStatePool.Get(runEntity);
                if (!battleState.IsPlayerTurnActive || !battleState.HasGeneratedHandThisTurn)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
                var selectedWave = _enemyWaveSelectionService.SelectWave(levelData, _enemyBasePool.Get(runEntity).Value);
                _battleRuntimeService.CurrentEnemyWave = selectedWave;

                if (!_currentEnemyWavePool.Has(runEntity))
                {
                    _currentEnemyWavePool.Add(runEntity);
                }

                _currentEnemyWavePool.Get(runEntity) = new CurrentEnemyWaveComponent
                {
                    ThresholdPercent = selectedWave != null ? selectedWave.ThresholdPercent : 0,
                    EnemyCount = selectedWave != null && selectedWave.Enemies != null ? selectedWave.Enemies.Count : 0,
                    TotalAttack = selectedWave != null ? selectedWave.TotalAttack : 0,
                    TotalHealth = selectedWave != null ? selectedWave.TotalHealth : 0
                };

                battleState.IsResolved = false;
                battleState.IsPlayerTurnActive = false;

                if (currentPhase != Enums.PhaseType.BattlePreparation)
                {
                    _phasePool.Get(runEntity).Value = Enums.PhaseType.BattlePreparation;
                    _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.BattlePreparation;
                }

                _enemyWaveSelectedEventPool.Add(world.NewEntity()).ThresholdPercent = selectedWave != null ? selectedWave.ThresholdPercent : 0;
                world.DelEntity(requestEntity);
            }
        }
    }
}
