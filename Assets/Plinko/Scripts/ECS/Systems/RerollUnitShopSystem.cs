using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RerollUnitShopSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _requestFilter;
        private EcsPool<RerollUnitShopRequest> _requestPool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePhaseStatePool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<GenerateUnitShopOffersRequest> _generateOffersRequestPool;

        public RerollUnitShopSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RerollUnitShopRequest>().End();
            _requestPool = world.GetPool<RerollUnitShopRequest>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _purchasePhaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _generateOffersRequestPool = world.GetPool<GenerateUnitShopOffersRequest>();
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
                var rerollPrice = _gameSettingsService.GetUnitShopRerollPrice();
                ref var gold = ref _goldPool.GetOrAdd(runEntity);
                if (gold.Value < rerollPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= rerollPrice;
                ref var purchaseState = ref _purchasePhaseStatePool.GetOrAdd(runEntity);
                purchaseState.RerollCount++;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                ref var generateRequest = ref _generateOffersRequestPool.Add(world.NewEntity());
                generateRequest.OfferCount = _gameSettingsService.GetUnitShopOfferCount();
                generateRequest.Offset = purchaseState.RerollCount * Mathf.Max(1, generateRequest.OfferCount);

                world.DelEntity(requestEntity);
            }
        }
    }
}