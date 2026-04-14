using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ReplaceOwnedUnitVersionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsPool<ReplaceOwnedUnitRequest> _requestPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<PassiveAbilityIdComponent> _passivePool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<OwnedUnitReplacedEvent> _replacedEventPool;
        private EcsPool<OwnedUnitPoolChangedEvent> _poolChangedEventPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;

        public ReplaceOwnedUnitVersionSystem(OwnedUnitIndex ownedUnitIndex)
        {
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ReplaceOwnedUnitRequest>().End();
            _requestPool = world.GetPool<ReplaceOwnedUnitRequest>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _passivePool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _replacedEventPool = world.GetPool<OwnedUnitReplacedEvent>();
            _poolChangedEventPool = world.GetPool<OwnedUnitPoolChangedEvent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (_ownedUnitIndex.TryGet(request.RuntimeId, out var ownedUnitEntity))
                {
                    _unitTypePool.Get(ownedUnitEntity).Value = request.UnitTypeId;
                    _displayNamePool.Get(ownedUnitEntity).Value = request.DisplayName;
                    _levelPool.Get(ownedUnitEntity).Value = request.Level;

                    ref var stats = ref _unitStatsPool.Get(ownedUnitEntity);
                    stats.Attack = request.Attack;
                    stats.Health = request.Health;

                    _manaCostPool.Get(ownedUnitEntity).Value = request.ManaCost;
                    _passivePool.Get(ownedUnitEntity).Value = request.PassiveAbilityId;
                    _upgradeCountPool.Get(ownedUnitEntity).Value = request.UpgradeCount;

                    _replacedEventPool.Add(world.NewEntity()).RuntimeId = request.RuntimeId;
                    _poolChangedEventPool.Add(world.NewEntity());
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}