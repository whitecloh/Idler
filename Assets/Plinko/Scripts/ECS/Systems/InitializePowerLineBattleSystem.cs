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
    public sealed class InitializePowerLineBattleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<InitializePowerLineBattleRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<DrawPowerLineHandCardsRequest> _drawHandRequestPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;

        public InitializePowerLineBattleSystem(
            GameSettingsService gameSettingsService,
            LevelConfigService levelConfigService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _levelConfigService = levelConfigService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<InitializePowerLineBattleRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _requestPool = world.GetPool<InitializePowerLineBattleRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _drawHandRequestPool = world.GetPool<DrawPowerLineHandCardsRequest>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                _requestPool.Get(requestEntity);
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                    !_levelTypePool.Has(runEntity) ||
                    _levelTypePool.Get(runEntity).Value != Enums.LevelType.PowerLineBattle ||
                    !_phasePool.Has(runEntity) ||
                    _phasePool.Get(runEntity).Value != Enums.PhaseType.Battle)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
                var state = PowerLineBattleUtility.CreateState(levelData, _gameSettingsService);
                if (state == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                _battleRuntimeService.CurrentTimeline = null;
                _battleRuntimeService.CurrentEnemyWave = null;
                _battleRuntimeService.CurrentBaseDefenseState = null;
                _battleRuntimeService.CurrentPowerLineState = state;
                _battleRuntimeService.CurrentResult = null;

                ClearHand(world);
                if (!_handStatePool.Has(runEntity))
                {
                    _handStatePool.Add(runEntity);
                }

                _handStatePool.Get(runEntity) = new HandStateComponent
                {
                    CardCount = 0,
                    NextRuntimeId = 1
                };

                if (!_battleStatePool.Has(runEntity))
                {
                    _battleStatePool.Add(runEntity);
                }

                _battleStatePool.Get(runEntity) = new BattleStateComponent
                {
                    CurrentTurn = 0,
                    IsResolved = false,
                    NextDeploymentOrder = 0,
                    IsPlayerTurnActive = true,
                    HasGeneratedHandThisTurn = true,
                    TotalEnemyKills = 0,
                    TotalDamageToEnemyBase = 0,
                    TotalDamageToPlayerBase = 0
                };

                _manaPool.Get(runEntity).Value = state.CurrentMana;
                _manaChangedEventPool.Add(world.NewEntity()).Value = state.CurrentMana;

                ref var drawRequest = ref _drawHandRequestPool.Add(world.NewEntity());
                drawRequest.Count = _gameSettingsService.GetHandSize();
                drawRequest.ClearExisting = true;
                _saveRunRequestPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private void ClearHand(EcsWorld world)
        {
            var toDelete = new List<int>();
            foreach (var entity in _handCardFilter)
            {
                toDelete.Add(entity);
            }

            for (var index = 0; index < toDelete.Count; index++)
            {
                world.DelEntity(toDelete[index]);
            }
        }
    }
}
