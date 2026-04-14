using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RemoveOwnedUnitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RemoveOwnedUnitRequest> _requestPool;
        private EcsPool<OwnedUnitRemovedEvent> _removedEventPool;
        private EcsPool<OwnedUnitPoolChangedEvent> _poolChangedEventPool;

        public RemoveOwnedUnitSystem(OwnedUnitIndex ownedUnitIndex)
        {
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RemoveOwnedUnitRequest>().End();
            _requestPool = world.GetPool<RemoveOwnedUnitRequest>();
            _removedEventPool = world.GetPool<OwnedUnitRemovedEvent>();
            _poolChangedEventPool = world.GetPool<OwnedUnitPoolChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                var runtimeId = _requestPool.Get(requestEntity).RuntimeId;
                if (_ownedUnitIndex.TryGet(runtimeId, out var ownedUnitEntity))
                {
                    _ownedUnitIndex.Unregister(runtimeId);
                    world.DelEntity(ownedUnitEntity);
                    _removedEventPool.Add(world.NewEntity()).RuntimeId = runtimeId;
                    _poolChangedEventPool.Add(world.NewEntity());
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}