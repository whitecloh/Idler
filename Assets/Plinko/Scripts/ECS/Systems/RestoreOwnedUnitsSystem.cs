using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RestoreOwnedUnitsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RestoreOwnedUnitsRequest> _requestPool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<OwnedUnitPoolChangedEvent> _ownedUnitPoolChangedEventPool;

        public RestoreOwnedUnitsSystem(OwnedUnitIndex ownedUnitIndex)
        {
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RestoreOwnedUnitsRequest>().End();
            _requestPool = world.GetPool<RestoreOwnedUnitsRequest>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _ownedUnitPoolChangedEventPool = world.GetPool<OwnedUnitPoolChangedEvent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                var ownedUnits = _requestPool.Get(requestEntity).OwnedUnits;
                if (ownedUnits != null)
                {
                    foreach (var dto in ownedUnits)
                    {
                        var entity = world.NewEntity();
                        _ownedUnitPool.Add(entity).RuntimeId = dto.RuntimeId;
                        _unitTypeIdPool.Add(entity).Value = dto.UnitTypeId;
                        _unitStatsPool.Add(entity) = new UnitStatsComponent { Attack = dto.Attack, Health = dto.Health };
                        _unitManaCostPool.Add(entity).Value = dto.ManaCost;
                        _displayNamePool.Add(entity).Value = dto.DisplayName;
                        _unitLevelPool.Add(entity).Value = dto.Level;
                        _passiveAbilityPool.Add(entity).Value = dto.PassiveAbilityId;
                        _upgradeCountPool.Add(entity).Value = dto.UpgradeCount;
                        _ownedUnitIndex.Register(dto.RuntimeId, entity);
                    }
                }

                _ownedUnitPoolChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}