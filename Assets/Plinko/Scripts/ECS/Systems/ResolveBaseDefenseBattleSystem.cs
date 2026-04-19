using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ResolveBaseDefenseBattleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _turnStartedFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<BattleResolvedEvent> _battleResolvedEventPool;
        private EcsPool<TurnCompletedEvent> _turnCompletedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<HandClearedEvent> _handClearedEventPool;
        private EcsPool<StartBattlePlaybackRequest> _startBattlePlaybackRequestPool;

        public ResolveBaseDefenseBattleSystem(BattleRuntimeService battleRuntimeService, RunEntityIndex runEntityIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _turnStartedFilter = world.Filter<BaseDefenseTurnStartedEvent>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _battleResolvedEventPool = world.GetPool<BattleResolvedEvent>();
            _turnCompletedEventPool = world.GetPool<TurnCompletedEvent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _handClearedEventPool = world.GetPool<HandClearedEvent>();
            _startBattlePlaybackRequestPool = world.GetPool<StartBattlePlaybackRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var eventEntity in _turnStartedFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                if (!_levelTypePool.Has(runEntity) ||
                    _levelTypePool.Get(runEntity).Value != Enums.LevelType.DefenceBattle ||
                    _phasePool.Get(runEntity).Value != Enums.PhaseType.Battle)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var state = _battleRuntimeService.CurrentBaseDefenseState;
                if (state == null)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var timeline = new BattleTimelineModel();
                var playerBaseBefore = _playerBasePool.Get(runEntity).Value;
                var enemyCountBefore = state.EnemyUnits.Count;
                var damageToPlayerBaseThisTurn = 0;
                var tickIndex = 0;

                SpawnPreviewWave(state, timeline, ref tickIndex);
                ExecutePlayerActions(state, timeline, ref tickIndex);
                ExecuteEnemyActions(state, timeline, ref tickIndex, ref damageToPlayerBaseThisTurn);
                CleanupDeadUnits(state);

                var playerBaseAfter = Mathf.Max(0, playerBaseBefore - damageToPlayerBaseThisTurn);
                _playerBasePool.Get(runEntity).Value = playerBaseAfter;

                state.CompletedTurnCount = Mathf.Min(state.RequiredTurnCount, state.CompletedTurnCount + 1);
                state.PreviewWaveUnits.Clear();

                ref var battleState = ref _battleStatePool.Get(runEntity);
                battleState.IsResolved = true;
                battleState.IsPlayerTurnActive = false;
                battleState.HasGeneratedHandThisTurn = false;
                battleState.TotalEnemyKills += Mathf.Max(0, enemyCountBefore - state.EnemyUnits.Count);
                battleState.TotalDamageToPlayerBase += damageToPlayerBaseThisTurn;

                var result = new BattleResultModel
                {
                    PlayerBaseHealthBefore = playerBaseBefore,
                    PlayerBaseHealthAfter = playerBaseAfter,
                    EnemyBaseHealthBefore = 0,
                    EnemyBaseHealthAfter = 0,
                    EnemyKillsThisTurn = Mathf.Max(0, enemyCountBefore - state.EnemyUnits.Count),
                    EnemyKillsTotal = battleState.TotalEnemyKills,
                    DamageToEnemyBaseThisTurn = 0,
                    DamageToEnemyBaseTotal = 0,
                    DamageToPlayerBaseThisTurn = damageToPlayerBaseThisTurn,
                    DamageToPlayerBaseTotal = battleState.TotalDamageToPlayerBase,
                    TurnsSpent = Mathf.Max(1, battleState.CurrentTurn),
                    BaseReward = 0,
                    RewardGranted = 0,
                    IsVictory = playerBaseAfter > 0 && state.CompletedTurnCount >= state.RequiredTurnCount,
                    IsDefeat = playerBaseAfter <= 0
                };

                _battleRuntimeService.CurrentTimeline = timeline;
                _battleRuntimeService.CurrentResult = result;

                _battleResolvedEventPool.Add(world.NewEntity());
                _turnCompletedEventPool.Add(world.NewEntity());
                ClearHandCards(world, runEntity);
                _handClearedEventPool.Add(world.NewEntity());
                _phasePool.Get(runEntity).Value = Enums.PhaseType.BattlePlayback;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.BattlePlayback;
                _startBattlePlaybackRequestPool.Add(world.NewEntity());
                world.DelEntity(eventEntity);
            }
        }

        private void ClearHandCards(EcsWorld world, int runEntity)
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

        private static void SpawnPreviewWave(BaseDefenseBattleStateModel state, BattleTimelineModel timeline, ref int tickIndex)
        {
            if (state.PreviewWaveUnits == null || state.PreviewWaveUnits.Count == 0)
            {
                return;
            }

            var tick = new BattleTickModel { TickIndex = tickIndex++ };
            foreach (var previewUnit in state.PreviewWaveUnits)
            {
                var spawnedUnit = new BaseDefenseUnitStateModel
                {
                    RuntimeId = state.NextRuntimeId++,
                    SourceOwnedUnitRuntimeId = 0,
                    SpawnId = previewUnit.SpawnId,
                    DisplayName = previewUnit.DisplayName,
                    Attack = previewUnit.Attack,
                    Health = previewUnit.Health,
                    ManaCost = 0,
                    MoveRange = Mathf.Max(1, previewUnit.MoveRange),
                    AttackRange = Mathf.Max(0, previewUnit.AttackRange),
                    CanAttackOtherLines = previewUnit.CanAttackOtherLines,
                    CanMoveBetweenLines = previewUnit.CanMoveBetweenLines,
                    LaneIndex = Mathf.Clamp(previewUnit.LaneIndex, 0, Mathf.Max(0, state.LaneCount - 1)),
                    CellIndex = BaseDefenseBattleUtility.GetAbsoluteCellIndex(state, previewUnit.EnemySideCellIndex),
                    IsEnemy = true,
                    PortraitSprite = previewUnit.PortraitSprite,
                    BattleAnimations = previewUnit.BattleAnimations
                };
                state.EnemyUnits.Add(spawnedUnit);
                tick.Actions.Add(new BattleActionModel
                {
                    Tick = tick.TickIndex,
                    ActionType = "spawn",
                    SourceRuntimeId = spawnedUnit.RuntimeId,
                    TargetRuntimeId = spawnedUnit.RuntimeId,
                    TargetPosition = new Vector2Int(spawnedUnit.CellIndex, spawnedUnit.LaneIndex)
                });
            }

            if (tick.Actions.Count > 0)
            {
                timeline.Ticks.Add(tick);
            }
        }

        private static void ExecutePlayerActions(BaseDefenseBattleStateModel state, BattleTimelineModel timeline, ref int tickIndex)
        {
            var tick = new BattleTickModel { TickIndex = tickIndex++ };
            foreach (var playerUnit in state.PlayerUnits)
            {
                if (playerUnit == null || playerUnit.Health <= 0)
                {
                    continue;
                }

                var target = FindClosestPlayerTarget(playerUnit, state.EnemyUnits);
                if (target == null || !CanAttack(playerUnit, target))
                {
                    continue;
                }

                ApplyAttack(tick, playerUnit, target);
            }

            if (tick.Actions.Count > 0)
            {
                timeline.Ticks.Add(tick);
            }
        }

        private static void ExecuteEnemyActions(
            BaseDefenseBattleStateModel state,
            BattleTimelineModel timeline,
            ref int tickIndex,
            ref int damageToPlayerBaseThisTurn)
        {
            var tick = new BattleTickModel { TickIndex = tickIndex++ };
            foreach (var enemyUnit in state.EnemyUnits)
            {
                if (enemyUnit == null || enemyUnit.Health <= 0)
                {
                    continue;
                }

                var immediateTarget = FindImmediateEnemyTarget(enemyUnit, state.PlayerUnits);
                if (immediateTarget != null && CanAttack(enemyUnit, immediateTarget))
                {
                    ApplyAttack(tick, enemyUnit, immediateTarget);
                    continue;
                }

                var movementTarget = FindMovementTarget(enemyUnit, state.PlayerUnits);
                if (movementTarget != null)
                {
                    var originalLane = enemyUnit.LaneIndex;
                    var originalCell = enemyUnit.CellIndex;
                    MoveEnemyTowardsTarget(enemyUnit, movementTarget);
                    if (enemyUnit.LaneIndex != originalLane || enemyUnit.CellIndex != originalCell)
                    {
                        tick.Actions.Add(new BattleActionModel
                        {
                            Tick = tick.TickIndex,
                            ActionType = "move",
                            SourceRuntimeId = enemyUnit.RuntimeId,
                            TargetRuntimeId = movementTarget.RuntimeId,
                            TargetPosition = new Vector2Int(enemyUnit.CellIndex, enemyUnit.LaneIndex)
                        });
                    }

                    continue;
                }

                if (enemyUnit.CellIndex == 0)
                {
                    damageToPlayerBaseThisTurn += enemyUnit.Attack;
                    tick.Actions.Add(new BattleActionModel
                    {
                        Tick = tick.TickIndex,
                        ActionType = "base_damage_player",
                        SourceRuntimeId = enemyUnit.RuntimeId,
                        TargetRuntimeId = 0,
                        Value = enemyUnit.Attack,
                        TargetPosition = new Vector2Int(-1, enemyUnit.LaneIndex)
                    });
                    continue;
                }

                var nextCell = Mathf.Max(0, enemyUnit.CellIndex - 1);
                if (nextCell != enemyUnit.CellIndex)
                {
                    enemyUnit.CellIndex = nextCell;
                    tick.Actions.Add(new BattleActionModel
                    {
                        Tick = tick.TickIndex,
                        ActionType = "move",
                        SourceRuntimeId = enemyUnit.RuntimeId,
                        TargetRuntimeId = 0,
                        TargetPosition = new Vector2Int(enemyUnit.CellIndex, enemyUnit.LaneIndex)
                    });
                }
            }

            if (tick.Actions.Count > 0)
            {
                timeline.Ticks.Add(tick);
            }
        }

        private static void ApplyAttack(BattleTickModel tick, BaseDefenseUnitStateModel attacker, BaseDefenseUnitStateModel target)
        {
            target.Health = Mathf.Max(0, target.Health - attacker.Attack);
            tick.Actions.Add(new BattleActionModel
            {
                Tick = tick.TickIndex,
                ActionType = "attack",
                SourceRuntimeId = attacker.RuntimeId,
                TargetRuntimeId = target.RuntimeId,
                Value = attacker.Attack,
                TargetPosition = new Vector2Int(target.CellIndex, target.LaneIndex)
            });

            if (target.Health > 0)
            {
                return;
            }

            tick.Actions.Add(new BattleActionModel
            {
                Tick = tick.TickIndex,
                ActionType = "defeat",
                SourceRuntimeId = attacker.RuntimeId,
                TargetRuntimeId = target.RuntimeId,
                TargetPosition = new Vector2Int(target.CellIndex, target.LaneIndex)
            });
        }

        private static BaseDefenseUnitStateModel FindClosestPlayerTarget(BaseDefenseUnitStateModel playerUnit, List<BaseDefenseUnitStateModel> enemies)
        {
            BaseDefenseUnitStateModel best = null;
            var bestDistance = int.MaxValue;
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.Health <= 0)
                {
                    continue;
                }

                if (!CanTarget(playerUnit, enemy))
                {
                    continue;
                }

                var distance = GetDistance(playerUnit, enemy);
                if (distance < bestDistance || distance == bestDistance && (best == null || enemy.RuntimeId < best.RuntimeId))
                {
                    best = enemy;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static BaseDefenseUnitStateModel FindImmediateEnemyTarget(BaseDefenseUnitStateModel enemyUnit, List<BaseDefenseUnitStateModel> players)
        {
            BaseDefenseUnitStateModel best = null;
            var bestDistance = int.MaxValue;
            foreach (var player in players)
            {
                if (player == null || player.Health <= 0)
                {
                    continue;
                }

                if (!CanTarget(enemyUnit, player) || !CanAttack(enemyUnit, player))
                {
                    continue;
                }

                var distance = GetDistance(enemyUnit, player);
                if (distance < bestDistance || distance == bestDistance && (best == null || player.RuntimeId < best.RuntimeId))
                {
                    best = player;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static BaseDefenseUnitStateModel FindMovementTarget(BaseDefenseUnitStateModel enemyUnit, List<BaseDefenseUnitStateModel> players)
        {
            BaseDefenseUnitStateModel best = null;
            var bestDistance = int.MaxValue;
            foreach (var player in players)
            {
                if (player == null || player.Health <= 0)
                {
                    continue;
                }

                var canConsider = enemyUnit.CanMoveBetweenLines || player.LaneIndex == enemyUnit.LaneIndex;
                if (!canConsider)
                {
                    continue;
                }

                var distance = GetDistance(enemyUnit, player);
                if (distance < bestDistance || distance == bestDistance && (best == null || player.RuntimeId < best.RuntimeId))
                {
                    best = player;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static bool CanTarget(BaseDefenseUnitStateModel source, BaseDefenseUnitStateModel target)
        {
            return source.CanAttackOtherLines || source.LaneIndex == target.LaneIndex;
        }

        private static bool CanAttack(BaseDefenseUnitStateModel source, BaseDefenseUnitStateModel target)
        {
            return GetDistance(source, target) <= Mathf.Max(0, source.AttackRange);
        }

        private static int GetDistance(BaseDefenseUnitStateModel from, BaseDefenseUnitStateModel to)
        {
            return Mathf.Abs(from.CellIndex - to.CellIndex) + Mathf.Abs(from.LaneIndex - to.LaneIndex);
        }

        private static void MoveEnemyTowardsTarget(BaseDefenseUnitStateModel enemyUnit, BaseDefenseUnitStateModel target)
        {
            if (enemyUnit.CellIndex != target.CellIndex)
            {
                enemyUnit.CellIndex += target.CellIndex > enemyUnit.CellIndex ? 1 : -1;
                return;
            }

            if (enemyUnit.CanMoveBetweenLines && enemyUnit.LaneIndex != target.LaneIndex)
            {
                enemyUnit.LaneIndex += target.LaneIndex > enemyUnit.LaneIndex ? 1 : -1;
            }
        }

        private static void CleanupDeadUnits(BaseDefenseBattleStateModel state)
        {
            state.PlayerUnits.RemoveAll(unit => unit == null || unit.Health <= 0);
            state.EnemyUnits.RemoveAll(unit => unit == null || unit.Health <= 0);
        }
    }
}
