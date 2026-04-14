using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class GenerateUnitShopOffersSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _existingOfferFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsFilter _stagedPurchasedUnitFilter;
        private EcsPool<GenerateUnitShopOffersRequest> _requestPool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _offerUnitTypePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;

        public GenerateUnitShopOffersSystem(UnitConfigService unitConfigService, ShopOfferIndex shopOfferIndex)
        {
            _unitConfigService = unitConfigService;
            _shopOfferIndex = shopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<GenerateUnitShopOffersRequest>().End();
            _existingOfferFilter = world.Filter<UnitShopOfferComponent>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().Inc<UnitTypeIdComponent>().End();
            _stagedPurchasedUnitFilter = world.Filter<StagedPurchasedUnitComponent>().Inc<UnitTypeIdComponent>().End();
            _requestPool = world.GetPool<GenerateUnitShopOffersRequest>();
            _offerPool = world.GetPool<UnitShopOfferComponent>();
            _offerUnitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _shopOffersChangedEventPool = world.GetPool<ShopOffersChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ClearExistingOffers(world);

                var excludedUnitTypeIds = new HashSet<string>();
                foreach (var ownedUnitEntity in _ownedUnitFilter)
                {
                    excludedUnitTypeIds.Add(_unitTypePool.Get(ownedUnitEntity).Value);
                }

                foreach (var stagedUnitEntity in _stagedPurchasedUnitFilter)
                {
                    excludedUnitTypeIds.Add(_unitTypePool.Get(stagedUnitEntity).Value);
                }

                var candidates = _unitConfigService.GetShopUnits(excludedUnitTypeIds);
                var offerCount = Mathf.Min(_requestPool.Get(requestEntity).OfferCount, candidates.Count);
                var offset = _requestPool.Get(requestEntity).Offset;
                for (var i = 0; i < offerCount; i++)
                {
                    var unitIndex = candidates.Count > 0 ? (offset + i) % candidates.Count : 0;
                    var unit = candidates[unitIndex];
                    var offerEntity = world.NewEntity();
                    var offerId = i + 1;

                    _offerPool.Add(offerEntity).OfferId = offerId;
                    _offerUnitTypePool.Add(offerEntity).Value = unit.Id;
                    _pricePool.Add(offerEntity).Value = unit.ShopPrice;
                    _shopOfferIndex.Register(offerId, offerEntity);
                }

                _shopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private void ClearExistingOffers(EcsWorld world)
        {
            foreach (var offerEntity in _existingOfferFilter)
            {
                var offerId = _offerPool.Get(offerEntity).OfferId;
                _shopOfferIndex.Unregister(offerId);
                world.DelEntity(offerEntity);
            }

            _shopOfferIndex.Clear();
        }
    }
}