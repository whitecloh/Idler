using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RerollPowerLineHandSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RerollPowerLineHandRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<DrawPowerLineHandCardsRequest> _drawHandRequestPool;

        public RerollPowerLineHandSystem(
            GameSettingsService gameSettingsService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RerollPowerLineHandRequest>().End();
            _requestPool = world.GetPool<RerollPowerLineHandRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _drawHandRequestPool = world.GetPool<DrawPowerLineHandCardsRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                _requestPool.Get(requestEntity);
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                    !_levelTypePool.Has(runEntity) ||
                    _levelTypePool.Get(runEntity).Value != Enums.LevelType.PowerLineBattle ||
                    !_phasePool.Has(runEntity) ||
                    _phasePool.Get(runEntity).Value != Enums.PhaseType.Battle ||
                    _battleRuntimeService.CurrentPowerLineState == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var rerollCost = _battleRuntimeService.CurrentPowerLineState.RerollManaCost;
                ref var currentMana = ref _manaPool.Get(runEntity);
                if (currentMana.Value < rerollCost)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                currentMana.Value -= rerollCost;
                _battleRuntimeService.CurrentPowerLineState.CurrentMana = currentMana.Value;
                _manaChangedEventPool.Add(world.NewEntity()).Value = currentMana.Value;

                ref var drawRequest = ref _drawHandRequestPool.Add(world.NewEntity());
                drawRequest.Count = _gameSettingsService.GetHandSize();
                drawRequest.ClearExisting = true;
                world.DelEntity(requestEntity);
            }
        }
    }
}
