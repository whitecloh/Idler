using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ReplaceOwnedUnitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsPool<ReplaceOwnedUnitRequest> _requestPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<OwnedUnitReplacedEvent> _replacedEventPool;
        private EcsPool<OwnedUnitPoolChangedEvent> _poolChangedEventPool;

        public ReplaceOwnedUnitSystem(OwnedUnitIndex ownedUnitIndex)
        {
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ReplaceOwnedUnitRequest>().End();
            _requestPool = world.GetPool<ReplaceOwnedUnitRequest>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _replacedEventPool = world.GetPool<OwnedUnitReplacedEvent>();
            _poolChangedEventPool = world.GetPool<OwnedUnitPoolChangedEvent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (_ownedUnitIndex.TryGet(request.RuntimeId, out var entity))
                {
                    _unitTypeIdPool.Get(entity).Value = request.UnitTypeId;
                    _unitStatsPool.Get(entity) = new UnitStatsComponent { Attack = request.Attack, Health = request.Health };
                    _unitCombatStatsPool.Get(entity) = new UnitCombatStatsComponent
                    {
                        MoveSpeed = request.MoveSpeed,
                        AttackRange = request.AttackRange,
                        AttackSpeed = request.AttackSpeed
                    };
                    _unitManaCostPool.Get(entity).Value = request.ManaCost;
                    _displayNamePool.Get(entity).Value = request.DisplayName;
                    _unitLevelPool.Get(entity).Value = request.Level;
                    _passiveAbilityPool.Get(entity).Value = request.PassiveAbilityId;
                    _upgradeCountPool.Get(entity).Value = request.UpgradeCount;
                    _replacedEventPool.Add(world.NewEntity()).RuntimeId = request.RuntimeId;
                    _poolChangedEventPool.Add(world.NewEntity());
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}
