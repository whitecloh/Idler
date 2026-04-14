using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ClearHandSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<HandClearedEvent> _handClearedEventPool;

        public ClearHandSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ClearHandRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _handStatePool = world.GetPool<HandStateComponent>();
            _handClearedEventPool = world.GetPool<HandClearedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var requestEntity in _requestFilter)
            {
                foreach (var handCardEntity in _handCardFilter)
                {
                    world.DelEntity(handCardEntity);
                }

                _handStatePool.GetOrAdd(runEntity).CardCount = 0;
                _handClearedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}