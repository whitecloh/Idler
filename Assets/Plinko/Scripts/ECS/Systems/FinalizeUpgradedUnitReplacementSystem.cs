using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class FinalizeUpgradedUnitReplacementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _trainingCompletedFilter;
        private EcsFilter _stagedUpgradeUnitFilter;
        private EcsPool<ReplaceOwnedUnitRequest> _replaceOwnedUnitRequestPool;
        private EcsPool<StagedUpgradeUnitComponent> _stagedUpgradePool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<PassiveAbilityIdComponent> _passivePool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _trainingCompletedFilter = world.Filter<TrainingCompletedEvent>().End();
            _stagedUpgradeUnitFilter = world.Filter<StagedUpgradeUnitComponent>().End();
            _replaceOwnedUnitRequestPool = world.GetPool<ReplaceOwnedUnitRequest>();
            _stagedUpgradePool = world.GetPool<StagedUpgradeUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
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

            foreach (var stagedEntity in _stagedUpgradeUnitFilter)
            {
                ref var request = ref _replaceOwnedUnitRequestPool.Add(world.NewEntity());
                request.RuntimeId = _stagedUpgradePool.Get(stagedEntity).RuntimeId;
                request.DisplayName = _displayNamePool.Get(stagedEntity).Value;
                request.Level = _levelPool.Get(stagedEntity).Value + 1;
                request.UnitTypeId = _unitTypePool.Get(stagedEntity).Value;
                request.Attack = _unitStatsPool.Get(stagedEntity).Attack + 1;
                request.Health = _unitStatsPool.Get(stagedEntity).Health + 1;
                request.ManaCost = _manaCostPool.Get(stagedEntity).Value;
                request.PassiveAbilityId = _passivePool.Get(stagedEntity).Value;
                request.UpgradeCount = _upgradeCountPool.Get(stagedEntity).Value + 1;
                world.DelEntity(stagedEntity);
            }
        }
    }
}