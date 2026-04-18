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
    public sealed class GenerateRetrainingShopOffersSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _requestFilter;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<GenerateRetrainingShopOffersRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;
        private EcsPool<RetrainingShopOfferComponent> _retrainingOfferPool;
        private EcsPool<RetrainingOfferOwnerUnitComponent> _offerOwnerUnitPool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<RetrainingShopOffersChangedEvent> _offersChangedEventPool;

        public GenerateRetrainingShopOffersSystem(UnitConfigService unitConfigService, RunEntityIndex runEntityIndex)
        {
            _unitConfigService = unitConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _requestFilter = world.Filter<GenerateRetrainingShopOffersRequest>().End();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _requestPool = world.GetPool<GenerateRetrainingShopOffersRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _retrainingOfferPool = world.GetPool<RetrainingShopOfferComponent>();
            _offerOwnerUnitPool = world.GetPool<RetrainingOfferOwnerUnitComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _offersChangedEventPool = world.GetPool<RetrainingShopOffersChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldGenerate = false;
            var offerCount = _retrainingStatePool.Get(runEntity).OfferCount;

            foreach (var eventEntity in _phaseChangedFilter)
            {
                if (_phaseChangedEventPool.Get(eventEntity).Value == Enums.PhaseType.RetrainingPhase)
                {
                    shouldGenerate = true;
                }
            }

            foreach (var requestEntity in _requestFilter)
            {
                if (_phasePool.Get(runEntity).Value == Enums.PhaseType.RetrainingPhase)
                {
                    shouldGenerate = true;
                    var requestedCount = _requestPool.Get(requestEntity).OfferCount;
                    if (requestedCount > 0)
                    {
                        offerCount = requestedCount;
                        _retrainingStatePool.Get(runEntity).OfferCount = requestedCount;
                    }
                }

                world.DelEntity(requestEntity);
            }

            if (!shouldGenerate)
            {
                return;
            }

            var eligibleOwnedEntities = RetrainingPhaseUtility.CollectEligibleOwnedEntities(world, _ownedUnitPool, _purchasedOnLevelPool);
            RetrainingPhaseUtility.GenerateBatch(
                world,
                offerCount,
                eligibleOwnedEntities,
                _unitConfigService,
                _ownedUnitPool,
                _retrainingOfferPool,
                _offerOwnerUnitPool,
                _pricePool,
                _unitTypeIdPool,
                _displayNamePool,
                _statsPool,
                _manaCostPool,
                _passiveAbilityPool,
                _levelPool,
                _upgradeCountPool);

            _offersChangedEventPool.Add(world.NewEntity());
        }
    }
}
