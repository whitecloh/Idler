using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class GeneratePinShopOffersSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly PinConfigService _pinConfigService;
        private readonly PinShopOfferIndex _pinShopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsPool<GeneratePinShopOffersRequest> _requestPool;
        private EcsFilter _existingOfferFilter;
        private EcsPool<PinShopOfferComponent> _offerPool;
        private EcsPool<ShopOfferPinTypeIdComponent> _offerPinTypePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<PinShopOffersChangedEvent> _pinShopOffersChangedEventPool;

        public GeneratePinShopOffersSystem(PinConfigService pinConfigService, PinShopOfferIndex pinShopOfferIndex)
        {
            _pinConfigService = pinConfigService;
            _pinShopOfferIndex = pinShopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<GeneratePinShopOffersRequest>().End();
            _requestPool = world.GetPool<GeneratePinShopOffersRequest>();
            _existingOfferFilter = world.Filter<PinShopOfferComponent>().End();
            _offerPool = world.GetPool<PinShopOfferComponent>();
            _offerPinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _pinShopOffersChangedEventPool = world.GetPool<PinShopOffersChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ClearExistingOffers(world);

                var pins = _pinConfigService.GetAllPins();
                var offerCount = _requestPool.Get(requestEntity).OfferCount;
                for (var i = 0; i < offerCount; i++)
                {
                    if (pins.Count <= 0)
                    {
                        break;
                    }

                    var pin = pins[UnityEngine.Random.Range(0, pins.Count)];
                    var offerEntity = world.NewEntity();
                    var offerId = i + 1;

                    _offerPool.Add(offerEntity).OfferId = offerId;
                    _offerPinTypePool.Add(offerEntity).Value = pin.Id;
                    _pricePool.Add(offerEntity).Value = pin.ShopPrice;
                    _pinShopOfferIndex.Register(offerId, offerEntity);
                }

                _pinShopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private void ClearExistingOffers(EcsWorld world)
        {
            foreach (var offerEntity in _existingOfferFilter)
            {
                var offerId = _offerPool.Get(offerEntity).OfferId;
                _pinShopOfferIndex.Unregister(offerId);
                world.DelEntity(offerEntity);
            }

            _pinShopOfferIndex.Clear();
        }
    }
}