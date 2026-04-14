using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View.Controllers;

namespace Plinko.Scripts.ECS.UISystems
{
    public sealed class RefreshPurchasePhaseUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly PurchasePhaseScreenController _controller;
        private readonly UnitConfigService _unitConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _purchaseEnteredFilter;
        private EcsFilter _shopOffersChangedFilter;
        private EcsFilter _goldChangedFilter;
        private EcsFilter _offerFilter;
        private EcsFilter _stagedPurchasedUnitFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePhaseStatePool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _offerUnitTypePool;
        private EcsPool<OfferPriceComponent> _pricePool;

        public RefreshPurchasePhaseUiSystem(PurchasePhaseScreenController controller, UnitConfigService unitConfigService, GameSettingsService gameSettingsService, RunEntityIndex runEntityIndex)
        {
            _controller = controller;
            _unitConfigService = unitConfigService;
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _purchaseEnteredFilter = world.Filter<PurchasePhaseEnteredEvent>().End();
            _shopOffersChangedFilter = world.Filter<ShopOffersChangedEvent>().End();
            _goldChangedFilter = world.Filter<GoldChangedEvent>().End();
            _offerFilter = world.Filter<UnitShopOfferComponent>().End();
            _stagedPurchasedUnitFilter = world.Filter<StagedPurchasedUnitComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _purchasePhaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _offerPool = world.GetPool<UnitShopOfferComponent>();
            _offerUnitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_controller == null || !_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldRefresh = _phaseChangedFilter.GetEntitiesCount() > 0 ||
                                _purchaseEnteredFilter.GetEntitiesCount() > 0 ||
                                _shopOffersChangedFilter.GetEntitiesCount() > 0 ||
                                _goldChangedFilter.GetEntitiesCount() > 0;
            if (!shouldRefresh)
            {
                return;
            }

            var isVisible = _phasePool.Get(runEntity).Value == Enums.PhaseType.PurchasePhase;
            _controller.Show(isVisible);
            if (!isVisible)
            {
                return;
            }

            var rerollPrice = _gameSettingsService.GetUnitShopRerollPrice();
            var currentGold = _goldPool.Get(runEntity).Value;
            var viewData = new PurchasePhaseViewData
            {
                Gold = currentGold,
                RerollCount = _purchasePhaseStatePool.GetOrAdd(runEntity).RerollCount,
                RerollPrice = rerollPrice,
                CanReroll = currentGold >= rerollPrice,
                HasStagedUnits = _stagedPurchasedUnitFilter.GetEntitiesCount() > 0,
                Offers = new List<UnitShopOfferViewData>()
            };

            foreach (var offerEntity in _offerFilter)
            {
                var unitTypeId = _offerUnitTypePool.Get(offerEntity).Value;
                var unit = _unitConfigService.GetUnit(unitTypeId);
                viewData.Offers.Add(new UnitShopOfferViewData
                {
                    OfferId = _offerPool.Get(offerEntity).OfferId,
                    UnitTypeId = unitTypeId,
                    DisplayName = unit != null ? unit.DisplayName : unitTypeId,
                    Price = _pricePool.Get(offerEntity).Value
                });
            }

            _controller.Refresh(viewData);
        }
    }
}