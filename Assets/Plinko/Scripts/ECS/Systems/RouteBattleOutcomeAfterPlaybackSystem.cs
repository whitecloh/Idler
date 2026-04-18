using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RouteBattleOutcomeAfterPlaybackSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly LevelConfigService _levelConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _playbackCompletedFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<LevelCompletedEvent> _levelCompletedEventPool;
        private EcsPool<RunCompletedEvent> _runCompletedEventPool;
        private EcsPool<RunFailedEvent> _runFailedEventPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;
        private EcsPool<BeginBattleTurnRequest> _beginBattleTurnRequestPool;

        public RouteBattleOutcomeAfterPlaybackSystem(
            BattleRuntimeService battleRuntimeService,
            LevelConfigService levelConfigService,
            LocationConfigService locationConfigService,
            RunEntityIndex runEntityIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _levelConfigService = levelConfigService;
            _locationConfigService = locationConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _playbackCompletedFilter = world.Filter<BattlePlaybackCompletedEvent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _levelCompletedEventPool = world.GetPool<LevelCompletedEvent>();
            _runCompletedEventPool = world.GetPool<RunCompletedEvent>();
            _runFailedEventPool = world.GetPool<RunFailedEvent>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
            _beginBattleTurnRequestPool = world.GetPool<BeginBattleTurnRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var playbackCompletedEntity in _playbackCompletedFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(playbackCompletedEntity);
                    continue;
                }

                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.BattlePlayback)
                {
                    world.DelEntity(playbackCompletedEntity);
                    continue;
                }

                var result = _battleRuntimeService.CurrentResult;
                if (result == null)
                {
                    RouteBackToBattlePreparation(world, runEntity);
                    world.DelEntity(playbackCompletedEntity);
                    continue;
                }

                if (result.IsDefeat)
                {
                    result.BaseReward = 0;
                    result.RewardGranted = 0;
                    _statusPool.Get(runEntity).Value = Enums.RunStatus.Defeat;
                    SetPhase(world, runEntity, Enums.PhaseType.Result);
                    _runFailedEventPool.Add(world.NewEntity());
                    QueueSave(world);
                    world.DelEntity(playbackCompletedEntity);
                    continue;
                }

                if (result.IsVictory)
                {
                    ApplyVictoryReward(world, runEntity, result);
                    _levelCompletedEventPool.Add(world.NewEntity());

                    if (HasNextLevel(runEntity))
                    {
                        _statusPool.Get(runEntity).Value = Enums.RunStatus.InProgress;
                    }
                    else
                    {
                        _statusPool.Get(runEntity).Value = Enums.RunStatus.Victory;
                        _runCompletedEventPool.Add(world.NewEntity());
                    }

                    SetPhase(world, runEntity, Enums.PhaseType.Result);
                    QueueSave(world);
                    world.DelEntity(playbackCompletedEntity);
                    continue;
                }

                RouteBackToBattlePreparation(world, runEntity);
                world.DelEntity(playbackCompletedEntity);
            }
        }

        private void RouteBackToBattlePreparation(EcsWorld world, int runEntity)
        {
            _statusPool.Get(runEntity).Value = Enums.RunStatus.InProgress;
            _beginBattleTurnRequestPool.Add(world.NewEntity());
        }

        private void ApplyVictoryReward(EcsWorld world, int runEntity, BattleResultModel result)
        {
            var levelData = GetCurrentLevel(runEntity);
            if (result == null || levelData == null)
            {
                return;
            }

            var turnsPenalty = result.TurnsSpent > 1 ? (result.TurnsSpent - 1) * 2 : 0;
            var enemyKillBonus = result.EnemyKillsTotal * 3;
            var enemyBaseDamageBonus = result.DamageToEnemyBaseTotal / 5;
            var playerBaseDamagePenalty = result.DamageToPlayerBaseTotal / 4;
            var reward = UnityEngine.Mathf.Max(
                0,
                levelData.VictoryReward +
                enemyKillBonus +
                enemyBaseDamageBonus -
                playerBaseDamagePenalty -
                turnsPenalty);

            result.BaseReward = levelData.VictoryReward;
            result.RewardGranted = reward;

            if (reward <= 0)
            {
                return;
            }

            _goldPool.Get(runEntity).Value += reward;
            _goldChangedEventPool.Add(world.NewEntity()).Value = _goldPool.Get(runEntity).Value;
        }

        private Plinko.Scripts.Data.Levels.LevelData GetCurrentLevel(int runEntity)
        {
            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _levelPool.Get(runEntity).LevelIndex;
            return _levelConfigService.GetLevel(locationId, levelIndex);
        }

        private bool HasNextLevel(int runEntity)
        {
            var location = _locationConfigService.GetLocation(_locationPool.Get(runEntity).LocationId);
            if (location == null || location.Levels == null)
            {
                return false;
            }

            return _levelPool.Get(runEntity).LevelIndex + 1 < location.Levels.Count;
        }

        private void SetPhase(EcsWorld world, int runEntity, Enums.PhaseType phase)
        {
            if (_phasePool.Get(runEntity).Value == phase)
            {
                return;
            }

            _phasePool.Get(runEntity).Value = phase;
            _phaseChangedEventPool.Add(world.NewEntity()).Value = phase;
        }

        private void QueueSave(EcsWorld world)
        {
            _saveRunRequestPool.Add(world.NewEntity());
        }
    }
}
