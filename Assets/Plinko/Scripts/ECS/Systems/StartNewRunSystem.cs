using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartNewRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LocationConfigService _locationConfigService;
        private readonly UnlocksService _unlocksService;
        private readonly UnitNamingService _unitNamingService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly ShopOfferIndex _shopOfferIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartNewRunRequest> _requestPool;
        private EcsPool<RunComponent> _runPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<BattleStateComponent> _battlePool;
        private EcsPool<RunStartedEvent> _runStartedEventPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<StartLevelRequest> _startLevelRequestPool;
        private EcsPool<RestoreOwnedUnitsRequest> _restoreOwnedUnitsRequestPool;

        public StartNewRunSystem(
            LocationConfigService locationConfigService,
            UnlocksService unlocksService,
            UnitNamingService unitNamingService,
            GameSettingsService gameSettingsService,
            PlinkoRuntimeService plinkoRuntimeService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex,
            ShopOfferIndex shopOfferIndex,
            PinShopOfferIndex pinShopOfferIndex,
            InstalledPinIndex installedPinIndex)
        {
            _locationConfigService = locationConfigService;
            _unlocksService = unlocksService;
            _unitNamingService = unitNamingService;
            _gameSettingsService = gameSettingsService;
            _plinkoRuntimeService = plinkoRuntimeService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
            _shopOfferIndex = shopOfferIndex;
            _pinShopOfferIndex = pinShopOfferIndex;
            _installedPinIndex = installedPinIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartNewRunRequest>().End();
            _requestPool = world.GetPool<StartNewRunRequest>();
            _runPool = world.GetPool<RunComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _purchasePool = world.GetPool<PurchasePhaseStateComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _battlePool = world.GetPool<BattleStateComponent>();
            _runStartedEventPool = world.GetPool<RunStartedEvent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _startLevelRequestPool = world.GetPool<StartLevelRequest>();
            _restoreOwnedUnitsRequestPool = world.GetPool<RestoreOwnedUnitsRequest>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                var location = _locationConfigService.GetLocation(request.LocationId);
                if (location == null || !_unlocksService.IsUnlocked(location.UnlockCondition))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                RuntimeEntityCleanup.ClearForNewRun(world, _runEntityIndex, _ownedUnitIndex, _shopOfferIndex, _pinShopOfferIndex, _installedPinIndex);
                _plinkoRuntimeService.Clear();
                _battleRuntimeService.Clear();

                var runEntity = world.NewEntity();
                _runPool.Add(runEntity);
                _locationPool.Add(runEntity).LocationId = request.LocationId;
                _levelPool.Add(runEntity).LevelIndex = 0;
                _levelTypePool.Add(runEntity).Value = Enums.LevelType.None;
                _phasePool.Add(runEntity).Value = Enums.PhaseType.Location;
                _goldPool.Add(runEntity).Value = _gameSettingsService.GetStartingGold();
                _playerBasePool.Add(runEntity) = new PlayerBaseHealthComponent
                {
                    Value = _gameSettingsService.GetStartingBaseHealth(),
                    MaxValue = _gameSettingsService.GetStartingBaseHealth()
                };
                _enemyBasePool.Add(runEntity) = new EnemyBaseHealthComponent { Value = 0, MaxValue = 0 };
                _statusPool.Add(runEntity).Value = Enums.RunStatus.InProgress;
                _manaPool.Add(runEntity).Value = _gameSettingsService.GetManaPerTurn();
                _handStatePool.Add(runEntity) = new HandStateComponent { CardCount = 0, NextRuntimeId = 1 };
                _purchasePool.Add(runEntity) = new PurchasePhaseStateComponent { RerollCount = 0, ActiveTrainingCount = 0, CanEnterBattle = false };
                _signalPurchasePool.Add(runEntity) = new SignalPurchasePhaseStateComponent
                {
                    RerollCount = 0,
                    ActiveTrainingCount = 0,
                    SignalsLaunchedCount = 0,
                    GeneratorBreakAfterSignalCount = 1,
                    IsGeneratorBroken = false,
                    WillBreakAfterCurrentSignal = false,
                    PassiveIncomeTickElapsed = 0f
                };
                _retrainingPool.Add(runEntity) = new RetrainingPhaseStateComponent
                {
                    OfferCount = _gameSettingsService.GetDefaultRetrainingOfferCount(),
                    RerollCount = 0,
                    ActiveTrainingCount = 0
                };
                _fieldUpgradePool.Add(runEntity) = new FieldUpgradePhaseStateComponent { RerollCount = 0, SelectedSlotIndex = -1, IsPlacementHighlighted = false };
                _battlePool.Add(runEntity) = new BattleStateComponent
                {
                    CurrentTurn = 0,
                    IsResolved = false,
                    NextDeploymentOrder = 0,
                    IsPlayerTurnActive = false,
                    HasGeneratedHandThisTurn = false,
                    TotalEnemyKills = 0,
                    TotalDamageToEnemyBase = 0,
                    TotalDamageToPlayerBase = 0
                };

                _runEntityIndex.SetRunEntity(runEntity);

                var starterUnits = BuildStarterOwnedUnits(location);
                if (starterUnits.Count > 0)
                {
                    _restoreOwnedUnitsRequestPool.Add(world.NewEntity()).OwnedUnits = starterUnits;
                }

                _runStartedEventPool.Add(world.NewEntity());
                _goldChangedEventPool.Add(world.NewEntity()).Value = _gameSettingsService.GetStartingGold();
                _startLevelRequestPool.Add(world.NewEntity()).LevelIndex = 0;
                world.DelEntity(requestEntity);
            }
        }

        private List<OwnedUnitSaveDto> BuildStarterOwnedUnits(LocationData location)
        {
            var result = new List<OwnedUnitSaveDto>();
            if (location?.StartingUnits == null)
            {
                return result;
            }

            var nextRuntimeId = 1;
            for (var index = 0; index < location.StartingUnits.Count; index++)
            {
                var unitType = location.StartingUnits[index];
                if (unitType == null)
                {
                    continue;
                }

                result.Add(new OwnedUnitSaveDto
                {
                    RuntimeId = nextRuntimeId++,
                    DisplayName = _unitNamingService.GetNextDisplayName(unitType.DisplayName),
                    Level = 1,
                    UnitTypeId = unitType.Id,
                    Attack = Mathf.Max(0, unitType.BaseAttack),
                    Health = Mathf.Max(1, unitType.BaseHealth),
                    ManaCost = Mathf.Max(0, unitType.DefaultManaCost),
                    MoveSpeed = Mathf.Max(0f, unitType.BaseMoveSpeed),
                    AttackRange = Mathf.Max(0, unitType.BattleAttackRange),
                    AttackSpeed = Mathf.Max(0f, unitType.BaseAttackSpeed),
                    PassiveAbilityId = unitType.PassiveAbility != null ? unitType.PassiveAbility.Id : string.Empty,
                    UpgradeCount = 0
                });
            }

            return result;
        }
    }
}
