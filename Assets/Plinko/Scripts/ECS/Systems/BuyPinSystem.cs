using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BuyPinSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<BuyPinRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferPinTypeIdComponent> _offerPinTypePool;
        private EcsPool<PendingPurchasedPinComponent> _pendingPinPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<PinPurchasedEvent> _pinPurchasedEventPool;
        private EcsPool<PinShopOffersChangedEvent> _pinShopOffersChangedEventPool;

        public BuyPinSystem(
            LevelConfigService levelConfigService,
            PinConfigService pinConfigService,
            WeightedRandomService weightedRandomService,
            RunEntityIndex runEntityIndex,
            PinShopOfferIndex pinShopOfferIndex)
        {
            _levelConfigService = levelConfigService;
            _pinConfigService = pinConfigService;
            _weightedRandomService = weightedRandomService;
            _runEntityIndex = runEntityIndex;
            _pinShopOfferIndex = pinShopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BuyPinRequest>().End();
            _pendingFilter = world.Filter<PendingPurchasedPinComponent>().End();
            _requestPool = world.GetPool<BuyPinRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerPinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _pendingPinPool = world.GetPool<PendingPurchasedPinComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _pinPurchasedEventPool = world.GetPool<PinPurchasedEvent>();
            _pinShopOffersChangedEventPool = world.GetPool<PinShopOffersChangedEvent>();
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
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.FieldUpgradePhase || !_pinShopOfferIndex.TryGet(request.OfferId, out var offerEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var hasPendingPin = false;
                foreach (var _ in _pendingFilter)
                {
                    hasPendingPin = true;
                    break;
                }

                if (hasPendingPin)
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

                var purchasedPinTypeId = _offerPinTypePool.Get(offerEntity).Value;
                var pinType = _pinConfigService.GetPin(purchasedPinTypeId);
                if (pinType == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= offerPrice;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                _pendingPinPool.Add(world.NewEntity()) = new PendingPurchasedPinComponent
                {
                    OfferId = request.OfferId,
                    PinTypeId = purchasedPinTypeId
                };

                ref var fieldState = ref _fieldUpgradeStatePool.Get(runEntity);
                fieldState.IsPlacementHighlighted = fieldState.SelectedSlotIndex >= 0;

                var levelData = _levelConfigService.GetLevel(_locationPool.Get(runEntity).LocationId, _levelPool.Get(runEntity).LevelIndex);
                var pool = BuildUnlockedPool(levelData);
                RefillOffer(pool, offerEntity);

                ref var pinPurchasedEvent = ref _pinPurchasedEventPool.Add(world.NewEntity());
                pinPurchasedEvent.OfferId = request.OfferId;
                pinPurchasedEvent.PinTypeId = purchasedPinTypeId;
                _pinShopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private List<PinTypeData> BuildUnlockedPool(Plinko.Scripts.Data.Levels.LevelData levelData)
        {
            var result = new List<PinTypeData>();
            var pool = _pinConfigService.GetUnlockedShopPool(levelData);
            if (pool == null)
            {
                return result;
            }

            foreach (var pin in pool)
            {
                if (pin != null)
                {
                    result.Add(pin);
                }
            }

            return result;
        }

        private void RefillOffer(List<PinTypeData> pool, int offerEntity)
        {
            if (pool == null || pool.Count == 0)
            {
                return;
            }

            var pin = _weightedRandomService.Roll(pool, value => value.GenerationWeight);
            if (pin == null)
            {
                return;
            }

            _pricePool.Get(offerEntity).Value = pin.ShopPrice;
            _offerPinTypePool.Get(offerEntity).Value = pin.Id;
        }
    }
}
