using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartBattlePlaybackSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<StartBattlePlaybackRequest> _requestPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<BattlePlaybackStartedEvent> _battlePlaybackStartedEventPool;
        private EcsPool<BattlePlaybackCompletedEvent> _battlePlaybackCompletedEventPool;

        public StartBattlePlaybackSystem(BattleRuntimeService battleRuntimeService, RunEntityIndex runEntityIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartBattlePlaybackRequest>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _requestPool = world.GetPool<StartBattlePlaybackRequest>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _battlePlaybackStartedEventPool = world.GetPool<BattlePlaybackStartedEvent>();
            _battlePlaybackCompletedEventPool = world.GetPool<BattlePlaybackCompletedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                _requestPool.Get(requestEntity);
                if (_battleRuntimeService.CurrentTimeline == null && _battleRuntimeService.CurrentResult == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.BattlePlayback)
                {
                    _phasePool.Get(runEntity).Value = Enums.PhaseType.BattlePlayback;
                    _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.BattlePlayback;
                }

                _battlePlaybackStartedEventPool.Add(world.NewEntity());

                // Until visual playback is implemented, resolved battle data is exposed immediately.
                _battlePlaybackCompletedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}
