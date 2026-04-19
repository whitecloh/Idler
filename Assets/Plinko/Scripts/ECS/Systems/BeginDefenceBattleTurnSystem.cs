using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BeginDefenceBattleTurnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<BeginBattleTurnRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<CurrentEnemyWaveComponent> _currentEnemyWavePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<GenerateHandRequest> _generateHandRequestPool;

        public BeginDefenceBattleTurnSystem(
            LevelConfigService levelConfigService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _levelConfigService = levelConfigService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BeginBattleTurnRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _requestPool = world.GetPool<BeginBattleTurnRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
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

                if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                    !_levelTypePool.Has(runEntity) ||
                    _levelTypePool.Get(runEntity).Value != Enums.LevelType.DefenceBattle)
                {
                    world.DelEntity(requestEntity);
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
                    !battleState.IsResolved)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ClearHandOnly(world, runEntity);
                _battleRuntimeService.ClearTransient();
                if (_currentEnemyWavePool.Has(runEntity))
                {
                    _currentEnemyWavePool.Del(runEntity);
                }

                EnsureHandState(runEntity);
                _handStatePool.Get(runEntity).CardCount = 0;

                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
                var state = _battleRuntimeService.CurrentBaseDefenseState ?? BaseDefenseBattleUtility.CreateState(levelData);
                if (state == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                _battleRuntimeService.CurrentBaseDefenseState = state;
                battleState.CurrentTurn = battleState.CurrentTurn > 0 ? battleState.CurrentTurn + 1 : 1;
                battleState.IsResolved = false;
                battleState.IsPlayerTurnActive = true;
                battleState.HasGeneratedHandThisTurn = false;
                battleState.NextDeploymentOrder = 0;

                state.CurrentManaCap = Mathf.Min(state.MaxMana, state.StartingMana + Mathf.Max(0, battleState.CurrentTurn - 1));
                state.PreviewWaveUnits = BaseDefenseBattleUtility.BuildPreviewWaveUnits(levelData, battleState.CurrentTurn);

                ref var currentMana = ref _manaPool.Get(runEntity);
                currentMana.Value = state.CurrentManaCap;
                _manaChangedEventPool.Add(world.NewEntity()).Value = currentMana.Value;

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

        private void ClearHandOnly(EcsWorld world, int runEntity)
        {
            var entitiesToDelete = new List<int>();
            foreach (var handCardEntity in _handCardFilter)
            {
                entitiesToDelete.Add(handCardEntity);
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
