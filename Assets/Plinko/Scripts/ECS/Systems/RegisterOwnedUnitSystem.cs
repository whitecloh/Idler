using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{ 
    public sealed class RegisterOwnedUnitSystem : IEcsInitSystem, IEcsRunSystem 
    {
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RegisterOwnedUnitRequest> _requestPool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<OwnedUnitRegisteredEvent> _registeredEventPool;
        private EcsPool<OwnedUnitPoolChangedEvent> _poolChangedEventPool;

        public RegisterOwnedUnitSystem(OwnedUnitIndex ownedUnitIndex)
        {
            _ownedUnitIndex = ownedUnitIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RegisterOwnedUnitRequest>().End();
            _requestPool = world.GetPool<RegisterOwnedUnitRequest>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _registeredEventPool = world.GetPool<OwnedUnitRegisteredEvent>();
            _poolChangedEventPool = world.GetPool<OwnedUnitPoolChangedEvent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                var entity = world.NewEntity();
                _ownedUnitPool.Add(entity).RuntimeId = request.RuntimeId;
                _unitTypeIdPool.Add(entity).Value = request.UnitTypeId;
                _unitStatsPool.Add(entity) = new UnitStatsComponent { Attack = request.Attack, Health = request.Health };
                _unitManaCostPool.Add(entity).Value = request.ManaCost;
                _displayNamePool.Add(entity).Value = request.DisplayName;
                _unitLevelPool.Add(entity).Value = request.Level;
                _passiveAbilityPool.Add(entity).Value = request.PassiveAbilityId;
                _upgradeCountPool.Add(entity).Value = request.UpgradeCount;
                _ownedUnitIndex.Register(request.RuntimeId, entity);

                _registeredEventPool.Add(world.NewEntity()).RuntimeId = request.RuntimeId;
                _poolChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}