using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RouteLevelTypeToPhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _levelLoadedFilter;
        private EcsFilter _retrainingOfferFilter;
        private EcsFilter _purchasedOnLevelFilter;
        private EcsPool<LevelLoadedEvent> _levelLoadedEventPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<PurchasePhaseEnteredEvent> _purchasePhaseEnteredEventPool;
        private EcsPool<SignalPurchasePhaseEnteredEvent> _signalPurchasePhaseEnteredEventPool;
        private EcsPool<RetrainingPhaseEnteredEvent> _retrainingPhaseEnteredEventPool;
        private EcsPool<FieldUpgradePhaseEnteredEvent> _fieldUpgradePhaseEnteredEventPool;
        private EcsPool<BeginBattleTurnRequest> _beginBattleTurnRequestPool;
        private EcsPool<InitializePowerLineBattleRequest> _initializePowerLineBattleRequestPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;

        public RouteLevelTypeToPhaseSystem(LevelConfigService levelConfigService, GameSettingsService gameSettingsService, RunEntityIndex runEntityIndex)
        {
            _levelConfigService = levelConfigService;
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _levelLoadedFilter = world.Filter<LevelLoadedEvent>().End();
            _retrainingOfferFilter = world.Filter<RetrainingShopOfferComponent>().End();
            _purchasedOnLevelFilter = world.Filter<RetrainingPurchasedOnLevelComponent>().End();
            _levelLoadedEventPool = world.GetPool<LevelLoadedEvent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _purchasePhaseEnteredEventPool = world.GetPool<PurchasePhaseEnteredEvent>();
            _signalPurchasePhaseEnteredEventPool = world.GetPool<SignalPurchasePhaseEnteredEvent>();
            _retrainingPhaseEnteredEventPool = world.GetPool<RetrainingPhaseEnteredEvent>();
            _fieldUpgradePhaseEnteredEventPool = world.GetPool<FieldUpgradePhaseEnteredEvent>();
            _beginBattleTurnRequestPool = world.GetPool<BeginBattleTurnRequest>();
            _initializePowerLineBattleRequestPool = world.GetPool<InitializePowerLineBattleRequest>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _levelLoadedFilter)
            {
                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
                if (levelData == null)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var nextPhase = Enums.PhaseType.PurchasePhase;
                switch (levelData.LevelType)
                {
                    case Enums.LevelType.Purchase:
                        nextPhase = Enums.PhaseType.PurchasePhase;
                        break;
                    case Enums.LevelType.SignalPurchase:
                        nextPhase = Enums.PhaseType.SignalPurchasePhase;
                        break;
                    case Enums.LevelType.Retraining:
                        nextPhase = Enums.PhaseType.RetrainingPhase;
                        break;
                    case Enums.LevelType.FieldUpgrade:
                        nextPhase = Enums.PhaseType.FieldUpgradePhase;
                        break;
                    case Enums.LevelType.StandardBattle:
                    case Enums.LevelType.DefenceBattle:
                        nextPhase = Enums.PhaseType.BattlePreparation;
                        break;
                    case Enums.LevelType.PowerLineBattle:
                        nextPhase = Enums.PhaseType.Battle;
                        break;
                }

                _phasePool.Get(runEntity).Value = nextPhase;

                ref var signalPurchaseState = ref _signalPurchasePool.Get(runEntity);
                signalPurchaseState.RerollCount = 0;
                signalPurchaseState.ActiveTrainingCount = 0;
                signalPurchaseState.SignalsLaunchedCount = 0;
                signalPurchaseState.PassiveIncomeTickElapsed = 0f;
                signalPurchaseState.IsGeneratorBroken = false;
                signalPurchaseState.WillBreakAfterCurrentSignal = false;
                if (levelData.SignalPurchase != null)
                {
                    var minSignals = levelData.SignalPurchase.GeneratorBreakAfterMinSignals;
                    var maxSignals = levelData.SignalPurchase.GeneratorBreakAfterMaxSignals;
                    signalPurchaseState.GeneratorBreakAfterSignalCount = UnityEngine.Random.Range(minSignals, maxSignals + 1);
                }
                else
                {
                    signalPurchaseState.GeneratorBreakAfterSignalCount = 1;
                }

                ref var retrainingState = ref _retrainingPool.Get(runEntity);
                retrainingState.OfferCount = levelData.PreBattlePhase != null && levelData.PreBattlePhase.OverrideRetrainingOfferCount > 0
                    ? levelData.PreBattlePhase.OverrideRetrainingOfferCount
                    : _gameSettingsService.GetDefaultRetrainingOfferCount();
                retrainingState.RerollCount = 0;
                retrainingState.ActiveTrainingCount = 0;

                ref var fieldUpgradeState = ref _fieldUpgradePool.Get(runEntity);
                fieldUpgradeState.SelectedSlotIndex = -1;
                fieldUpgradeState.IsPlacementHighlighted = false;

                foreach (var offerEntity in _retrainingOfferFilter)
                {
                    world.DelEntity(offerEntity);
                }

                foreach (var purchasedEntity in _purchasedOnLevelFilter)
                {
                    _purchasedOnLevelPool.Del(purchasedEntity);
                }

                _phaseChangedEventPool.Add(world.NewEntity()).Value = nextPhase;
                switch (nextPhase)
                {
                    case Enums.PhaseType.PurchasePhase:
                        _purchasePhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.SignalPurchasePhase:
                        _signalPurchasePhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.RetrainingPhase:
                        _retrainingPhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.FieldUpgradePhase:
                        _fieldUpgradePhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.BattlePreparation:
                        _beginBattleTurnRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.Battle:
                        if (levelData.LevelType == Enums.LevelType.PowerLineBattle)
                        {
                            _initializePowerLineBattleRequestPool.Add(world.NewEntity());
                        }
                        break;
                }

                world.DelEntity(eventEntity);
            }
        }
    }
}
