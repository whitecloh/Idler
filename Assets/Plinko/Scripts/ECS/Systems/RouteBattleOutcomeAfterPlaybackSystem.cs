using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RouteBattleOutcomeAfterPlaybackSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _playbackCompletedFilter;
        private EcsFilter _deployedFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBaseHealthPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBaseHealthPool;
        private EcsPool<RunStatusComponent> _runStatusPool;
        private EcsPool<TurnCompletedEvent> _turnCompletedEventPool;
        private EcsPool<LevelCompletedEvent> _levelCompletedEventPool;
        private EcsPool<RunFailedEvent> _runFailedEventPool;
        private EcsPool<ClearHandRequest> _clearHandRequestPool;
        private EcsPool<GenerateHandRequest> _generateHandRequestPool;

        public RouteBattleOutcomeAfterPlaybackSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _playbackCompletedFilter = world.Filter<BattlePlaybackCompletedEvent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _enemyBaseHealthPool = world.GetPool<EnemyBaseHealthComponent>();
            _playerBaseHealthPool = world.GetPool<PlayerBaseHealthComponent>();
            _runStatusPool = world.GetPool<RunStatusComponent>();
            _turnCompletedEventPool = world.GetPool<TurnCompletedEvent>();
            _levelCompletedEventPool = world.GetPool<LevelCompletedEvent>();
            _runFailedEventPool = world.GetPool<RunFailedEvent>();
            _clearHandRequestPool = world.GetPool<ClearHandRequest>();
            _generateHandRequestPool = world.GetPool<GenerateHandRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var playbackCompletedEntity in _playbackCompletedFilter)
            {
                foreach (var deployedEntity in _deployedFilter)
                {
                    world.DelEntity(deployedEntity);
                }

                var enemyBaseHealth = _enemyBaseHealthPool.GetOrAdd(runEntity).Value;
                var playerBaseHealth = _playerBaseHealthPool.GetOrAdd(runEntity).Value;
                if (enemyBaseHealth <= 0)
                {
                    _phasePool.GetOrAdd(runEntity).Value = Enums.PhaseType.Result;
                    _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Result;
                    _levelCompletedEventPool.Add(world.NewEntity());
                }
                else if (playerBaseHealth <= 0)
                {
                    _phasePool.GetOrAdd(runEntity).Value = Enums.PhaseType.Result;
                    _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Result;
                    _runStatusPool.GetOrAdd(runEntity).Value = Enums.RunStatus.Defeat;
                    _runFailedEventPool.Add(world.NewEntity());
                }
                else
                {
                    _turnCompletedEventPool.Add(world.NewEntity());
                    _clearHandRequestPool.Add(world.NewEntity());
                    _generateHandRequestPool.Add(world.NewEntity());
                }

                world.DelEntity(playbackCompletedEntity);
            }
        }
    }
}