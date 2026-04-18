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
    public sealed class ResolveBattleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private const int MaxBattleTicks = 64;
        private const int PlayerFrontX = -2;

        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _waveSelectedFilter;
        private EcsFilter _deployedFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<DeploymentOrderComponent> _deploymentOrderPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<BattleResolvedEvent> _battleResolvedEventPool;
        private EcsPool<TurnCompletedEvent> _turnCompletedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<HandClearedEvent> _handClearedEventPool;
        private EcsPool<StartBattlePlaybackRequest> _startBattlePlaybackRequestPool;

        public ResolveBattleSystem(
            BattleRuntimeService battleRuntimeService,
            GameSettingsService gameSettingsService,
            OwnedUnitIndex ownedUnitIndex,
            RunEntityIndex runEntityIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _gameSettingsService = gameSettingsService;
            _ownedUnitIndex = ownedUnitIndex;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _waveSelectedFilter = world.Filter<EnemyWaveSelectedEvent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().Inc<HandCardOwnerUnitComponent>().Inc<DeploymentOrderComponent>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _deploymentOrderPool = world.GetPool<DeploymentOrderComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
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
            foreach (var eventEntity in _waveSelectedFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.BattlePreparation)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var enemyWave = _battleRuntimeService.CurrentEnemyWave;
                if (enemyWave == null)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                _phasePool.Get(runEntity).Value = Enums.PhaseType.Battle;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Battle;

                var playerCombatants = BuildPlayerCombatants(world);
                var enemyCombatants = BuildEnemyCombatants(enemyWave);
                var timeline = ResolveTimeline(playerCombatants, enemyCombatants);
                var result = ApplyBattleOutcome(runEntity, timeline, playerCombatants, enemyCombatants);

                _battleRuntimeService.CurrentTimeline = timeline;
                _battleRuntimeService.CurrentResult = result;

                if (_battleStatePool.Has(runEntity))
                {
                    ref var battleState = ref _battleStatePool.Get(runEntity);
                    battleState.IsResolved = true;
                    battleState.NextDeploymentOrder = 0;
                    battleState.IsPlayerTurnActive = false;
                    battleState.HasGeneratedHandThisTurn = false;
                }

                ClearTurnEntities(world, runEntity);

                _battleResolvedEventPool.Add(world.NewEntity());
                _turnCompletedEventPool.Add(world.NewEntity());
                _phasePool.Get(runEntity).Value = Enums.PhaseType.BattlePlayback;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.BattlePlayback;
                _startBattlePlaybackRequestPool.Add(world.NewEntity());

                world.DelEntity(eventEntity);
            }
        }

        private List<CombatantState> BuildPlayerCombatants(EcsWorld world)
        {
            var combatants = new List<CombatantState>();
            var deployedUnits = new List<DeployedUnitState>();

            foreach (var deployedEntity in _deployedFilter)
            {
                var ownerRuntimeId = _handCardOwnerPool.Get(deployedEntity).OwnedUnitRuntimeId;
                if (!_ownedUnitIndex.TryGet(ownerRuntimeId, out var ownedUnitEntity))
                {
                    continue;
                }

                deployedUnits.Add(new DeployedUnitState
                {
                    DeploymentOrder = _deploymentOrderPool.Get(deployedEntity).Value,
                    RuntimeId = deployedEntity,
                    DisplayName = _displayNamePool.Get(ownedUnitEntity).Value,
                    Attack = Mathf.Max(0, _unitStatsPool.Get(ownedUnitEntity).Attack),
                    Health = Mathf.Max(0, _unitStatsPool.Get(ownedUnitEntity).Health),
                    MoveRange = 1,
                    AttackRange = 1
                });
            }

            deployedUnits.Sort((left, right) =>
            {
                var orderCompare = right.DeploymentOrder.CompareTo(left.DeploymentOrder);
                return orderCompare != 0 ? orderCompare : left.RuntimeId.CompareTo(right.RuntimeId);
            });

            for (var index = 0; index < deployedUnits.Count; index++)
            {
                var deployedUnit = deployedUnits[index];
                combatants.Add(new CombatantState
                {
                    RuntimeId = deployedUnit.RuntimeId,
                    DisplayName = deployedUnit.DisplayName,
                    Attack = deployedUnit.Attack,
                    Health = deployedUnit.Health,
                    MoveRange = deployedUnit.MoveRange,
                    AttackRange = deployedUnit.AttackRange,
                    Position = new Vector2Int(PlayerFrontX + index, 0)
                });
            }

            return combatants;
        }

        private static List<CombatantState> BuildEnemyCombatants(EnemyWaveModel enemyWave)
        {
            var combatants = new List<CombatantState>();
            if (enemyWave == null || enemyWave.Enemies == null)
            {
                return combatants;
            }

            for (var index = 0; index < enemyWave.Enemies.Count; index++)
            {
                var enemy = enemyWave.Enemies[index];
                if (enemy == null)
                {
                    continue;
                }

                combatants.Add(new CombatantState
                {
                    RuntimeId = -(index + 1),
                    DisplayName = enemy.DisplayName,
                    Attack = Mathf.Max(0, enemy.Attack),
                    Health = Mathf.Max(0, enemy.Health),
                    MoveRange = Mathf.Max(1, enemy.MoveRange),
                    AttackRange = Mathf.Max(1, enemy.AttackRange),
                    Position = new Vector2Int(enemy.BoardX, 0)
                });
            }

            return combatants;
        }

        private static BattleTimelineModel ResolveTimeline(List<CombatantState> players, List<CombatantState> enemies)
        {
            var timeline = new BattleTimelineModel();
            var tickIndex = 0;

            while (HasAlive(players) && HasAlive(enemies) && tickIndex < MaxBattleTicks)
            {
                var tick = new BattleTickModel { TickIndex = tickIndex };
                ExecuteSide(players, enemies, tick);
                ExecuteSide(enemies, players, tick);
                if (tick.Actions.Count == 0)
                {
                    break;
                }

                timeline.Ticks.Add(tick);
                tickIndex++;
            }

            return timeline;
        }

        private BattleResultModel ApplyBattleOutcome(
            int runEntity,
            BattleTimelineModel timeline,
            List<CombatantState> players,
            List<CombatantState> enemies)
        {
            var playerBaseBefore = _playerBasePool.Get(runEntity).Value;
            var enemyBaseBefore = _enemyBasePool.Get(runEntity).Value;
            var playerSurvivors = GetAliveCombatants(players);
            var enemySurvivors = GetAliveCombatants(enemies);
            var enemyKillsThisTurn = Mathf.Max(0, enemies.Count - enemySurvivors.Count);

            var damageToEnemyBase = enemySurvivors.Count == 0 ? SumAttack(playerSurvivors) : 0;
            var damageToPlayerBase = playerSurvivors.Count == 0 ? SumAttack(enemySurvivors) : 0;

            timeline.SurvivorDamageToEnemyBase = damageToEnemyBase;
            timeline.SurvivorDamageToPlayerBase = damageToPlayerBase;
            AppendBaseDamageTick(timeline, damageToEnemyBase, damageToPlayerBase);

            var playerBaseAfter = Mathf.Max(0, playerBaseBefore - damageToPlayerBase);
            var enemyBaseAfter = Mathf.Max(0, enemyBaseBefore - damageToEnemyBase);
            _playerBasePool.Get(runEntity).Value = playerBaseAfter;
            _enemyBasePool.Get(runEntity).Value = enemyBaseAfter;

            var enemyKillsTotal = enemyKillsThisTurn;
            var damageToEnemyBaseTotal = damageToEnemyBase;
            var damageToPlayerBaseTotal = damageToPlayerBase;
            var turnsSpent = 1;

            if (_battleStatePool.Has(runEntity))
            {
                ref var battleState = ref _battleStatePool.Get(runEntity);
                battleState.TotalEnemyKills += enemyKillsThisTurn;
                battleState.TotalDamageToEnemyBase += damageToEnemyBase;
                battleState.TotalDamageToPlayerBase += damageToPlayerBase;
                enemyKillsTotal = battleState.TotalEnemyKills;
                damageToEnemyBaseTotal = battleState.TotalDamageToEnemyBase;
                damageToPlayerBaseTotal = battleState.TotalDamageToPlayerBase;
                turnsSpent = Mathf.Max(1, battleState.CurrentTurn);
            }

            return new BattleResultModel
            {
                PlayerBaseHealthBefore = playerBaseBefore,
                PlayerBaseHealthAfter = playerBaseAfter,
                EnemyBaseHealthBefore = enemyBaseBefore,
                EnemyBaseHealthAfter = enemyBaseAfter,
                EnemyKillsThisTurn = enemyKillsThisTurn,
                EnemyKillsTotal = enemyKillsTotal,
                DamageToEnemyBaseThisTurn = damageToEnemyBase,
                DamageToEnemyBaseTotal = damageToEnemyBaseTotal,
                DamageToPlayerBaseThisTurn = damageToPlayerBase,
                DamageToPlayerBaseTotal = damageToPlayerBaseTotal,
                TurnsSpent = turnsSpent,
                BaseReward = 0,
                RewardGranted = 0,
                IsVictory = enemyBaseAfter <= 0,
                IsDefeat = playerBaseAfter <= 0
            };
        }

        private static void ExecuteSide(List<CombatantState> actors, List<CombatantState> targets, BattleTickModel tick)
        {
            for (var index = 0; index < actors.Count; index++)
            {
                var actor = actors[index];
                if (!actor.IsAlive)
                {
                    continue;
                }

                var target = FindNearestTarget(actor, targets);
                if (target == null)
                {
                    return;
                }

                var originalPosition = actor.Position;
                MoveTowardsTarget(actor, target);
                if (actor.Position != originalPosition)
                {
                    tick.Actions.Add(new BattleActionModel
                    {
                        Tick = tick.TickIndex,
                        ActionType = "move",
                        SourceRuntimeId = actor.RuntimeId,
                        TargetRuntimeId = target.RuntimeId,
                        TargetPosition = actor.Position
                    });
                }

                if (!target.IsAlive || GetDistance(actor.Position, target.Position) > actor.AttackRange)
                {
                    continue;
                }

                target.Health = Mathf.Max(0, target.Health - actor.Attack);
                tick.Actions.Add(new BattleActionModel
                {
                    Tick = tick.TickIndex,
                    ActionType = "attack",
                    SourceRuntimeId = actor.RuntimeId,
                    TargetRuntimeId = target.RuntimeId,
                    Value = actor.Attack,
                    TargetPosition = target.Position
                });

                if (!target.IsAlive)
                {
                    tick.Actions.Add(new BattleActionModel
                    {
                        Tick = tick.TickIndex,
                        ActionType = "defeat",
                        SourceRuntimeId = actor.RuntimeId,
                        TargetRuntimeId = target.RuntimeId,
                        TargetPosition = target.Position
                    });
                }
            }
        }

        private static CombatantState FindNearestTarget(CombatantState actor, List<CombatantState> candidates)
        {
            CombatantState best = null;
            var bestDistance = int.MaxValue;
            foreach (var candidate in candidates)
            {
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                var distance = GetDistance(actor.Position, candidate.Position);
                if (distance < bestDistance || distance == bestDistance && (best == null || candidate.RuntimeId < best.RuntimeId))
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static void MoveTowardsTarget(CombatantState actor, CombatantState target)
        {
            var remainingSteps = Mathf.Max(0, actor.MoveRange);
            while (remainingSteps > 0 && GetDistance(actor.Position, target.Position) > actor.AttackRange)
            {
                if (target.Position.x != actor.Position.x)
                {
                    actor.Position += new Vector2Int(target.Position.x > actor.Position.x ? 1 : -1, 0);
                }
                else
                {
                    break;
                }

                remainingSteps--;
            }
        }

        private void ClearTurnEntities(EcsWorld world, int runEntity)
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

            _handClearedEventPool.Add(world.NewEntity());
        }

        private static void AppendBaseDamageTick(BattleTimelineModel timeline, int damageToEnemyBase, int damageToPlayerBase)
        {
            if (damageToEnemyBase <= 0 && damageToPlayerBase <= 0)
            {
                return;
            }

            var tick = new BattleTickModel
            {
                TickIndex = timeline.Ticks.Count
            };

            if (damageToEnemyBase > 0)
            {
                tick.Actions.Add(new BattleActionModel
                {
                    Tick = tick.TickIndex,
                    ActionType = "base_damage_enemy",
                    Value = damageToEnemyBase,
                    TargetRuntimeId = 0,
                    TargetPosition = new Vector2Int(2, 0)
                });
            }

            if (damageToPlayerBase > 0)
            {
                tick.Actions.Add(new BattleActionModel
                {
                    Tick = tick.TickIndex,
                    ActionType = "base_damage_player",
                    Value = damageToPlayerBase,
                    TargetRuntimeId = 0,
                    TargetPosition = new Vector2Int(PlayerFrontX - 1, 0)
                });
            }

            timeline.Ticks.Add(tick);
        }

        private static List<CombatantState> GetAliveCombatants(List<CombatantState> combatants)
        {
            var alive = new List<CombatantState>();
            foreach (var combatant in combatants)
            {
                if (combatant != null && combatant.IsAlive)
                {
                    alive.Add(combatant);
                }
            }

            return alive;
        }

        private static bool HasAlive(List<CombatantState> combatants)
        {
            foreach (var combatant in combatants)
            {
                if (combatant != null && combatant.IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        private static int SumAttack(List<CombatantState> combatants)
        {
            var total = 0;
            foreach (var combatant in combatants)
            {
                total += combatant.Attack;
            }

            return total;
        }

        private static int GetDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x);
        }

        private sealed class DeployedUnitState
        {
            public int DeploymentOrder;
            public int RuntimeId;
            public string DisplayName;
            public int Attack;
            public int Health;
            public int MoveRange;
            public int AttackRange;
        }

        private sealed class CombatantState
        {
            public int RuntimeId;
            public string DisplayName;
            public int Attack;
            public int Health;
            public int MoveRange;
            public int AttackRange;
            public Vector2Int Position;

            public bool IsAlive => Health > 0;
        }
    }
}
