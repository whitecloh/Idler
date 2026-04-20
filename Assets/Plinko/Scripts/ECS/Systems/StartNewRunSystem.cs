using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartNewRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LocationConfigService _locationConfigService;
        private readonly UnlocksService _unlocksService;
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

        public StartNewRunSystem(
            LocationConfigService locationConfigService,
            UnlocksService unlocksService,
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

                _runStartedEventPool.Add(world.NewEntity());
                _goldChangedEventPool.Add(world.NewEntity()).Value = _gameSettingsService.GetStartingGold();
                _startLevelRequestPool.Add(world.NewEntity()).LevelIndex = 0;
                world.DelEntity(requestEntity);
            }
        }
    }
}
