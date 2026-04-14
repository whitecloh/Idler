using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class PrepareEnemyTurnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<StartBattleRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<EnemyTurnSetupComponent> _enemyTurnSetupPool;
        private EcsPool<EnemyTurnPreparedEvent> _enemyTurnPreparedEventPool;

        public PrepareEnemyTurnSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartBattleRequest>().End();
            _requestPool = world.GetPool<StartBattleRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _enemyTurnSetupPool = world.GetPool<EnemyTurnSetupComponent>();
            _enemyTurnPreparedEventPool = world.GetPool<EnemyTurnPreparedEvent>();
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
                _phasePool.GetOrAdd(runEntity).Value = Enums.PhaseType.Battle;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Battle;

                ref var battleState = ref _battleStatePool.GetOrAdd(runEntity);
                battleState.CurrentTurn++;
                battleState.IsResolved = false;

                _enemyTurnSetupPool.GetOrAdd(runEntity).EnemyPower = Mathf.Max(1, battleState.CurrentTurn);
                _enemyTurnPreparedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}