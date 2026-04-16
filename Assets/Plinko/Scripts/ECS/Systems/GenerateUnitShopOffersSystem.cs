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
    public sealed class GenerateUnitShopOffersSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly UnitConfigService _unitConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _requestFilter;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<GenerateUnitShopOffersRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<PurchasePhaseStateComponent> _purchaseStatePool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _unitTypePool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;

        public GenerateUnitShopOffersSystem(
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
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _requestFilter = world.Filter<GenerateUnitShopOffersRequest>().End();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _requestPool = world.GetPool<GenerateUnitShopOffersRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _purchaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
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

            foreach (var eventEntity in _phaseChangedFilter)
            {
                if (_phaseChangedEventPool.Get(eventEntity).Value == Enums.PhaseType.PurchasePhase)
                {
                    shouldGenerate = true;
                }
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
            PurchasePhaseUtility.GenerateFullShop(world, offerCount, pool, _weightedRandomService, _shopOfferIndex, _offerPool, _pricePool, _unitTypePool);

            ref var purchaseState = ref _purchaseStatePool.Get(runEntity);
            purchaseState.CanEnterBattle = purchaseState.ActiveTrainingCount <= 0;
            _shopOffersChangedEventPool.Add(world.NewEntity());
        }
    }
}