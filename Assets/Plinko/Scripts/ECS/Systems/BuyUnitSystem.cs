using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BuyUnitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _offerFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsFilter _stagedPurchasedUnitFilter;
        private EcsPool<BuyUnitRequest> _requestPool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _offerUnitTypePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<GoldChangedEvent> _goldChangedPool;
        private EcsPool<StagedPurchasedUnitComponent> _stagedPurchasedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<PassiveAbilityIdComponent> _passivePool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<UnitPurchasedEvent> _unitPurchasedEventPool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;    

        public BuyUnitSystem(UnitConfigService unitConfigService, RunEntityIndex runEntityIndex, ShopOfferIndex shopOfferIndex)
        {
            _unitConfigService = unitConfigService;
            _runEntityIndex = runEntityIndex;
            _shopOfferIndex = shopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BuyUnitRequest>().End();
            _offerFilter = world.Filter<UnitShopOfferComponent>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().Inc<UnitTypeIdComponent>().End();
            _stagedPurchasedUnitFilter = world.Filter<StagedPurchasedUnitComponent>().Inc<UnitTypeIdComponent>().End();
            _requestPool = world.GetPool<BuyUnitRequest>();
            _offerPool = world.GetPool<UnitShopOfferComponent>();
            _offerUnitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _goldChangedPool = world.GetPool<GoldChangedEvent>();
            _stagedPurchasedUnitPool = world.GetPool<StagedPurchasedUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _passivePool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _unitPurchasedEventPool = world.GetPool<UnitPurchasedEvent>();
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
                var offerId = _requestPool.Get(requestEntity).OfferId;
                if (!_shopOfferIndex.TryGet(offerId, out var offerEntity))
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

                var unitTypeId = _offerUnitTypePool.Get(offerEntity).Value;
                var unit = _unitConfigService.GetUnit(unitTypeId);
                if (unit == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= price;
                _goldChangedPool.Add(world.NewEntity()).Value = gold.Value;

                var stagedEntity = world.NewEntity();
                ref var stagedUnit = ref _stagedPurchasedUnitPool.Add(stagedEntity);
                var runtimeId = Math.Abs(Guid.NewGuid().GetHashCode());
                stagedUnit.RuntimeId = runtimeId;
                stagedUnit.SourceOfferId = offerId;

                _unitTypePool.Add(stagedEntity).Value = unit.Id;

                ref var stats = ref _unitStatsPool.Add(stagedEntity);
                stats.Attack = unit.BaseAttack;
                stats.Health = unit.BaseHealth;

                _manaCostPool.Add(stagedEntity).Value = unit.ManaCost;
                _passivePool.Add(stagedEntity).Value = unit.PassiveAbilityId;
                _upgradeCountPool.Add(stagedEntity).Value = 0;

                ref var purchasedEvent = ref _unitPurchasedEventPool.Add(world.NewEntity());
                purchasedEvent.OfferId = offerId;
                purchasedEvent.RuntimeId = runtimeId;

                var excludedUnitTypeIds = new HashSet<string>();
                foreach (var ownedUnitEntity in _ownedUnitFilter)
                {
                    excludedUnitTypeIds.Add(_unitTypePool.Get(ownedUnitEntity).Value);
                }

                foreach (var stagedPurchasedUnitEntity in _stagedPurchasedUnitFilter)
                {
                    excludedUnitTypeIds.Add(_unitTypePool.Get(stagedPurchasedUnitEntity).Value);
                }

                foreach (var otherOfferEntity in _offerFilter)
                {
                    if (otherOfferEntity == offerEntity)
                    {
                        continue;
                    }

                    excludedUnitTypeIds.Add(_offerUnitTypePool.Get(otherOfferEntity).Value);
                }

                excludedUnitTypeIds.Add(unit.Id);

                var replacementUnit = _unitConfigService.GetNextShopUnit(unit.Id, excludedUnitTypeIds);
                world.DelEntity(offerEntity);

                if (replacementUnit != null)
                {
                    var replacementOfferEntity = world.NewEntity();
                    _offerPool.Add(replacementOfferEntity).OfferId = offerId;
                    _offerUnitTypePool.Add(replacementOfferEntity).Value = replacementUnit.Id;
                    _pricePool.Add(replacementOfferEntity).Value = replacementUnit.ShopPrice;
                    _shopOfferIndex.Register(offerId, replacementOfferEntity);
                }
                else
                {
                    _shopOfferIndex.Unregister(offerId);
                }

                _shopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}