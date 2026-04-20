using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshRetrainingPhaseUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly UnitConfigService _unitConfigService;
        private readonly StatTypeConfigService _statTypeConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;
        private EcsPool<RetrainingShopOfferComponent> _retrainingOfferPool;
        private EcsPool<RetrainingOfferOwnerUnitComponent> _offerOwnerPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<OfferPriceComponent> _pricePool;

        private EcsFilter _ownedFilter;
        private EcsFilter _offerFilter;

        public RefreshRetrainingPhaseUiSystem(
            GameSettingsService gameSettingsService,
            UnitConfigService unitConfigService,
            StatTypeConfigService statTypeConfigService,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _gameSettingsService = gameSettingsService;
            _unitConfigService = unitConfigService;
            _statTypeConfigService = statTypeConfigService;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _retrainingOfferPool = world.GetPool<RetrainingShopOfferComponent>();
            _offerOwnerPool = world.GetPool<RetrainingOfferOwnerUnitComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();

            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
            _offerFilter = world.Filter<RetrainingShopOfferComponent>().Inc<RetrainingOfferOwnerUnitComponent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
            {
                _uiCompositionRoot.RefreshRetrainingPhase(new RetrainingPhaseViewData());
                return;
            }

            var retrainingState = _retrainingStatePool.Get(runEntity);
            var offers = new List<RetrainingOfferViewData>();
            var batchPrice = 0;
            foreach (var offerEntity in _offerFilter)
            {
                var price = _pricePool.Get(offerEntity).Value;
                offers.Add(new RetrainingOfferViewData
                {
                    OfferSlotIndex = _retrainingOfferPool.Get(offerEntity).OfferSlotIndex,
                    RuntimeId = _offerOwnerPool.Get(offerEntity).RuntimeId,
                    DisplayName = _displayNamePool.Get(offerEntity).Value,
                    UnitTypeId = _unitTypePool.Get(offerEntity).Value,
                    Level = _levelPool.Get(offerEntity).Value,
                    Attack = _statsPool.Get(offerEntity).Attack,
                    Health = _statsPool.Get(offerEntity).Health,
                      ManaCost = _manaCostPool.Get(offerEntity).Value,
                      MoveSpeed = _unitCombatStatsPool.Get(offerEntity).MoveSpeed,
                      AttackRange = _unitCombatStatsPool.Get(offerEntity).AttackRange,
                      AttackSpeed = _unitCombatStatsPool.Get(offerEntity).AttackSpeed,
                      UpgradeCount = _upgradeCountPool.Get(offerEntity).Value,
                      Price = price,
                      Stats = StatViewDataFactory.BuildUnitStats(
                          _statTypeConfigService,
                          _unitConfigService.GetUnit(_unitTypePool.Get(offerEntity).Value),
                          _statsPool.Get(offerEntity).Attack,
                          _statsPool.Get(offerEntity).Health,
                          _manaCostPool.Get(offerEntity).Value,
                          _unitCombatStatsPool.Get(offerEntity).MoveSpeed,
                          _unitCombatStatsPool.Get(offerEntity).AttackRange,
                          _unitCombatStatsPool.Get(offerEntity).AttackSpeed)
                  });
                batchPrice += price;
            }
            
            offers.Sort((left, right) => left.OfferSlotIndex.CompareTo(right.OfferSlotIndex));

            var eligibleCount = 0;
            foreach (var ownedEntity in _ownedFilter)
            {
                if (_purchasedOnLevelPool.Has(ownedEntity))
                {
                    continue;
                }

                eligibleCount++;
            }

            var viewData = new RetrainingPhaseViewData
            {
                OfferCount = retrainingState.OfferCount,
                EligibleCount = eligibleCount,
                CurrentOfferCount = offers.Count,
                BatchPrice = batchPrice,
                RerollCount = retrainingState.RerollCount,
                CanAdvance = retrainingState.ActiveTrainingCount <= 0,
                ActiveTrainingCount = retrainingState.ActiveTrainingCount,
                PrimaryActionLabel = "Next Level",
                Offers = offers
            };

            _uiCompositionRoot.RefreshRetrainingPhase(viewData);
        }
    }
}
