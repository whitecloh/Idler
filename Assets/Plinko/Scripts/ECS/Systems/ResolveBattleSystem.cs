using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ResolveBattleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly BattleRuntimeService _battleRuntimeService;

        private EcsFilter _enemyPreparedFilter;
        private EcsFilter _deployedFilter;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<PlayerBaseHealthComponent> _playerBaseHealthPool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBaseHealthPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<EnemyTurnSetupComponent> _enemyTurnSetupPool;
        private EcsPool<BattleResolvedEvent> _battleResolvedEventPool;
        private EcsPool<StartBattlePlaybackRequest> _startPlaybackRequestPool;

        public ResolveBattleSystem(RunEntityIndex runEntityIndex, BattleRuntimeService battleRuntimeService)
        {
            _runEntityIndex = runEntityIndex;
            _battleRuntimeService = battleRuntimeService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _enemyPreparedFilter = world.Filter<EnemyTurnPreparedEvent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().End();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _playerBaseHealthPool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBaseHealthPool = world.GetPool<EnemyBaseHealthComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _enemyTurnSetupPool = world.GetPool<EnemyTurnSetupComponent>();
            _battleResolvedEventPool = world.GetPool<BattleResolvedEvent>();
            _startPlaybackRequestPool = world.GetPool<StartBattlePlaybackRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _enemyPreparedFilter)
            {
                var totalAttack = 0;
                foreach (var deployedEntity in _deployedFilter)
                {
                    totalAttack += _unitStatsPool.Get(deployedEntity).Attack;
                }

                var enemyPower = _enemyTurnSetupPool.GetOrAdd(runEntity).EnemyPower;
                ref var enemyBaseHealth = ref _enemyBaseHealthPool.GetOrAdd(runEntity);
                ref var playerBaseHealth = ref _playerBaseHealthPool.GetOrAdd(runEntity);

                var enemyDamage = Mathf.Max(0, totalAttack - enemyPower);
                var playerDamage = Mathf.Max(0, enemyPower - totalAttack);
                enemyBaseHealth.Value = Mathf.Max(0, enemyBaseHealth.Value - enemyDamage);
                playerBaseHealth.Value = Mathf.Max(0, playerBaseHealth.Value - playerDamage);

                _battleRuntimeService.CurrentTimeline = new BattleTimelineModel
                {
                    Actions = new List<BattleActionModel>
                    {
                        new BattleActionModel
                        {
                            ActionType = "PlayerAttack",
                            SourceRuntimeId = 0,
                            TargetRuntimeId = 0,
                            Value = enemyDamage
                        },
                        new BattleActionModel
                        {
                            ActionType = "EnemyAttack",
                            SourceRuntimeId = 0,
                            TargetRuntimeId = 0,
                            Value = playerDamage
                        }
                    }
                };

                _battleRuntimeService.CurrentResult = new BattleResultModel
                {
                    PlayerBaseHealthAfter = playerBaseHealth.Value,
                    EnemyBaseHealthAfter = enemyBaseHealth.Value,
                    IsVictory = enemyBaseHealth.Value <= 0 && playerBaseHealth.Value > 0
                };

                _battleStatePool.GetOrAdd(runEntity).IsResolved = true;
                _battleResolvedEventPool.Add(world.NewEntity());
                _startPlaybackRequestPool.Add(world.NewEntity());
                world.DelEntity(eventEntity);
            }
        }
    }
}