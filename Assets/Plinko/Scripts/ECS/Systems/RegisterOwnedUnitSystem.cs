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
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<PassiveAbilityIdComponent> _passivePool;
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
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _passivePool = world.GetPool<PassiveAbilityIdComponent>();
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

                var ownedUnitEntity = world.NewEntity();
                _ownedUnitPool.Add(ownedUnitEntity).RuntimeId = request.RuntimeId;
                _unitTypePool.Add(ownedUnitEntity).Value = request.UnitTypeId;
                _displayNamePool.Add(ownedUnitEntity).Value = request.DisplayName;
                _levelPool.Add(ownedUnitEntity).Value = request.Level;

                ref var stats = ref _unitStatsPool.Add(ownedUnitEntity);
                stats.Attack = request.Attack;
                stats.Health = request.Health;

                _manaCostPool.Add(ownedUnitEntity).Value = request.ManaCost;
                _passivePool.Add(ownedUnitEntity).Value = request.PassiveAbilityId;
                _upgradeCountPool.Add(ownedUnitEntity).Value = request.UpgradeCount;

                _ownedUnitIndex.Register(request.RuntimeId, ownedUnitEntity);
                _registeredEventPool.Add(world.NewEntity()).RuntimeId = request.RuntimeId;
                _poolChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}