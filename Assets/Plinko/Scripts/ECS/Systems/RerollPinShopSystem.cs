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
    public sealed class RerollPinShopSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RerollPinShopRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<PinShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferPinTypeIdComponent> _offerPinTypePool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<PinShopOffersChangedEvent> _pinShopOffersChangedEventPool;

        public RerollPinShopSystem(
            GameSettingsService gameSettingsService,
            LevelConfigService levelConfigService,
            PinConfigService pinConfigService,
            WeightedRandomService weightedRandomService,
            RunEntityIndex runEntityIndex,
            PinShopOfferIndex pinShopOfferIndex)
        {
            _gameSettingsService = gameSettingsService;
            _levelConfigService = levelConfigService;
            _pinConfigService = pinConfigService;
            _weightedRandomService = weightedRandomService;
            _runEntityIndex = runEntityIndex;
            _pinShopOfferIndex = pinShopOfferIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RerollPinShopRequest>().End();
            _requestPool = world.GetPool<RerollPinShopRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _offerPool = world.GetPool<PinShopOfferComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerPinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
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
                _requestPool.Get(requestEntity);
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.FieldUpgradePhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var rerollPrice = _gameSettingsService.GetPinShopRerollPrice();
                ref var gold = ref _goldPool.Get(runEntity);
                if (gold.Value < rerollPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= rerollPrice;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                var levelData = _levelConfigService.GetLevel(_locationPool.Get(runEntity).LocationId, _levelPool.Get(runEntity).LevelIndex);
                var pool = BuildUnlockedPool(levelData);
                GenerateFullShop(world, _gameSettingsService.GetPinShopOfferCount(), pool);

                ref var fieldState = ref _fieldUpgradeStatePool.Get(runEntity);
                fieldState.RerollCount++;
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

        private void GenerateFullShop(EcsWorld world, int offerCount, List<PinTypeData> pool)
        {
            ClearOffers(world);
            if (pool == null || pool.Count == 0)
            {
                return;
            }

            for (var offerId = 0; offerId < offerCount; offerId++)
            {
                var pin = _weightedRandomService.Roll(pool, value => value.GenerationWeight);
                if (pin == null)
                {
                    continue;
                }

                var entity = world.NewEntity();
                _offerPool.Add(entity).OfferId = offerId;
                _pricePool.Add(entity).Value = pin.ShopPrice;
                _offerPinTypePool.Add(entity).Value = pin.Id;
                _pinShopOfferIndex.Register(offerId, entity);
            }
        }

        private void ClearOffers(EcsWorld world)
        {
            foreach (var entity in world.Filter<PinShopOfferComponent>().End())
            {
                world.DelEntity(entity);
            }

            _pinShopOfferIndex.Clear();
        }
    }
}
