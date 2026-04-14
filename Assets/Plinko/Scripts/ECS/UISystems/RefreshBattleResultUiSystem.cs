using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View.Controllers;

namespace Plinko.Scripts.ECS.UISystems
{
    public sealed class RefreshBattleResultUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleResultScreenController _controller;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly BattleRuntimeService _battleRuntimeService;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _levelCompletedFilter;
        private EcsFilter _runCompletedFilter;
        private EcsFilter _runFailedFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;

        public RefreshBattleResultUiSystem(BattleResultScreenController controller, RunEntityIndex runEntityIndex, BattleRuntimeService battleRuntimeService)
        {
            _controller = controller;
            _runEntityIndex = runEntityIndex;
            _battleRuntimeService = battleRuntimeService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _levelCompletedFilter = world.Filter<LevelCompletedEvent>().End();
            _runCompletedFilter = world.Filter<RunCompletedEvent>().End();
            _runFailedFilter = world.Filter<RunFailedEvent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_controller == null || !_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldRefresh = _phaseChangedFilter.GetEntitiesCount() > 0 ||
                                _levelCompletedFilter.GetEntitiesCount() > 0 ||
                                _runCompletedFilter.GetEntitiesCount() > 0 ||
                                _runFailedFilter.GetEntitiesCount() > 0;
            if (!shouldRefresh)
            {
                return;
            }

            var isVisible = _phasePool.Get(runEntity).Value == Enums.PhaseType.Result;
            _controller.Show(isVisible);
            if (!isVisible)
            {
                return;
            }

            var result = _battleRuntimeService.CurrentResult;
            var viewData = new BattleResultViewData
            {
                IsVictory = result != null && result.IsVictory,
                IsDefeat = result != null && !result.IsVictory && result.PlayerBaseHealthAfter <= 0,
                IsRunCompleted = _runCompletedFilter.GetEntitiesCount() > 0,
                PlayerBaseHealthAfter = result != null ? result.PlayerBaseHealthAfter : 0,
                EnemyBaseHealthAfter = result != null ? result.EnemyBaseHealthAfter : 0,
                CanAdvance = _levelCompletedFilter.GetEntitiesCount() > 0 || _runCompletedFilter.GetEntitiesCount() > 0,
                CanReturnToMenu = _runFailedFilter.GetEntitiesCount() > 0 || _runCompletedFilter.GetEntitiesCount() > 0
            };

            _controller.Refresh(viewData);
        }
    }
}