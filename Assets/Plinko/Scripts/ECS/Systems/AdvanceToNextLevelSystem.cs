using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class AdvanceToNextLevelSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly LocationConfigService _locationConfigService;
        private readonly BattleRuntimeService _battleRuntimeService;

        private EcsFilter _requestFilter;
        private EcsPool<AdvanceToNextLevelRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<StartLevelRequest> _startLevelRequestPool;
        private EcsPool<RunCompletedEvent> _runCompletedEventPool;
        private EcsPool<RunStatusComponent> _runStatusPool;

        public AdvanceToNextLevelSystem(RunEntityIndex runEntityIndex, LocationConfigService locationConfigService, BattleRuntimeService battleRuntimeService)
        {
            _runEntityIndex = runEntityIndex;
            _locationConfigService = locationConfigService;
            _battleRuntimeService = battleRuntimeService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<AdvanceToNextLevelRequest>().End();
            _requestPool = world.GetPool<AdvanceToNextLevelRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _startLevelRequestPool = world.GetPool<StartLevelRequest>();
            _runCompletedEventPool = world.GetPool<RunCompletedEvent>();
            _runStatusPool = world.GetPool<RunStatusComponent>();
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
                var location = _locationConfigService.GetLocation(_locationPool.Get(runEntity).LocationId);
                if (location == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var nextLevelIndex = _levelPool.GetOrAdd(runEntity).LevelIndex + 1;
                _battleRuntimeService.Clear();

                if (nextLevelIndex >= location.Levels.Count)
                {
                    _runStatusPool.GetOrAdd(runEntity).Value = Enums.RunStatus.Victory;
                    _runCompletedEventPool.Add(world.NewEntity());
                    world.DelEntity(requestEntity);
                    continue;
                }

                _phasePool.GetOrAdd(runEntity).Value = Enums.PhaseType.Location;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Location;
                _startLevelRequestPool.Add(world.NewEntity()).LevelIndex = nextLevelIndex;
                world.DelEntity(requestEntity);
            }
        }
    }
}