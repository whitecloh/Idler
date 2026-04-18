using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshBattleResultUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly LocationConfigService _locationConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;

        public RefreshBattleResultUiSystem(
            BattleRuntimeService battleRuntimeService,
            LocationConfigService locationConfigService,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _battleRuntimeService = battleRuntimeService;
            _locationConfigService = locationConfigService;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.Result)
            {
                _uiCompositionRoot.RefreshBattleResult(new BattleResultViewData());
                return;
            }

            var result = _battleRuntimeService.CurrentResult ?? BuildFallbackResult(runEntity);
            _uiCompositionRoot.RefreshBattleResult(BuildViewData(runEntity, result));
        }

        private BattleResultViewData BuildViewData(int runEntity, BattleResultModel result)
        {
            var hasNextLevel = HasNextLevel(runEntity);
            var status = _statusPool.Has(runEntity) ? _statusPool.Get(runEntity).Value : Enums.RunStatus.None;
            var isDefeat = status == Enums.RunStatus.Defeat || (result != null && result.IsDefeat);
            var isVictory = !isDefeat && (result == null || result.IsVictory);
            var isRunCompleted = status == Enums.RunStatus.Victory || (isVictory && !hasNextLevel);

            var viewData = new BattleResultViewData
            {
                IsVictory = isVictory,
                IsDefeat = isDefeat,
                IsRunCompleted = isRunCompleted,
                PlayerBaseHealthAfter = result != null ? result.PlayerBaseHealthAfter : _playerBasePool.Get(runEntity).Value,
                EnemyBaseHealthAfter = result != null ? result.EnemyBaseHealthAfter : _enemyBasePool.Get(runEntity).Value,
                CanAdvance = isVictory && !isRunCompleted,
                CanReturnToMenu = isDefeat || isRunCompleted
            };

            if (isDefeat)
            {
                viewData.Title = "Defeat";
                viewData.Description = $"Your base was destroyed on turn {Mathf.Max(1, result != null ? result.TurnsSpent : 1)}.";
                viewData.PrimaryActionLabel = "Return to Menu";
                viewData.RewardText = "Reward: +0 gold";
                if (result != null)
                {
                    viewData.RewardBreakdownText =
                        $"Enemy kills {result.EnemyKillsTotal}, enemy base damage {result.DamageToEnemyBaseTotal}, base loss {result.DamageToPlayerBaseTotal}.";
                }

                return viewData;
            }

            viewData.Title = isRunCompleted ? "Location Complete" : "Victory";
            viewData.Description = isRunCompleted
                ? $"All levels in this location are cleared. Total turns: {Mathf.Max(1, result != null ? result.TurnsSpent : 1)}."
                : $"Battle level cleared in {Mathf.Max(1, result != null ? result.TurnsSpent : 1)} turn(s).";
            viewData.PrimaryActionLabel = isRunCompleted ? "Return to Menu" : "Next Level";
            viewData.RewardText = $"Reward: +{Mathf.Max(0, result != null ? result.RewardGranted : 0)} gold";

            if (result != null)
            {
                var turnsPenalty = result.TurnsSpent > 1 ? (result.TurnsSpent - 1) * 2 : 0;
                viewData.RewardBreakdownText =
                    $"Base {result.BaseReward} + kills {result.EnemyKillsTotal}x3 + enemy base {result.DamageToEnemyBaseTotal}/5 - base loss {result.DamageToPlayerBaseTotal}/4 - turns {turnsPenalty}.";
            }

            return viewData;
        }

        private BattleResultModel BuildFallbackResult(int runEntity)
        {
            var turnsSpent = _battleStatePool.Has(runEntity)
                ? Mathf.Max(1, _battleStatePool.Get(runEntity).CurrentTurn)
                : 1;
            var totalEnemyKills = _battleStatePool.Has(runEntity) ? _battleStatePool.Get(runEntity).TotalEnemyKills : 0;
            var totalDamageToEnemyBase = _battleStatePool.Has(runEntity) ? _battleStatePool.Get(runEntity).TotalDamageToEnemyBase : 0;
            var totalDamageToPlayerBase = _battleStatePool.Has(runEntity) ? _battleStatePool.Get(runEntity).TotalDamageToPlayerBase : 0;
            var playerBaseAfter = _playerBasePool.Get(runEntity).Value;
            var enemyBaseAfter = _enemyBasePool.Get(runEntity).Value;

            return new BattleResultModel
            {
                PlayerBaseHealthBefore = playerBaseAfter + totalDamageToPlayerBase,
                PlayerBaseHealthAfter = playerBaseAfter,
                EnemyBaseHealthBefore = enemyBaseAfter + totalDamageToEnemyBase,
                EnemyBaseHealthAfter = enemyBaseAfter,
                EnemyKillsThisTurn = 0,
                EnemyKillsTotal = totalEnemyKills,
                DamageToEnemyBaseThisTurn = 0,
                DamageToEnemyBaseTotal = totalDamageToEnemyBase,
                DamageToPlayerBaseThisTurn = 0,
                DamageToPlayerBaseTotal = totalDamageToPlayerBase,
                TurnsSpent = turnsSpent,
                BaseReward = 0,
                RewardGranted = 0,
                IsVictory = enemyBaseAfter <= 0,
                IsDefeat = playerBaseAfter <= 0
            };
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
    }
}
