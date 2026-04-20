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
    public sealed class RerollSignalUnitShopSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly UnitConfigService _unitConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly ShopOfferIndex _shopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RerollSignalUnitShopRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<UnitShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferUnitTypeIdComponent> _unitTypePool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<ShopOffersChangedEvent> _shopOffersChangedEventPool;

        public RerollSignalUnitShopSystem(
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
            _requestFilter = world.Filter<RerollSignalUnitShopRequest>().End();
            _requestPool = world.GetPool<RerollSignalUnitShopRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _offerPool = world.GetPool<UnitShopOfferComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _unitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
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
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.SignalPurchasePhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var state = ref _signalPurchasePool.Get(runEntity);
                if (state.IsGeneratorBroken || state.ActiveTrainingCount > 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var rerollPrice = _gameSettingsService.GetUnitShopRerollPrice();
                ref var gold = ref _goldPool.Get(runEntity);
                if (gold.Value < rerollPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= rerollPrice;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                var levelData = PurchasePhaseUtility.GetCurrentLevelData(_levelConfigService, _locationPool, _levelPool, runEntity);
                var pool = PurchasePhaseUtility.BuildUnlockedPool(_unitConfigService, levelData);
                PurchasePhaseUtility.GenerateFullShop(
                    world,
                    _gameSettingsService.GetUnitShopOfferCount(),
                    pool,
                    _weightedRandomService,
                    _shopOfferIndex,
                    _offerPool,
                    _pricePool,
                    _unitTypePool);

                state.RerollCount++;
                _shopOffersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}
