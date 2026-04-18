using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class WriteRunSaveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunSaveService _runSaveService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<SaveRunRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<BattleStateComponent> _battlePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<DeployedForTurnComponent> _deployedPool;
        private EcsPool<DeploymentOrderComponent> _deploymentOrderPool;
        private EcsPool<RunSavedEvent> _runSavedEventPool;

        private EcsFilter _ownedUnitFilter;
        private EcsFilter _installedPinFilter;
        private EcsFilter _handCardFilter;
        private EcsFilter _deployedFilter;

        public WriteRunSaveSystem(
            RunSaveService runSaveService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _runSaveService = runSaveService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<SaveRunRequest>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _requestPool = world.GetPool<SaveRunRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _purchasePool = world.GetPool<PurchasePhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _battlePool = world.GetPool<BattleStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _deployedPool = world.GetPool<DeployedForTurnComponent>();
            _deploymentOrderPool = world.GetPool<DeploymentOrderComponent>();
            _runSavedEventPool = world.GetPool<RunSavedEvent>();
            _handCardFilter = world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().Inc<HandCardOwnerUnitComponent>().Inc<DeploymentOrderComponent>().End();
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
                var dto = new RunSaveDto
                {
                    HasActiveRun = true,
                    LocationId = _locationPool.Get(runEntity).LocationId,
                    LevelIndex = _levelPool.Get(runEntity).LevelIndex,
                    LevelType = _levelTypePool.Get(runEntity).Value,
                    PhaseType = _phasePool.Get(runEntity).Value,
                    RunStatus = _statusPool.Get(runEntity).Value,
                    Gold = _goldPool.Get(runEntity).Value,
                    CurrentMana = _manaPool.Has(runEntity) ? _manaPool.Get(runEntity).Value : 0,
                    PlayerBaseHealth = _playerBasePool.Get(runEntity).Value,
                    EnemyBaseHealth = _enemyBasePool.Get(runEntity).Value,
                    BattleTurn = _battlePool.Has(runEntity) ? _battlePool.Get(runEntity).CurrentTurn : 0,
                    HandNextRuntimeId = _handStatePool.Has(runEntity) ? _handStatePool.Get(runEntity).NextRuntimeId : 1,
                    NextDeploymentOrder = _battlePool.Has(runEntity) ? _battlePool.Get(runEntity).NextDeploymentOrder : 0,
                    BattleEnemyKillsTotal = _battlePool.Has(runEntity) ? _battlePool.Get(runEntity).TotalEnemyKills : 0,
                    BattleDamageToEnemyBaseTotal = _battlePool.Has(runEntity) ? _battlePool.Get(runEntity).TotalDamageToEnemyBase : 0,
                    BattleDamageToPlayerBaseTotal = _battlePool.Has(runEntity) ? _battlePool.Get(runEntity).TotalDamageToPlayerBase : 0,
                    PurchaseRerollCount = _purchasePool.Has(runEntity) ? _purchasePool.Get(runEntity).RerollCount : 0,
                    PinRerollCount = _fieldUpgradePool.Has(runEntity) ? _fieldUpgradePool.Get(runEntity).RerollCount : 0,
                    BattleResult = CloneBattleResult(_battleRuntimeService.CurrentResult),
                    OwnedUnits = new List<OwnedUnitSaveDto>(),
                    HandCards = new List<HandCardSaveDto>(),
                    DeployedUnits = new List<DeployedUnitSaveDto>(),
                    Board = new PlinkoBoardSaveDto { InstalledPins = new List<InstalledPinSaveDto>() }
                };

                foreach (var ownedUnitEntity in _ownedUnitFilter)
                {
                    dto.OwnedUnits.Add(new OwnedUnitSaveDto
                    {
                        RuntimeId = _ownedUnitPool.Get(ownedUnitEntity).RuntimeId,
                        UnitTypeId = _unitTypeIdPool.Get(ownedUnitEntity).Value,
                        Attack = _unitStatsPool.Get(ownedUnitEntity).Attack,
                        Health = _unitStatsPool.Get(ownedUnitEntity).Health,
                        ManaCost = _unitManaCostPool.Get(ownedUnitEntity).Value,
                        DisplayName = _displayNamePool.Get(ownedUnitEntity).Value,
                        Level = _unitLevelPool.Get(ownedUnitEntity).Value,
                        PassiveAbilityId = _passiveAbilityPool.Get(ownedUnitEntity).Value,
                        UpgradeCount = _upgradeCountPool.Get(ownedUnitEntity).Value
                    });
                }

                foreach (var installedPinEntity in _installedPinFilter)
                {
                    var installedPin = _installedPinPool.Get(installedPinEntity);
                    dto.Board.InstalledPins.Add(new InstalledPinSaveDto
                    {
                        SlotIndex = installedPin.SlotIndex,
                        PinTypeId = installedPin.PinTypeId
                    });
                }

                foreach (var handCardEntity in _handCardFilter)
                {
                    if (_deployedPool.Has(handCardEntity))
                    {
                        continue;
                    }

                    dto.HandCards.Add(new HandCardSaveDto
                    {
                        HandCardRuntimeId = _handCardPool.Get(handCardEntity).HandCardRuntimeId,
                        OwnedUnitRuntimeId = _handCardOwnerPool.Get(handCardEntity).OwnedUnitRuntimeId
                    });
                }

                foreach (var deployedEntity in _deployedFilter)
                {
                    dto.DeployedUnits.Add(new DeployedUnitSaveDto
                    {
                        OwnedUnitRuntimeId = _handCardOwnerPool.Get(deployedEntity).OwnedUnitRuntimeId,
                        DeploymentOrder = _deploymentOrderPool.Get(deployedEntity).Value
                    });
                }

                _runSaveService.Save(dto);
                _runSavedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private static BattleResultModel CloneBattleResult(BattleResultModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new BattleResultModel
            {
                PlayerBaseHealthBefore = source.PlayerBaseHealthBefore,
                PlayerBaseHealthAfter = source.PlayerBaseHealthAfter,
                EnemyBaseHealthBefore = source.EnemyBaseHealthBefore,
                EnemyBaseHealthAfter = source.EnemyBaseHealthAfter,
                EnemyKillsThisTurn = source.EnemyKillsThisTurn,
                EnemyKillsTotal = source.EnemyKillsTotal,
                DamageToEnemyBaseThisTurn = source.DamageToEnemyBaseThisTurn,
                DamageToEnemyBaseTotal = source.DamageToEnemyBaseTotal,
                DamageToPlayerBaseThisTurn = source.DamageToPlayerBaseThisTurn,
                DamageToPlayerBaseTotal = source.DamageToPlayerBaseTotal,
                TurnsSpent = source.TurnsSpent,
                BaseReward = source.BaseReward,
                RewardGranted = source.RewardGranted,
                IsVictory = source.IsVictory,
                IsDefeat = source.IsDefeat
            };
        }
    }
}
