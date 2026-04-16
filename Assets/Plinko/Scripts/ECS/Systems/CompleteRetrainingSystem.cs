using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class CompleteRetrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _eventFilter;
        private EcsFilter _stagedFilter;
        private EcsPool<TrainingCompletedEvent> _trainingCompletedEventPool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<ReplaceOwnedUnitRequest> _replaceOwnedUnitRequestPool;

        public CompleteRetrainingSystem(PlinkoRuntimeService plinkoRuntimeService, RunEntityIndex runEntityIndex)
        {
            _plinkoRuntimeService = plinkoRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _eventFilter = world.Filter<TrainingCompletedEvent>().End();
            _stagedFilter = world.Filter<StagedTraineeComponent>().End();
            _trainingCompletedEventPool = world.GetPool<TrainingCompletedEvent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _replaceOwnedUnitRequestPool = world.GetPool<ReplaceOwnedUnitRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _eventFilter)
            {
                ref var completedEvent = ref _trainingCompletedEventPool.Get(eventEntity);
                if (!completedEvent.IsRetraining)
                {
                    continue;
                }

                var runtimeId = completedEvent.RuntimeId;
                var stagedEntity = -1;
                foreach (var entity in _stagedFilter)
                {
                    if (_stagedPool.Get(entity).RuntimeId == runtimeId && _stagedPool.Get(entity).IsRetraining)
                    {
                        stagedEntity = entity;
                        break;
                    }
                }

                if (_plinkoRuntimeService.TryGetResult(runtimeId, out var result) && result != null && result.Result != null)
                {
                    ref var replaceRequest = ref _replaceOwnedUnitRequestPool.Add(world.NewEntity());
                    replaceRequest.RuntimeId = result.Result.RuntimeId;
                    replaceRequest.DisplayName = result.Result.DisplayName;
                    replaceRequest.Level = result.Result.Level;
                    replaceRequest.UnitTypeId = result.Result.UnitTypeId;
                    replaceRequest.Attack = result.Result.FinalAttack;
                    replaceRequest.Health = result.Result.FinalHealth;
                    replaceRequest.ManaCost = result.Result.FinalManaCost;
                    replaceRequest.PassiveAbilityId = result.Result.PassiveAbilityId;
                    replaceRequest.UpgradeCount = result.Result.UpgradeCount;
                    _plinkoRuntimeService.RemoveResult(runtimeId);
                }
                else
                {
                    Debug.LogWarning($"Retraining completed without a runtime result for unit {runtimeId}.");
                }

                if (stagedEntity >= 0)
                {
                    world.DelEntity(stagedEntity);
                }
                else
                {
                    Debug.LogWarning($"Retraining completed without a staged trainee entity for unit {runtimeId}.");
                }

                ref var retrainingState = ref _retrainingStatePool.Get(runEntity);
                retrainingState.ActiveTrainingCount = Mathf.Max(0, retrainingState.ActiveTrainingCount - 1);
                if (retrainingState.ActiveTrainingCount <= 0)
                {
                    retrainingState.ActiveTrainingCount = 0;
                    retrainingState.SelectedCount = 0;
                }

                world.DelEntity(eventEntity);
            }
        }
    }
}
