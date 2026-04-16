using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Pins;
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
        private readonly GameSettingsService _gameSettingsService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly WeightedRandomService _weightedRandomService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _requestFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<GeneratePinShopOffersRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<PinShopOfferComponent> _offerPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<ShopOfferPinTypeIdComponent> _offerPinTypePool;
        private EcsPool<PendingPurchasedPinComponent> _pendingPinPool;
        private EcsPool<PinShopOffersChangedEvent> _pinShopOffersChangedEventPool;

        public GeneratePinShopOffersSystem(
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
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _requestFilter = world.Filter<GeneratePinShopOffersRequest>().End();
            _pendingFilter = world.Filter<PendingPurchasedPinComponent>().End();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _requestPool = world.GetPool<GeneratePinShopOffersRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _offerPool = world.GetPool<PinShopOfferComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _offerPinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _pendingPinPool = world.GetPool<PendingPurchasedPinComponent>();
            _pinShopOffersChangedEventPool = world.GetPool<PinShopOffersChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldGenerate = false;
            var clearFieldTransientState = false;
            var offerCount = _gameSettingsService.GetPinShopOfferCount();

            foreach (var eventEntity in _phaseChangedFilter)
            {
                if (_phaseChangedEventPool.Get(eventEntity).Value == Enums.PhaseType.FieldUpgradePhase)
                {
                    shouldGenerate = true;
                    clearFieldTransientState = true;
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

            if (_phasePool.Get(runEntity).Value != Enums.PhaseType.FieldUpgradePhase)
            {
                return;
            }

            if (clearFieldTransientState)
            {
                var pendingEntities = new List<int>();
                foreach (var entity in _pendingFilter)
                {
                    pendingEntities.Add(entity);
                }

                foreach (var entity in pendingEntities)
                {
                    _pendingPinPool.Del(entity);
                    world.DelEntity(entity);
                }

                ref var fieldState = ref _fieldUpgradeStatePool.Get(runEntity);
                fieldState.SelectedSlotIndex = -1;
                fieldState.IsPlacementHighlighted = false;
            }

            var levelData = _levelConfigService.GetLevel(_locationPool.Get(runEntity).LocationId, _levelPool.Get(runEntity).LevelIndex);
            var pool = BuildUnlockedPool(levelData);
            GenerateFullShop(world, offerCount, pool);
            _pinShopOffersChangedEventPool.Add(world.NewEntity());
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
