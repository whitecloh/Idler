using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class BuyRetrainingBatchSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _offerFilter;
        private EcsPool<BuyRetrainingBatchRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<RetrainingOfferOwnerUnitComponent> _offerOwnerUnitPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<RetrainingBatchPurchasedEvent> _batchPurchasedEventPool;

        public BuyRetrainingBatchSystem(RunEntityIndex runEntityIndex, OwnedUnitIndex ownedUnitIndex)
        {
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<BuyRetrainingBatchRequest>().End();
            _offerFilter = world.Filter<RetrainingShopOfferComponent>().Inc<RetrainingOfferOwnerUnitComponent>().Inc<OfferPriceComponent>().End();
            _requestPool = world.GetPool<BuyRetrainingBatchRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _offerOwnerUnitPool = world.GetPool<RetrainingOfferOwnerUnitComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _batchPurchasedEventPool = world.GetPool<RetrainingBatchPurchasedEvent>();
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
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var totalPrice = 0;
                var offerCount = 0;
                foreach (var offerEntity in _offerFilter)
                {
                    totalPrice += _pricePool.Get(offerEntity).Value;
                    offerCount++;
                }

                if (offerCount <= 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var gold = ref _goldPool.Get(runEntity);
                if (gold.Value < totalPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= totalPrice;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                foreach (var offerEntity in _offerFilter)
                {
                    var runtimeId = _offerOwnerUnitPool.Get(offerEntity).RuntimeId;
                    if (_ownedUnitIndex.TryGet(runtimeId, out var ownedEntity) && !_purchasedOnLevelPool.Has(ownedEntity))
                    {
                        _purchasedOnLevelPool.Add(ownedEntity);
                    }
                }

                _batchPurchasedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}
