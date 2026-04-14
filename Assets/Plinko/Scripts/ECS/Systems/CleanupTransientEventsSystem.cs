using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Events;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class CleanupTransientEventsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _phaseChangedFilter;
        private EcsFilter _goldChangedFilter;
        private EcsFilter _purchasePhaseEnteredFilter;
        private EcsFilter _upgradePhaseEnteredFilter;
        private EcsFilter _fieldUpgradePhaseEnteredFilter;
        private EcsFilter _shopOffersChangedFilter;
        private EcsFilter _pinShopOffersChangedFilter;
        private EcsFilter _upgradeSelectionChangedFilter;
        private EcsFilter _upgradeSelectionConfirmedFilter;
        private EcsFilter _boardSlotSelectionChangedFilter;
        private EcsFilter _plinkoBoardChangedFilter;
        private EcsFilter _pinPurchasedFilter;
        private EcsFilter _trainingCompletedFilter;
        private EcsFilter _ownedUnitPoolChangedFilter;
        private EcsFilter _ownedUnitRegisteredFilter;
        private EcsFilter _ownedUnitRemovedFilter;
        private EcsFilter _ownedUnitReplacedFilter;
        private EcsFilter _runStartedFilter;
        private EcsFilter _runSavedFilter;
        private EcsFilter _unitPurchasedFilter;
        private EcsFilter _handGeneratedFilter;
        private EcsFilter _handClearedFilter;
        private EcsFilter _unitDeployedEventFilter;
        private EcsFilter _manaChangedEventFilter;
        private EcsFilter _enemyTurnPreparedEventFilter;
        private EcsFilter _battleResolvedEventFilter;
        private EcsFilter _battlePlaybackStartedEventFilter;
        private EcsFilter _battlePlaybackCompletedEventFilter;
        private EcsFilter _turnCompletedEventFilter;
        private EcsFilter _levelCompletedEventFilter;
        private EcsFilter _runCompletedEventFilter;
        private EcsFilter _runFailedEventFilter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _goldChangedFilter = world.Filter<GoldChangedEvent>().End();
            _purchasePhaseEnteredFilter = world.Filter<PurchasePhaseEnteredEvent>().End();
            _upgradePhaseEnteredFilter = world.Filter<UpgradePhaseEnteredEvent>().End();
            _fieldUpgradePhaseEnteredFilter = world.Filter<FieldUpgradePhaseEnteredEvent>().End();
            _shopOffersChangedFilter = world.Filter<ShopOffersChangedEvent>().End();
            _pinShopOffersChangedFilter = world.Filter<PinShopOffersChangedEvent>().End();
            _upgradeSelectionChangedFilter = world.Filter<UpgradeSelectionChangedEvent>().End();
            _upgradeSelectionConfirmedFilter = world.Filter<UpgradeSelectionConfirmedEvent>().End();
            _boardSlotSelectionChangedFilter = world.Filter<BoardSlotSelectionChangedEvent>().End();
            _plinkoBoardChangedFilter = world.Filter<PlinkoBoardChangedEvent>().End();
            _pinPurchasedFilter = world.Filter<PinPurchasedEvent>().End();
            _trainingCompletedFilter = world.Filter<TrainingCompletedEvent>().End();
            _ownedUnitPoolChangedFilter = world.Filter<OwnedUnitPoolChangedEvent>().End();
            _ownedUnitRegisteredFilter = world.Filter<OwnedUnitRegisteredEvent>().End();
            _ownedUnitRemovedFilter = world.Filter<OwnedUnitRemovedEvent>().End();
            _ownedUnitReplacedFilter = world.Filter<OwnedUnitReplacedEvent>().End();
            _runStartedFilter = world.Filter<RunStartedEvent>().End();
            _runSavedFilter = world.Filter<RunSavedEvent>().End();
            _unitPurchasedFilter = world.Filter<UnitPurchasedEvent>().End();
            _handGeneratedFilter = world.Filter<HandGeneratedEvent>().End();
            _handClearedFilter = world.Filter<HandClearedEvent>().End();
            _unitDeployedEventFilter = world.Filter<UnitDeployedEvent>().End();
            _manaChangedEventFilter = world.Filter<ManaChangedEvent>().End();
            _enemyTurnPreparedEventFilter = world.Filter<EnemyTurnPreparedEvent>().End();
            _battleResolvedEventFilter = world.Filter<BattleResolvedEvent>().End();
            _battlePlaybackStartedEventFilter = world.Filter<BattlePlaybackStartedEvent>().End();
            _battlePlaybackCompletedEventFilter = world.Filter<BattlePlaybackCompletedEvent>().End();
            _turnCompletedEventFilter = world.Filter<TurnCompletedEvent>().End();
            _levelCompletedEventFilter = world.Filter<LevelCompletedEvent>().End();
            _runCompletedEventFilter = world.Filter<RunCompletedEvent>().End();
            _runFailedEventFilter = world.Filter<RunFailedEvent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            DeleteAll(world, _phaseChangedFilter);
            DeleteAll(world, _goldChangedFilter);
            DeleteAll(world, _purchasePhaseEnteredFilter);
            DeleteAll(world, _upgradePhaseEnteredFilter);
            DeleteAll(world, _fieldUpgradePhaseEnteredFilter);
            DeleteAll(world, _shopOffersChangedFilter);
            DeleteAll(world, _pinShopOffersChangedFilter);
            DeleteAll(world, _upgradeSelectionChangedFilter);
            DeleteAll(world, _upgradeSelectionConfirmedFilter);
            DeleteAll(world, _boardSlotSelectionChangedFilter);
            DeleteAll(world, _plinkoBoardChangedFilter);
            DeleteAll(world, _pinPurchasedFilter);
            DeleteAll(world, _trainingCompletedFilter);
            DeleteAll(world, _ownedUnitPoolChangedFilter);
            DeleteAll(world, _ownedUnitRegisteredFilter);
            DeleteAll(world, _ownedUnitRemovedFilter);
            DeleteAll(world, _ownedUnitReplacedFilter);
            DeleteAll(world, _runStartedFilter);
            DeleteAll(world, _runSavedFilter);
            DeleteAll(world, _unitPurchasedFilter);
            DeleteAll(world, _handGeneratedFilter);
            DeleteAll(world, _handClearedFilter);
            DeleteAll(world, _unitDeployedEventFilter);
            DeleteAll(world, _manaChangedEventFilter);
            DeleteAll(world, _enemyTurnPreparedEventFilter);
            DeleteAll(world, _battleResolvedEventFilter);
            DeleteAll(world, _battlePlaybackStartedEventFilter);
            DeleteAll(world, _battlePlaybackCompletedEventFilter);
            DeleteAll(world, _turnCompletedEventFilter);
            DeleteAll(world, _levelCompletedEventFilter);
            DeleteAll(world, _runCompletedEventFilter);
            DeleteAll(world, _runFailedEventFilter);
        }

        private static void DeleteAll(EcsWorld world, EcsFilter filter)
        {
            foreach (var entity in filter)
            {
                world.DelEntity(entity);
            }
        }
    }
}