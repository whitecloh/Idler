using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BeginStandardBattleTurnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsFilter _deployedFilter;
        private EcsPool<BeginBattleTurnRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<CurrentEnemyWaveComponent> _currentEnemyWavePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<GenerateHandRequest> _generateHandRequestPool;

        public BeginStandardBattleTurnSystem(
            GameSettingsService gameSettingsService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BeginBattleTurnRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().End();
            _requestPool = world.GetPool<BeginBattleTurnRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _currentEnemyWavePool = world.GetPool<CurrentEnemyWaveComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _generateHandRequestPool = world.GetPool<GenerateHandRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                _requestPool.Get(requestEntity);

                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (!_levelTypePool.Has(runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (_levelTypePool.Get(runEntity).Value != Enums.LevelType.StandardBattle)
                {
                    continue;
                }

                if (!_battleStatePool.Has(runEntity))
                {
                    _battleStatePool.Add(runEntity);
                }

                ref var battleState = ref _battleStatePool.Get(runEntity);
                if (_phasePool.Has(runEntity) &&
                    _phasePool.Get(runEntity).Value == Enums.PhaseType.BattlePreparation &&
                    battleState.IsPlayerTurnActive &&
                    !battleState.IsResolved &&
                    !_currentEnemyWavePool.Has(runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ClearHandAndDeployedEntities(world, runEntity);
                _battleRuntimeService.Clear();
                if (_currentEnemyWavePool.Has(runEntity))
                {
                    _currentEnemyWavePool.Del(runEntity);
                }

                ref var currentMana = ref _manaPool.Get(runEntity);
                currentMana.Value = _gameSettingsService.GetManaPerTurn();
                _manaChangedEventPool.Add(world.NewEntity()).Value = currentMana.Value;

                EnsureHandState(runEntity);
                _handStatePool.Get(runEntity).CardCount = 0;

                battleState.CurrentTurn = battleState.CurrentTurn > 0 ? battleState.CurrentTurn + 1 : 1;
                battleState.IsResolved = false;
                battleState.NextDeploymentOrder = 0;
                battleState.IsPlayerTurnActive = true;
                battleState.HasGeneratedHandThisTurn = false;

                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.BattlePreparation)
                {
                    _phasePool.Get(runEntity).Value = Enums.PhaseType.BattlePreparation;
                    _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.BattlePreparation;
                }

                _generateHandRequestPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private void EnsureHandState(int runEntity)
        {
            if (!_handStatePool.Has(runEntity))
            {
                _handStatePool.Add(runEntity) = new HandStateComponent
                {
                    CardCount = 0,
                    NextRuntimeId = 1
                };
            }
        }

        private void ClearHandAndDeployedEntities(EcsWorld world, int runEntity)
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

            if (_handStatePool.Has(runEntity))
            {
                _handStatePool.Get(runEntity).CardCount = 0;
            }
        }
    }
}
