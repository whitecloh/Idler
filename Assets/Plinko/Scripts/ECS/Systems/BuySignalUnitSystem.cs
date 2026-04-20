using System.Collections.Generic;
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
    public sealed class BuySignalUnitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly UnitNamingService _unitNamingService;
        private readonly LevelConfigService _levelConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<BuySignalUnitRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _offerUnitTypePool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<SignalPendingUnitComponent> _signalPendingPool;
        private EcsPool<SignalPendingSlotComponent> _signalPendingSlotPool;
        private EcsPool<SignalUnitPurchasedEvent> _signalUnitPurchasedEventPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;

        public BuySignalUnitSystem(
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
            _requestFilter = world.Filter<BuySignalUnitRequest>().End();
            _pendingFilter = world.Filter<SignalPendingUnitComponent>().Inc<SignalPendingSlotComponent>().End();
            _requestPool = world.GetPool<BuySignalUnitRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerUnitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _signalPendingPool = world.GetPool<SignalPendingUnitComponent>();
            _signalPendingSlotPool = world.GetPool<SignalPendingSlotComponent>();
            _signalUnitPurchasedEventPool = world.GetPool<SignalUnitPurchasedEvent>();
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
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.SignalPurchasePhase ||
                    !_shopOfferIndex.TryGet(request.OfferId, out var offerEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var state = ref _signalPurchasePool.Get(runEntity);
                var levelData = PurchasePhaseUtility.GetCurrentLevelData(_levelConfigService, _locationPool, _levelPool, runEntity);
                var slotCount = levelData != null && levelData.SignalPurchase != null
                    ? levelData.SignalPurchase.NewUnitSlotCount
                    : 3;
                var occupiedSlots = BuildOccupiedSlots();
                if (state.IsGeneratorBroken || state.ActiveTrainingCount > 0 || occupiedSlots.Count >= slotCount)
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

                var slotIndex = GetFirstFreeSlot(occupiedSlots, slotCount);
                if (slotIndex < 0)
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
                _signalPendingPool.Add(stagedEntity);
                _signalPendingSlotPool.Add(stagedEntity).Value = slotIndex;
                _unitTypeIdPool.Add(stagedEntity).Value = purchasedUnitTypeId;
                _displayNamePool.Add(stagedEntity).Value = _unitNamingService.GetNextDisplayName(unitType.DisplayName);

                var pool = PurchasePhaseUtility.BuildUnlockedPool(_unitConfigService, levelData);
                PurchasePhaseUtility.RefillOffer(pool, _weightedRandomService, offerEntity, _pricePool, _offerUnitTypePool);

                _signalUnitPurchasedEventPool.Add(world.NewEntity()) = new SignalUnitPurchasedEvent
                {
                    OfferId = request.OfferId,
                    RuntimeId = runtimeId,
                    SlotIndex = slotIndex
                };
                _shopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private HashSet<int> BuildOccupiedSlots()
        {
            var result = new HashSet<int>();
            foreach (var entity in _pendingFilter)
            {
                result.Add(_signalPendingSlotPool.Get(entity).Value);
            }

            return result;
        }

        private static int GetFirstFreeSlot(HashSet<int> occupiedSlots, int slotCount)
        {
            for (var slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                if (!occupiedSlots.Contains(slotIndex))
                {
                    return slotIndex;
                }
            }

            return -1;
        }
    }
}
