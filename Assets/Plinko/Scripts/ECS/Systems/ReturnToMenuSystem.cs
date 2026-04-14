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
    public sealed class ReturnToMenuSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly BattleRuntimeService _battleRuntimeService;

        private EcsFilter _requestFilter;
        private EcsPool<ReturnToMenuRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;

        public ReturnToMenuSystem(RunEntityIndex runEntityIndex, BattleRuntimeService battleRuntimeService)
        {
            _runEntityIndex = runEntityIndex;
            _battleRuntimeService = battleRuntimeService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ReturnToMenuRequest>().End();
            _requestPool = world.GetPool<ReturnToMenuRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
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
                _battleRuntimeService.Clear();
                _phasePool.GetOrAdd(runEntity).Value = Enums.PhaseType.MainMenu;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.MainMenu;
                world.DelEntity(requestEntity);
            }
        }
    }
}