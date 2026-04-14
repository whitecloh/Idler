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
    public sealed class RerollPinShopSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _requestFilter;
        private EcsPool<RerollPinShopRequest> _requestPool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePhaseStatePool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<GeneratePinShopOffersRequest> _generateOffersRequestPool;

        public RerollPinShopSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RerollPinShopRequest>().End();
            _requestPool = world.GetPool<RerollPinShopRequest>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _fieldUpgradePhaseStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _generateOffersRequestPool = world.GetPool<GeneratePinShopOffersRequest>();
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
                var rerollPrice = _gameSettingsService.GetPinShopRerollPrice();
                ref var gold = ref _goldPool.GetOrAdd(runEntity);
                if (gold.Value < rerollPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= rerollPrice;
                ref var fieldUpgradeState = ref _fieldUpgradePhaseStatePool.GetOrAdd(runEntity);
                fieldUpgradeState.RerollCount++;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                ref var generateRequest = ref _generateOffersRequestPool.Add(world.NewEntity());
                generateRequest.OfferCount = _gameSettingsService.GetPinShopOfferCount();
                generateRequest.Offset = fieldUpgradeState.RerollCount * Mathf.Max(1, generateRequest.OfferCount);

                world.DelEntity(requestEntity);
            }
        }
    }
}