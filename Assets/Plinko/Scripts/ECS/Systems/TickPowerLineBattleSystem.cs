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
    public sealed class TickPowerLineBattleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _finalizeRequestFilter;
        private EcsPool<FinalizePowerLineBattleResultRequest> _finalizeResultRequestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<BattleStateComponent> _battlePool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<LevelCompletedEvent> _levelCompletedEventPool;
        private EcsPool<RunCompletedEvent> _runCompletedEventPool;
        private EcsPool<RunFailedEvent> _runFailedEventPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;
        private EcsPool<PowerLineUnitSpawnedEvent> _powerLineUnitSpawnedEventPool;
        private EcsPool<PowerLineAttackEvent> _powerLineAttackEventPool;
        private EcsPool<PowerLineDamageEvent> _powerLineDamageEventPool;
        private EcsPool<PowerLineUnitDiedEvent> _powerLineUnitDiedEventPool;
        private EcsPool<PowerLinePlugStateChangedEvent> _powerLinePlugStateChangedEventPool;
        private EcsPool<PowerLineLaneConnectedEvent> _powerLineLaneConnectedEventPool;

        public TickPowerLineBattleSystem(
            GameSettingsService gameSettingsService,
            LevelConfigService levelConfigService,
            LocationConfigService locationConfigService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _levelConfigService = levelConfigService;
            _locationConfigService = locationConfigService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _finalizeRequestFilter = world.Filter<FinalizePowerLineBattleResultRequest>().End();
            _finalizeResultRequestPool = world.GetPool<FinalizePowerLineBattleResultRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _battlePool = world.GetPool<BattleStateComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _levelCompletedEventPool = world.GetPool<LevelCompletedEvent>();
            _runCompletedEventPool = world.GetPool<RunCompletedEvent>();
            _runFailedEventPool = world.GetPool<RunFailedEvent>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
            _powerLineUnitSpawnedEventPool = world.GetPool<PowerLineUnitSpawnedEvent>();
            _powerLineAttackEventPool = world.GetPool<PowerLineAttackEvent>();
            _powerLineDamageEventPool = world.GetPool<PowerLineDamageEvent>();
            _powerLineUnitDiedEventPool = world.GetPool<PowerLineUnitDiedEvent>();
            _powerLinePlugStateChangedEventPool = world.GetPool<PowerLinePlugStateChangedEvent>();
            _powerLineLaneConnectedEventPool = world.GetPool<PowerLineLaneConnectedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            ProcessFinalizeRequests(world);
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_levelTypePool.Has(runEntity) ||
                _levelTypePool.Get(runEntity).Value != Enums.LevelType.PowerLineBattle ||
                !_phasePool.Has(runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.Battle)
            {
                return;
            }

            var state = _battleRuntimeService.CurrentPowerLineState;
            if (state == null || state.IsPendingVictorySequence)
            {
                return;
            }

            var tickDuration = Mathf.Max(0.01f, _gameSettingsService.GetBattleTickDuration());
            state.TickAccumulator += Time.deltaTime * GetSimulationSpeedMultiplier(state);
            while (state.TickAccumulator >= tickDuration)
            {
                state.TickAccumulator -= tickDuration;
                SimulateTick(world, runEntity, state, tickDuration);
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.Battle || state.IsPendingVictorySequence)
                {
                    break;
                }
            }
        }

        private void SimulateTick(EcsWorld world, int runEntity, PowerLineBattleStateModel state, float tickDuration)
        {
            state.CurrentTick++;
            _battlePool.Get(runEntity).CurrentTurn = state.CurrentTick;

            if (state.ManaTickInterval > 0 && state.CurrentTick % state.ManaTickInterval == 0)
            {
                var oldMana = state.CurrentMana;
                state.CurrentMana = Mathf.Clamp(state.CurrentMana + state.ManaPerTick, 0, state.MaxMana);
                if (oldMana != state.CurrentMana)
                {
                    _manaPool.Get(runEntity).Value = state.CurrentMana;
                    _manaChangedEventPool.Add(world.NewEntity()).Value = state.CurrentMana;
                }
            }

            SpawnDueEnemies(world, state);
            SimulateAllLanes(world, runEntity, state, tickDuration);
            ResolveDeathsAndDroppedPlugs(world, runEntity, state);

            if (_playerBasePool.Get(runEntity).Value <= 0)
            {
                FinishLevel(world, runEntity, state, false);
                return;
            }

            if (HasUnconnectedLanes(state) &&
                _handStatePool.Has(runEntity) &&
                _handStatePool.Get(runEntity).CardCount <= 0 &&
                state.PlayerUnits.Count <= 0)
            {
                FinishLevel(world, runEntity, state, false);
                return;
            }

            if (PowerLineBattleUtility.GetConnectedLaneCount(state) >= state.Lanes.Count)
            {
                FinishLevel(world, runEntity, state, true);
            }
        }

        private void SpawnDueEnemies(EcsWorld world, PowerLineBattleStateModel state)
        {
            while (state.PendingSpawns.Count > 0 && state.PendingSpawns[0].TimeTick <= state.CurrentTick)
            {
                var spawn = state.PendingSpawns[0];
                state.PendingSpawns.RemoveAt(0);
                var laneState = PowerLineBattleUtility.GetLane(state, spawn.Lane);
                if (laneState == null || laneState.IsConnected)
                {
                    continue;
                }

                var unit = PowerLineBattleUtility.CreateEnemyUnit(state, spawn);
                state.EnemyUnits.Add(unit);
                _powerLineUnitSpawnedEventPool.Add(world.NewEntity()) = new PowerLineUnitSpawnedEvent
                {
                    RuntimeId = unit.RuntimeId,
                    IsEnemy = true,
                    Lane = unit.Lane,
                    Position = unit.Position
                };
            }
        }

        private void SimulateAllLanes(EcsWorld world, int runEntity, PowerLineBattleStateModel state, float tickDuration)
        {
            var movementStep = Mathf.Max(0f, _gameSettingsService.GetPowerLineMovementStep());
            for (var laneIndex = 0; laneIndex < state.Lanes.Count; laneIndex++)
            {
                var laneState = state.Lanes[laneIndex];
                if (laneState.IsConnected)
                {
                    continue;
                }

                SimulatePlayersOnLane(world, runEntity, state, laneState, tickDuration, movementStep);
                if (laneState.IsConnected)
                {
                    continue;
                }

                SimulateEnemiesOnLane(world, state, laneState, tickDuration, movementStep);
            }
        }

        private void SimulatePlayersOnLane(EcsWorld world, int runEntity, PowerLineBattleStateModel state, PowerLineLaneStateModel laneState, float tickDuration, float movementStep)
        {
            var lane = laneState.Lane;
            var units = new List<PowerLineUnitStateModel>();
            for (var index = 0; index < state.PlayerUnits.Count; index++)
            {
                if (state.PlayerUnits[index].Lane == lane && state.PlayerUnits[index].Health > 0)
                {
                    units.Add(state.PlayerUnits[index]);
                }
            }

            units.Sort((left, right) => left.Position.CompareTo(right.Position));

            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                if (unit.Health <= 0 || laneState.IsConnected)
                {
                    continue;
                }

                var target = FindNearestEnemyAhead(state, unit);
                if (target != null)
                {
                    var distance = target.Position - unit.Position;
                    if (distance <= unit.AttackRange)
                    {
                        TickAttack(world, unit, target, tickDuration);
                        continue;
                    }

                    var targetPosition = Mathf.Max(unit.Position, target.Position - unit.AttackRange);
                    var moveDelta = unit.MoveSpeed * movementStep;
                    unit.AttackAccumulator = 0f;
                    unit.Position = Mathf.Clamp(Mathf.Min(unit.Position + moveDelta, targetPosition), 0f, state.LaneLength);
                    if (TryPickupPlug(laneState, unit))
                    {
                        _powerLinePlugStateChangedEventPool.Add(world.NewEntity()) = new PowerLinePlugStateChangedEvent
                        {
                            Lane = laneState.Lane,
                            Status = laneState.Plug.Status,
                            Position = laneState.Plug.Position,
                            CarrierRuntimeId = laneState.Plug.CarrierRuntimeId
                        };
                    }

                    if (unit.IsCarryingPlug)
                    {
                        laneState.Plug.Position = unit.Position;
                        laneState.Plug.CarrierRuntimeId = unit.RuntimeId;
                    }

                    if (unit.IsCarryingPlug && unit.Position >= state.LaneLength)
                    {
                        ConnectLane(world, runEntity, state, laneState);
                    }

                    continue;
                }

                unit.AttackAccumulator = 0f;
                unit.Position = Mathf.Clamp(unit.Position + unit.MoveSpeed * movementStep, 0f, state.LaneLength);
                if (TryPickupPlug(laneState, unit))
                {
                    _powerLinePlugStateChangedEventPool.Add(world.NewEntity()) = new PowerLinePlugStateChangedEvent
                    {
                        Lane = laneState.Lane,
                        Status = laneState.Plug.Status,
                        Position = laneState.Plug.Position,
                        CarrierRuntimeId = laneState.Plug.CarrierRuntimeId
                    };
                }

                if (unit.IsCarryingPlug)
                {
                    laneState.Plug.Position = unit.Position;
                    laneState.Plug.CarrierRuntimeId = unit.RuntimeId;
                }

                if (unit.IsCarryingPlug && unit.Position >= state.LaneLength)
                {
                    ConnectLane(world, runEntity, state, laneState);
                }
            }
        }

        private void SimulateEnemiesOnLane(EcsWorld world, PowerLineBattleStateModel state, PowerLineLaneStateModel laneState, float tickDuration, float movementStep)
        {
            var lane = laneState.Lane;
            var units = new List<PowerLineUnitStateModel>();
            for (var index = 0; index < state.EnemyUnits.Count; index++)
            {
                if (state.EnemyUnits[index].Lane == lane && state.EnemyUnits[index].Health > 0)
                {
                    units.Add(state.EnemyUnits[index]);
                }
            }

            units.Sort((left, right) => right.Position.CompareTo(left.Position));

            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                if (unit.Health <= 0 || laneState.IsConnected)
                {
                    continue;
                }

                var target = FindNearestPlayerAhead(state, unit);
                if (target != null)
                {
                    var distance = unit.Position - target.Position;
                    if (distance <= unit.AttackRange)
                    {
                        TickAttack(world, unit, target, tickDuration);
                        continue;
                    }

                    var targetPosition = Mathf.Min(unit.Position, target.Position + unit.AttackRange);
                    var moveDelta = unit.MoveSpeed * movementStep;
                    unit.AttackAccumulator = 0f;
                    unit.Position = Mathf.Clamp(Mathf.Max(unit.Position - moveDelta, targetPosition), 0f, state.LaneLength);
                    continue;
                }

                if (unit.Position <= unit.AttackRange)
                {
                    TickBaseAttack(world, unit, tickDuration);
                    continue;
                }

                unit.AttackAccumulator = 0f;
                unit.Position = Mathf.Clamp(unit.Position - unit.MoveSpeed * movementStep, 0f, state.LaneLength);
            }
        }

        private void ResolveDeathsAndDroppedPlugs(EcsWorld world, int runEntity, PowerLineBattleStateModel state)
        {
            for (var index = state.PlayerUnits.Count - 1; index >= 0; index--)
            {
                var unit = state.PlayerUnits[index];
                if (unit.Health > 0)
                {
                    continue;
                }

                if (unit.IsCarryingPlug)
                {
                    var laneState = PowerLineBattleUtility.GetLane(state, unit.Lane);
                    if (laneState != null && !laneState.IsConnected)
                    {
                        laneState.Plug.Status = PowerLinePlugStatus.Dropped;
                        laneState.Plug.CarrierRuntimeId = 0;
                        laneState.Plug.Position = unit.Position;
                        _powerLinePlugStateChangedEventPool.Add(world.NewEntity()) = new PowerLinePlugStateChangedEvent
                        {
                            Lane = laneState.Lane,
                            Status = laneState.Plug.Status,
                            Position = laneState.Plug.Position,
                            CarrierRuntimeId = 0
                        };
                    }
                }

                _powerLineUnitDiedEventPool.Add(world.NewEntity()) = new PowerLineUnitDiedEvent
                {
                    RuntimeId = unit.RuntimeId,
                    IsEnemy = false,
                    Lane = unit.Lane,
                    Position = unit.Position,
                    WasCarryingPlug = unit.IsCarryingPlug
                };
                state.PlayerUnits.RemoveAt(index);
            }

            for (var index = state.EnemyUnits.Count - 1; index >= 0; index--)
            {
                var unit = state.EnemyUnits[index];
                if (unit.Health > 0)
                {
                    continue;
                }

                _battlePool.Get(runEntity).TotalEnemyKills++;
                _powerLineUnitDiedEventPool.Add(world.NewEntity()) = new PowerLineUnitDiedEvent
                {
                    RuntimeId = unit.RuntimeId,
                    IsEnemy = true,
                    Lane = unit.Lane,
                    Position = unit.Position,
                    WasCarryingPlug = false
                };
                state.EnemyUnits.RemoveAt(index);
            }
        }

        private PowerLineUnitStateModel FindNearestEnemyAhead(PowerLineBattleStateModel state, PowerLineUnitStateModel unit)
        {
            PowerLineUnitStateModel nearest = null;
            var nearestDistance = float.MaxValue;
            for (var index = 0; index < state.EnemyUnits.Count; index++)
            {
                var candidate = state.EnemyUnits[index];
                if (candidate.Lane != unit.Lane || candidate.Health <= 0 || candidate.Position < unit.Position)
                {
                    continue;
                }

                var distance = candidate.Position - unit.Position;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private PowerLineUnitStateModel FindNearestPlayerAhead(PowerLineBattleStateModel state, PowerLineUnitStateModel unit)
        {
            PowerLineUnitStateModel nearest = null;
            var nearestDistance = float.MaxValue;
            for (var index = 0; index < state.PlayerUnits.Count; index++)
            {
                var candidate = state.PlayerUnits[index];
                if (candidate.Lane != unit.Lane || candidate.Health <= 0 || candidate.Position > unit.Position)
                {
                    continue;
                }

                var distance = unit.Position - candidate.Position;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void TickAttack(EcsWorld world, PowerLineUnitStateModel attacker, PowerLineUnitStateModel target, float tickDuration)
        {
            var attacksPerSecond = attacker.AttackSpeed > 0.001f ? attacker.AttackSpeed : 1f;
            var attackInterval = 1f / attacksPerSecond;
            attacker.AttackAccumulator += tickDuration;
            while (attacker.AttackAccumulator >= attackInterval && target.Health > 0)
            {
                attacker.AttackAccumulator -= attackInterval;
                _powerLineAttackEventPool.Add(world.NewEntity()) = new PowerLineAttackEvent
                {
                    AttackerRuntimeId = attacker.RuntimeId,
                    AttackerIsEnemy = attacker.IsEnemy,
                    TargetIsBase = false,
                    Lane = attacker.Lane,
                    StartPosition = attacker.Position,
                    TargetPosition = target.Position,
                    AttackType = attacker.AttackType,
                    ProjectileSprite = attacker.ProjectileSprite
                };
                target.Health -= attacker.Attack;
                _powerLineDamageEventPool.Add(world.NewEntity()) = new PowerLineDamageEvent
                {
                    TargetRuntimeId = target.RuntimeId,
                    TargetIsEnemy = target.IsEnemy,
                    TargetIsBase = false,
                    Lane = target.Lane,
                    Position = target.Position,
                    Amount = attacker.Attack
                };
            }
        }

        private void TickBaseAttack(EcsWorld world, PowerLineUnitStateModel attacker, float tickDuration)
        {
            var attacksPerSecond = attacker.AttackSpeed > 0.001f ? attacker.AttackSpeed : 1f;
            var attackInterval = 1f / attacksPerSecond;
            attacker.AttackAccumulator += tickDuration;
            while (attacker.AttackAccumulator >= attackInterval)
            {
                attacker.AttackAccumulator -= attackInterval;
                _powerLineAttackEventPool.Add(world.NewEntity()) = new PowerLineAttackEvent
                {
                    AttackerRuntimeId = attacker.RuntimeId,
                    AttackerIsEnemy = attacker.IsEnemy,
                    TargetIsBase = true,
                    Lane = attacker.Lane,
                    StartPosition = attacker.Position,
                    TargetPosition = 0f,
                    AttackType = attacker.AttackType,
                    ProjectileSprite = attacker.ProjectileSprite
                };
                if (_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    ref var playerBase = ref _playerBasePool.Get(runEntity);
                    playerBase.Value = Mathf.Max(0, playerBase.Value - attacker.Attack);
                    _battlePool.Get(runEntity).TotalDamageToPlayerBase += attacker.Attack;
                    _powerLineDamageEventPool.Add(world.NewEntity()) = new PowerLineDamageEvent
                    {
                        TargetRuntimeId = 0,
                        TargetIsEnemy = false,
                        TargetIsBase = true,
                        Lane = attacker.Lane,
                        Position = 0f,
                        Amount = attacker.Attack
                    };
                }
            }
        }

        private static bool TryPickupPlug(PowerLineLaneStateModel laneState, PowerLineUnitStateModel unit)
        {
            if (laneState == null || laneState.IsConnected || unit == null || unit.IsEnemy || unit.IsCarryingPlug)
            {
                return false;
            }

            if (laneState.Plug.Status != PowerLinePlugStatus.AtSpawn &&
                laneState.Plug.Status != PowerLinePlugStatus.Dropped)
            {
                return false;
            }

            if (unit.Position < laneState.Plug.Position)
            {
                return false;
            }

            unit.IsCarryingPlug = true;
            laneState.Plug.Status = PowerLinePlugStatus.Carried;
            laneState.Plug.CarrierRuntimeId = unit.RuntimeId;
            laneState.Plug.Position = unit.Position;
            return true;
        }

        private void ConnectLane(EcsWorld world, int runEntity, PowerLineBattleStateModel state, PowerLineLaneStateModel laneState)
        {
            laneState.IsConnected = true;
            laneState.Plug.Status = PowerLinePlugStatus.Connected;
            laneState.Plug.CarrierRuntimeId = 0;
            laneState.Plug.Position = state.LaneLength;
            _powerLineLaneConnectedEventPool.Add(world.NewEntity()).Lane = laneState.Lane;
            _powerLinePlugStateChangedEventPool.Add(world.NewEntity()) = new PowerLinePlugStateChangedEvent
            {
                Lane = laneState.Lane,
                Status = laneState.Plug.Status,
                Position = laneState.Plug.Position,
                CarrierRuntimeId = 0
            };

            for (var index = state.PlayerUnits.Count - 1; index >= 0; index--)
            {
                if (state.PlayerUnits[index].Lane == laneState.Lane)
                {
                    _powerLineUnitDiedEventPool.Add(world.NewEntity()) = new PowerLineUnitDiedEvent
                    {
                        RuntimeId = state.PlayerUnits[index].RuntimeId,
                        IsEnemy = false,
                        Lane = state.PlayerUnits[index].Lane,
                        Position = state.PlayerUnits[index].Position,
                        WasCarryingPlug = state.PlayerUnits[index].IsCarryingPlug
                    };
                    state.PlayerUnits.RemoveAt(index);
                }
            }

            for (var index = state.EnemyUnits.Count - 1; index >= 0; index--)
            {
                if (state.EnemyUnits[index].Lane == laneState.Lane)
                {
                    _battlePool.Get(runEntity).TotalEnemyKills++;
                    _powerLineUnitDiedEventPool.Add(world.NewEntity()) = new PowerLineUnitDiedEvent
                    {
                        RuntimeId = state.EnemyUnits[index].RuntimeId,
                        IsEnemy = true,
                        Lane = state.EnemyUnits[index].Lane,
                        Position = state.EnemyUnits[index].Position,
                        WasCarryingPlug = false
                    };
                    state.EnemyUnits.RemoveAt(index);
                }
            }

            for (var index = state.PendingSpawns.Count - 1; index >= 0; index--)
            {
                if (state.PendingSpawns[index].Lane == laneState.Lane)
                {
                    state.PendingSpawns.RemoveAt(index);
                }
            }
        }

        private void FinishLevel(EcsWorld world, int runEntity, PowerLineBattleStateModel state, bool isVictory)
        {
            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _levelPool.Get(runEntity).LevelIndex;
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var totalEnemyKills = _battlePool.Get(runEntity).TotalEnemyKills;
            var totalDamageToPlayerBase = _battlePool.Get(runEntity).TotalDamageToPlayerBase;
            var tickPenalty = Mathf.Max(0, state.CurrentTick / 10);
            var baseReward = isVictory && levelData != null ? levelData.VictoryReward : 0;
            var reward = isVictory ? Mathf.Max(0, baseReward + totalEnemyKills * 3 - totalDamageToPlayerBase / 4 - tickPenalty) : 0;

            _battleRuntimeService.CurrentResult = new BattleResultModel
            {
                PlayerBaseHealthBefore = _playerBasePool.Get(runEntity).Value + totalDamageToPlayerBase,
                PlayerBaseHealthAfter = _playerBasePool.Get(runEntity).Value,
                EnemyBaseHealthBefore = 0,
                EnemyBaseHealthAfter = 0,
                EnemyKillsThisTurn = 0,
                EnemyKillsTotal = totalEnemyKills,
                DamageToEnemyBaseThisTurn = 0,
                DamageToEnemyBaseTotal = 0,
                DamageToPlayerBaseThisTurn = 0,
                DamageToPlayerBaseTotal = totalDamageToPlayerBase,
                TurnsSpent = Mathf.Max(1, state.CurrentTick),
                BaseReward = baseReward,
                RewardGranted = reward,
                IsVictory = isVictory,
                IsDefeat = !isVictory
            };

            if (isVictory)
            {
                state.IsPendingVictorySequence = true;
                _saveRunRequestPool.Add(world.NewEntity());
                return;
            }

            _statusPool.Get(runEntity).Value = Enums.RunStatus.Defeat;
            _runFailedEventPool.Add(world.NewEntity());
            _phasePool.Get(runEntity).Value = Enums.PhaseType.Result;
            _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Result;
            _saveRunRequestPool.Add(world.NewEntity());
        }

        private void ProcessFinalizeRequests(EcsWorld world)
        {
            foreach (var requestEntity in _finalizeRequestFilter)
            {
                _finalizeResultRequestPool.Get(requestEntity);
                if (_runEntityIndex.TryGetRunEntity(out var runEntity) &&
                    _levelTypePool.Has(runEntity) &&
                    _levelTypePool.Get(runEntity).Value == Enums.LevelType.PowerLineBattle &&
                    _phasePool.Has(runEntity) &&
                    _phasePool.Get(runEntity).Value == Enums.PhaseType.Battle)
                {
                    var state = _battleRuntimeService.CurrentPowerLineState;
                    var result = _battleRuntimeService.CurrentResult;
                    if (state != null && state.IsPendingVictorySequence && result != null && result.IsVictory && !result.IsDefeat)
                    {
                        state.IsPendingVictorySequence = false;
                        if (result.RewardGranted > 0)
                        {
                            _goldPool.Get(runEntity).Value += result.RewardGranted;
                            _goldChangedEventPool.Add(world.NewEntity()).Value = _goldPool.Get(runEntity).Value;
                        }

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

                        _phasePool.Get(runEntity).Value = Enums.PhaseType.Result;
                        _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Result;
                        _saveRunRequestPool.Add(world.NewEntity());
                    }
                }

                world.DelEntity(requestEntity);
            }
        }

        private bool HasNextLevel(int runEntity)
        {
            var location = _locationConfigService.GetLocation(_locationPool.Get(runEntity).LocationId);
            return location != null &&
                   location.Levels != null &&
                   _levelPool.Get(runEntity).LevelIndex + 1 < location.Levels.Count;
        }

        private static bool HasUnconnectedLanes(PowerLineBattleStateModel state)
        {
            return state != null && PowerLineBattleUtility.GetConnectedLaneCount(state) < state.Lanes.Count;
        }

        private float GetSimulationSpeedMultiplier(PowerLineBattleStateModel state)
        {
            if (state == null)
            {
                return 1f;
            }

            var allEnemyWavesResolved = state.EnemyUnits.Count <= 0 && state.PendingSpawns.Count <= 0;
            if (allEnemyWavesResolved)
            {
                return 2f;
            }

            if (_runEntityIndex.TryGetRunEntity(out var runEntity) &&
                _handStatePool.Has(runEntity) &&
                _handStatePool.Get(runEntity).CardCount <= 0 &&
                state.DeckOwnedUnitRuntimeIds.Count <= 0)
            {
                return 2f;
            }

            return 1f;
        }
    }
}
