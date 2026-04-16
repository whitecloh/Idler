using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BuyUnitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly UnitNamingService _unitNamingService;
        private readonly LevelConfigService _levelConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsPool<BuyUnitRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<PurchasePhaseStateComponent> _purchaseStatePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _offerUnitTypePool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitPurchasedEvent> _unitPurchasedEventPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;

        public BuyUnitSystem(
            UnitConfigService unitConfigService,
            UnitNamingService unitNamingService,
            LevelConfigService levelConfigService,
            WeightedRandomService weightedRandomService,
            RunEntityIndex runEntityIndex,
            ShopOfferIndex shopOfferIndex)
        {
            _unitConfigService = unitConfigService;
            _unitNamingService = unitNamingService;
            _levelConfigService = levelConfigService;
            _weightedRandomService = weightedRandomService;
            _runEntityIndex = runEntityIndex;
            _shopOfferIndex = shopOfferIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BuyUnitRequest>().End();
            _requestPool = world.GetPool<BuyUnitRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _purchaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerUnitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitPurchasedEventPool = world.GetPool<UnitPurchasedEvent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _shopOffersChangedEventPool = world.GetPool<ShopOffersChangedEvent>();
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
                ref var request = ref _requestPool.Get(requestEntity);
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.PurchasePhase || !_shopOfferIndex.TryGet(request.OfferId, out var offerEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var offerPrice = _pricePool.Get(offerEntity).Value;
                ref var gold = ref _goldPool.Get(runEntity);
                if (gold.Value < offerPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var purchasedUnitTypeId = _offerUnitTypePool.Get(offerEntity).Value;
                var unitType = _unitConfigService.GetUnit(purchasedUnitTypeId);
                if (unitType == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= offerPrice;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                var runtimeId = PurchasePhaseUtility.GetNextRuntimeId(world, _ownedUnitPool, _stagedPool);
                var stagedEntity = world.NewEntity();
                _stagedPool.Add(stagedEntity) = new StagedTraineeComponent
                {
                    RuntimeId = runtimeId,
                    IsRetraining = false,
                    SourceOfferId = request.OfferId
                };
                _unitTypeIdPool.Add(stagedEntity).Value = purchasedUnitTypeId;
                _displayNamePool.Add(stagedEntity).Value = _unitNamingService.GetNextDisplayName(unitType.DisplayName);

                ref var purchaseState = ref _purchaseStatePool.Get(runEntity);
                purchaseState.ActiveTrainingCount++;
                purchaseState.CanEnterBattle = false;

                var levelData = PurchasePhaseUtility.GetCurrentLevelData(_levelConfigService, _locationPool, _levelPool, runEntity);
                var pool = PurchasePhaseUtility.BuildUnlockedPool(_unitConfigService, levelData);
                PurchasePhaseUtility.RefillOffer(pool, _weightedRandomService, offerEntity, _pricePool, _offerUnitTypePool);

                ref var purchasedEvent = ref _unitPurchasedEventPool.Add(world.NewEntity());
                purchasedEvent.OfferId = request.OfferId;
                purchasedEvent.RuntimeId = runtimeId;
                _shopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}