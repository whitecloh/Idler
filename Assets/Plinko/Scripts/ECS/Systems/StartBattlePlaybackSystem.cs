using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartBattlePlaybackSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartBattlePlaybackRequest> _requestPool;
        private EcsPool<BattlePlaybackStateComponent> _battlePlaybackStatePool;
        private EcsPool<BattlePlaybackStartedEvent> _battlePlaybackStartedEventPool;
        private EcsPool<BattlePlaybackCompletedEvent> _battlePlaybackCompletedEventPool;

        public StartBattlePlaybackSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartBattlePlaybackRequest>().End();
            _requestPool = world.GetPool<StartBattlePlaybackRequest>();
            _battlePlaybackStatePool = world.GetPool<BattlePlaybackStateComponent>();
            _battlePlaybackStartedEventPool = world.GetPool<BattlePlaybackStartedEvent>();
            _battlePlaybackCompletedEventPool = world.GetPool<BattlePlaybackCompletedEvent>();
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
                _battlePlaybackStatePool.GetOrAdd(runEntity).IsPlaying = true;
                _battlePlaybackStartedEventPool.Add(world.NewEntity());
                _battlePlaybackStatePool.Get(runEntity).IsPlaying = false;
                _battlePlaybackCompletedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}