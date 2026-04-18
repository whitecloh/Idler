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
    public sealed class RerollRetrainingShopSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly UnitConfigService _unitConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _offerFilter;
        private EcsPool<RerollRetrainingShopRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
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
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<RetrainingShopOffersChangedEvent> _offersChangedEventPool;

        public RerollRetrainingShopSystem(
            GameSettingsService gameSettingsService,
            UnitConfigService unitConfigService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _unitConfigService = unitConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RerollRetrainingShopRequest>().End();
            _offerFilter = world.Filter<RetrainingShopOfferComponent>().End();
            _requestPool = world.GetPool<RerollRetrainingShopRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
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
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _offersChangedEventPool = world.GetPool<RetrainingShopOffersChangedEvent>();
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
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var eligibleOwnedEntities = RetrainingPhaseUtility.CollectEligibleOwnedEntities(world, _ownedUnitPool, _purchasedOnLevelPool);
                var currentOfferCount = 0;
                foreach (var _ in _offerFilter)
                {
                    currentOfferCount++;
                }

                if (eligibleOwnedEntities.Count <= currentOfferCount)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var rerollPrice = _gameSettingsService.GetRetrainingShopRerollPrice();
                ref var gold = ref _goldPool.Get(runEntity);
                if (gold.Value < rerollPrice)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                gold.Value -= rerollPrice;
                _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;

                RetrainingPhaseUtility.GenerateBatch(
                    world,
                    _retrainingStatePool.Get(runEntity).OfferCount,
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

                _retrainingStatePool.Get(runEntity).RerollCount++;
                _offersChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}
