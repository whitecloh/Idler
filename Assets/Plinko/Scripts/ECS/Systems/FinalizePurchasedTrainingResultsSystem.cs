using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class FinalizePurchasedTrainingResultsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly UnitNamingService _unitNamingService;

        private EcsFilter _trainingCompletedFilter;
        private EcsFilter _stagedPurchasedUnitFilter;
        private EcsPool<RegisterOwnedUnitRequest> _registerOwnedUnitRequestPool;
        private EcsPool<StagedPurchasedUnitComponent> _stagedPurchasedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<PassiveAbilityIdComponent> _passivePool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;

        public FinalizePurchasedTrainingResultsSystem(UnitConfigService unitConfigService, UnitNamingService unitNamingService)
        {
            _unitConfigService = unitConfigService;
            _unitNamingService = unitNamingService;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _trainingCompletedFilter = world.Filter<TrainingCompletedEvent>().End();
            _stagedPurchasedUnitFilter = world.Filter<StagedPurchasedUnitComponent>().End();
            _registerOwnedUnitRequestPool = world.GetPool<RegisterOwnedUnitRequest>();
            _stagedPurchasedUnitPool = world.GetPool<StagedPurchasedUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _passivePool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (_trainingCompletedFilter.GetEntitiesCount() <= 0)
            {
                return;
            }

            foreach (var stagedEntity in _stagedPurchasedUnitFilter)
            {
                ref var request = ref _registerOwnedUnitRequestPool.Add(world.NewEntity());
                request.RuntimeId = _stagedPurchasedUnitPool.Get(stagedEntity).RuntimeId;
                request.UnitTypeId = _unitTypePool.Get(stagedEntity).Value;
                var unitData = _unitConfigService.GetUnit(request.UnitTypeId);
                request.DisplayName = _unitNamingService.GetNextDisplayName(unitData != null ? unitData.DisplayName : request.UnitTypeId);
                request.Level = 1;
                request.Attack = _unitStatsPool.Get(stagedEntity).Attack;
                request.Health = _unitStatsPool.Get(stagedEntity).Health;
                request.ManaCost = _manaCostPool.Get(stagedEntity).Value;
                request.PassiveAbilityId = _passivePool.Get(stagedEntity).Value;
                request.UpgradeCount = _upgradeCountPool.Get(stagedEntity).Value;
                world.DelEntity(stagedEntity);
            }
        }
    }
}