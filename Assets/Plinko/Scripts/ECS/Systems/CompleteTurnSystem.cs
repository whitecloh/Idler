using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class CompleteTurnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _playbackCompletedFilter;
        private EcsFilter _deployedFilter;
        private EcsPool<TurnCompletedEvent> _turnCompletedEventPool;
        private EcsPool<ClearHandRequest> _clearHandRequestPool;
        private EcsPool<GenerateHandRequest> _generateHandRequestPool;

        public CompleteTurnSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _playbackCompletedFilter = world.Filter<BattlePlaybackCompletedEvent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().End();
            _turnCompletedEventPool = world.GetPool<TurnCompletedEvent>();
            _clearHandRequestPool = world.GetPool<ClearHandRequest>();
            _generateHandRequestPool = world.GetPool<GenerateHandRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out _))
            {
                return;
            }

            foreach (var playbackCompletedEntity in _playbackCompletedFilter)
            {
                foreach (var deployedEntity in _deployedFilter)
                {
                    world.DelEntity(deployedEntity);
                }

                _turnCompletedEventPool.Add(world.NewEntity());
                _clearHandRequestPool.Add(world.NewEntity());
                _generateHandRequestPool.Add(world.NewEntity());
                world.DelEntity(playbackCompletedEntity);
            }
        }
    }
}