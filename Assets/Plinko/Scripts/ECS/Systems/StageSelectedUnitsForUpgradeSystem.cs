using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StageSelectedUnitsForUpgradeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _confirmedFilter;
        private EcsFilter _selectedOwnedUnitFilter;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
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
            _confirmedFilter = world.Filter<UpgradeSelectionConfirmedEvent>().End();
            _selectedOwnedUnitFilter = world.Filter<OwnedUnitComponent>().Inc<SelectedForUpgradeComponent>().End();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
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
            foreach (var confirmedEntity in _confirmedFilter)
            {
                foreach (var ownedUnitEntity in _selectedOwnedUnitFilter)
                {
                    var stagedEntity = world.NewEntity();
                    _stagedUpgradePool.Add(stagedEntity).RuntimeId = _ownedUnitPool.Get(ownedUnitEntity).RuntimeId;
                    _unitTypePool.Add(stagedEntity).Value = _unitTypePool.Get(ownedUnitEntity).Value;
                    _displayNamePool.Add(stagedEntity).Value = _displayNamePool.Get(ownedUnitEntity).Value;
                    _levelPool.Add(stagedEntity).Value = _levelPool.Get(ownedUnitEntity).Value;

                    ref var stagedStats = ref _unitStatsPool.Add(stagedEntity);
                    var sourceStats = _unitStatsPool.Get(ownedUnitEntity);
                    stagedStats.Attack = sourceStats.Attack;
                    stagedStats.Health = sourceStats.Health;

                    _manaCostPool.Add(stagedEntity).Value = _manaCostPool.Get(ownedUnitEntity).Value;
                    _passivePool.Add(stagedEntity).Value = _passivePool.Get(ownedUnitEntity).Value;
                    _upgradeCountPool.Add(stagedEntity).Value = _upgradeCountPool.Get(ownedUnitEntity).Value;
                }

                world.DelEntity(confirmedEntity);
            }
        }
    }
}