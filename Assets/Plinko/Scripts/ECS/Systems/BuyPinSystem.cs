using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BuyPinSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly PinConfigService _pinConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _candidateFilter;
        private EcsPool<BuyPinRequest> _requestPool;
        private EcsPool<ShopOfferPinTypeIdComponent> _offerPinTypePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<GoldChangedEvent> _goldChangedPool;
        private EcsPool<BoughtPinCandidateComponent> _candidatePool;
        private EcsPool<PinPurchasedEvent> _pinPurchasedEventPool;
        private EcsPool<PinShopOffersChangedEvent> _pinShopOffersChangedEventPool;

        public BuyPinSystem(PinConfigService pinConfigService, RunEntityIndex runEntityIndex, PinShopOfferIndex pinShopOfferIndex)
        {
            _pinConfigService = pinConfigService;
            _runEntityIndex = runEntityIndex;
            _pinShopOfferIndex = pinShopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BuyPinRequest>().End();
            _candidateFilter = world.Filter<BoughtPinCandidateComponent>().End();
            _requestPool = world.GetPool<BuyPinRequest>();
            _offerPinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _goldChangedPool = world.GetPool<GoldChangedEvent>();
            _candidatePool = world.GetPool<BoughtPinCandidateComponent>();
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
                var offerId = _requestPool.Get(requestEntity).OfferId;
                if (!_pinShopOfferIndex.TryGet(offerId, out var offerEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var price = _pricePool.Get(offerEntity).Value;
                ref var gold = ref _goldPool.Get(runEntity);
                if (gold.Value < price)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var pinTypeId = _offerPinTypePool.Get(offerEntity).Value;
                var pin = _pinConfigService.GetPin(pinTypeId);
                if (pin == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= price;
                _goldChangedPool.Add(world.NewEntity()).Value = gold.Value;

                foreach (var candidateEntity in _candidateFilter)
                {
                    world.DelEntity(candidateEntity);
                }

                var boughtPinEntity = world.NewEntity();
                ref var candidate = ref _candidatePool.Add(boughtPinEntity);
                candidate.PinTypeId = pin.Id;
                candidate.OfferId = offerId;

                ref var pinPurchasedEvent = ref _pinPurchasedEventPool.Add(world.NewEntity());
                pinPurchasedEvent.OfferId = offerId;
                pinPurchasedEvent.PinTypeId = pin.Id;

                _pinShopOfferIndex.Unregister(offerId);
                world.DelEntity(offerEntity);
                _pinShopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}