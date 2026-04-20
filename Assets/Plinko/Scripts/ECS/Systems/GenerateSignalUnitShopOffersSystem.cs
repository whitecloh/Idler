using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class GenerateSignalUnitShopOffersSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly UnitConfigService _unitConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _phaseEnteredFilter;
        private EcsFilter _requestFilter;
        private EcsPool<GenerateSignalUnitShopOffersRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _unitTypePool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;

        public GenerateSignalUnitShopOffersSystem(
            GameSettingsService gameSettingsService,
            LevelConfigService levelConfigService,
            UnitConfigService unitConfigService,
            WeightedRandomService weightedRandomService,
            RunEntityIndex runEntityIndex,
            ShopOfferIndex shopOfferIndex)
        {
            _gameSettingsService = gameSettingsService;
            _levelConfigService = levelConfigService;
            _unitConfigService = unitConfigService;
            _weightedRandomService = weightedRandomService;
            _runEntityIndex = runEntityIndex;
            _shopOfferIndex = shopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseEnteredFilter = world.Filter<SignalPurchasePhaseEnteredEvent>().End();
            _requestFilter = world.Filter<GenerateSignalUnitShopOffersRequest>().End();
            _requestPool = world.GetPool<GenerateSignalUnitShopOffersRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _offerPool = world.GetPool<UnitShopOfferComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _unitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            _shopOffersChangedEventPool = world.GetPool<ShopOffersChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldGenerate = false;
            var offerCount = _gameSettingsService.GetUnitShopOfferCount();

            foreach (var _ in _phaseEnteredFilter)
            {
                shouldGenerate = true;
            }

            foreach (var requestEntity in _requestFilter)
            {
                shouldGenerate = true;
                offerCount = Mathf.Max(1, _requestPool.Get(requestEntity).OfferCount);
                world.DelEntity(requestEntity);
            }

            if (!shouldGenerate)
            {
                return;
            }

            var levelData = PurchasePhaseUtility.GetCurrentLevelData(_levelConfigService, _locationPool, _levelPool, runEntity);
            var pool = PurchasePhaseUtility.BuildUnlockedPool(_unitConfigService, levelData);
            PurchasePhaseUtility.GenerateFullShop(
                world,
                offerCount,
                pool,
                _weightedRandomService,
                _shopOfferIndex,
                _offerPool,
                _pricePool,
                _unitTypePool);

            ref var state = ref _signalPurchasePool.Get(runEntity);
            state.RerollCount = 0;
            _shopOffersChangedEventPool.Add(world.NewEntity());
        }
    }
}
