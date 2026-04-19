using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartDefenceBattleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartBattleRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<BaseDefenseTurnStartedEvent> _baseDefenseTurnStartedEventPool;

        public StartDefenceBattleSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartBattleRequest>().End();
            _requestPool = world.GetPool<StartBattleRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _baseDefenseTurnStartedEventPool = world.GetPool<BaseDefenseTurnStartedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                _requestPool.Get(requestEntity);

                if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                    !_levelTypePool.Has(runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (_levelTypePool.Get(runEntity).Value != Enums.LevelType.DefenceBattle)
                {
                    continue;
                }

                var currentPhase = _phasePool.Get(runEntity).Value;
                if (currentPhase != Enums.PhaseType.BattlePreparation || !_battleStatePool.Has(runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var battleState = ref _battleStatePool.Get(runEntity);
                if (!battleState.IsPlayerTurnActive || !battleState.HasGeneratedHandThisTurn)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                battleState.IsResolved = false;
                battleState.IsPlayerTurnActive = false;
                _phasePool.Get(runEntity).Value = Enums.PhaseType.Battle;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Battle;
                _baseDefenseTurnStartedEventPool.Add(world.NewEntity()).TurnIndex = battleState.CurrentTurn;
                world.DelEntity(requestEntity);
            }
        }
    }
}
