using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Events;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class CleanupTransientEventsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _runStartedFilter;
        private EcsFilter _phaseChangedFilter;
        private EcsFilter _goldChangedFilter;
        private EcsFilter _runSavedFilter;
        private EcsFilter _ownedUnitRegisteredFilter;
        private EcsFilter _ownedUnitReplacedFilter;
        private EcsFilter _ownedUnitPoolChangedFilter;
        private EcsFilter _purchasePhaseEnteredFilter;
        private EcsFilter _retrainingPhaseEnteredFilter;
        private EcsFilter _fieldUpgradePhaseEnteredFilter;
        private EcsFilter _shopOffersChangedFilter;
        private EcsFilter _pinShopOffersChangedFilter;
        private EcsFilter _retrainingShopOffersChangedFilter;
        private EcsFilter _retrainingBatchPurchasedFilter;
        private EcsFilter _boardSlotSelectionChangedFilter;
        private EcsFilter _plinkoBoardChangedFilter;
        private EcsFilter _pinPurchasedFilter;
        private EcsFilter _unitPurchasedFilter;
        private EcsFilter _unitTrainingStartedFilter;
        private EcsFilter _trainingPlaybackStartedFilter;
        private EcsFilter _trainingCompletedFilter;
        private EcsFilter _handGeneratedFilter;
        private EcsFilter _handClearedFilter;
        private EcsFilter _unitDeployedFilter;
        private EcsFilter _manaChangedFilter;
        private EcsFilter _enemyWaveSelectedFilter;
        private EcsFilter _powerLineUnitSpawnedFilter;
        private EcsFilter _powerLineDamageFilter;
        private EcsFilter _powerLineUnitDiedFilter;
        private EcsFilter _powerLinePlugStateChangedFilter;
        private EcsFilter _powerLineLaneConnectedFilter;
        private EcsFilter _battleResolvedFilter;
        private EcsFilter _battlePlaybackStartedFilter;
        private EcsFilter _battlePlaybackCompletedFilter;
        private EcsFilter _turnCompletedFilter;
        private EcsFilter _levelCompletedFilter;
        private EcsFilter _runCompletedFilter;
        private EcsFilter _runFailedFilter;
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _runStartedFilter = world.Filter<RunStartedEvent>().End();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _goldChangedFilter = world.Filter<GoldChangedEvent>().End();
            _runSavedFilter = world.Filter<RunSavedEvent>().End();
            _ownedUnitRegisteredFilter = world.Filter<OwnedUnitRegisteredEvent>().End();
            _ownedUnitReplacedFilter = world.Filter<OwnedUnitReplacedEvent>().End();
            _ownedUnitPoolChangedFilter = world.Filter<OwnedUnitPoolChangedEvent>().End();
            _purchasePhaseEnteredFilter = world.Filter<PurchasePhaseEnteredEvent>().End();
            _retrainingPhaseEnteredFilter = world.Filter<RetrainingPhaseEnteredEvent>().End();
            _fieldUpgradePhaseEnteredFilter = world.Filter<FieldUpgradePhaseEnteredEvent>().End();
            _shopOffersChangedFilter = world.Filter<ShopOffersChangedEvent>().End();
            _pinShopOffersChangedFilter = world.Filter<PinShopOffersChangedEvent>().End();
            _retrainingShopOffersChangedFilter = world.Filter<RetrainingShopOffersChangedEvent>().End();
            _retrainingBatchPurchasedFilter = world.Filter<RetrainingBatchPurchasedEvent>().End();
            _boardSlotSelectionChangedFilter = world.Filter<BoardSlotSelectionChangedEvent>().End();
            _plinkoBoardChangedFilter = world.Filter<PlinkoBoardChangedEvent>().End();
            _pinPurchasedFilter = world.Filter<PinPurchasedEvent>().End();
            _unitPurchasedFilter = world.Filter<UnitPurchasedEvent>().End();
            _unitTrainingStartedFilter = world.Filter<UnitTrainingStartedEvent>().End();
            _trainingPlaybackStartedFilter = world.Filter<TrainingPlaybackStartedEvent>().End();
            _trainingCompletedFilter = world.Filter<TrainingCompletedEvent>().End();
            _handGeneratedFilter = world.Filter<HandGeneratedEvent>().End();
            _handClearedFilter = world.Filter<HandClearedEvent>().End();
            _unitDeployedFilter = world.Filter<UnitDeployedEvent>().End();
            _manaChangedFilter = world.Filter<ManaChangedEvent>().End();
            _enemyWaveSelectedFilter = world.Filter<EnemyWaveSelectedEvent>().End();
            _powerLineUnitSpawnedFilter = world.Filter<PowerLineUnitSpawnedEvent>().End();
            _powerLineDamageFilter = world.Filter<PowerLineDamageEvent>().End();
            _powerLineUnitDiedFilter = world.Filter<PowerLineUnitDiedEvent>().End();
            _powerLinePlugStateChangedFilter = world.Filter<PowerLinePlugStateChangedEvent>().End();
            _powerLineLaneConnectedFilter = world.Filter<PowerLineLaneConnectedEvent>().End();
            _battleResolvedFilter = world.Filter<BattleResolvedEvent>().End();
            _battlePlaybackStartedFilter = world.Filter<BattlePlaybackStartedEvent>().End();
            _battlePlaybackCompletedFilter = world.Filter<BattlePlaybackCompletedEvent>().End();
            _turnCompletedFilter = world.Filter<TurnCompletedEvent>().End();
            _levelCompletedFilter = world.Filter<LevelCompletedEvent>().End();
            _runCompletedFilter = world.Filter<RunCompletedEvent>().End();
            _runFailedFilter = world.Filter<RunFailedEvent>().End();
        }
                
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            DeleteFilter(world, _runStartedFilter);
            DeleteFilter(world, _phaseChangedFilter);
            DeleteFilter(world, _goldChangedFilter);
            DeleteFilter(world, _runSavedFilter);
            DeleteFilter(world, _ownedUnitRegisteredFilter);
            DeleteFilter(world, _ownedUnitReplacedFilter);
            DeleteFilter(world, _ownedUnitPoolChangedFilter);
            DeleteFilter(world, _purchasePhaseEnteredFilter);
            DeleteFilter(world, _retrainingPhaseEnteredFilter);
            DeleteFilter(world, _fieldUpgradePhaseEnteredFilter);
            DeleteFilter(world, _shopOffersChangedFilter);
            DeleteFilter(world, _pinShopOffersChangedFilter);
            DeleteFilter(world, _retrainingShopOffersChangedFilter);
            DeleteFilter(world, _retrainingBatchPurchasedFilter);
            DeleteFilter(world, _boardSlotSelectionChangedFilter);
            DeleteFilter(world, _plinkoBoardChangedFilter);
            DeleteFilter(world, _pinPurchasedFilter);
            DeleteFilter(world, _unitPurchasedFilter);
            DeleteFilter(world, _unitTrainingStartedFilter);
            DeleteFilter(world, _trainingPlaybackStartedFilter);
            DeleteFilter(world, _trainingCompletedFilter);
            DeleteFilter(world, _handGeneratedFilter);
            DeleteFilter(world, _handClearedFilter);
            DeleteFilter(world, _unitDeployedFilter);
            DeleteFilter(world, _manaChangedFilter);
            DeleteFilter(world, _enemyWaveSelectedFilter);
            DeleteFilter(world, _powerLineUnitSpawnedFilter);
            DeleteFilter(world, _powerLineDamageFilter);
            DeleteFilter(world, _powerLineUnitDiedFilter);
            DeleteFilter(world, _powerLinePlugStateChangedFilter);
            DeleteFilter(world, _powerLineLaneConnectedFilter);
            DeleteFilter(world, _battleResolvedFilter);
            DeleteFilter(world, _battlePlaybackStartedFilter);
            DeleteFilter(world, _battlePlaybackCompletedFilter);
            DeleteFilter(world, _turnCompletedFilter);
            DeleteFilter(world, _levelCompletedFilter);
            DeleteFilter(world, _runCompletedFilter);
            DeleteFilter(world, _runFailedFilter);
        }
                        
        private static void DeleteFilter(EcsWorld world, EcsFilter filter)
        {
            foreach (var entity in filter)
            {
                world.DelEntity(entity);
            }
        }
    }
}
