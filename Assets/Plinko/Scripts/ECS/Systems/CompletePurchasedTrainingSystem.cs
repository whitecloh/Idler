using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class CompletePurchasedTrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _eventFilter;
        private EcsFilter _stagedFilter;
        private EcsPool<TrainingCompletedEvent> _trainingCompletedEventPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<PurchasePhaseStateComponent> _purchaseStatePool;
        private EcsPool<RegisterOwnedUnitRequest> _registerOwnedUnitRequestPool;

        public CompletePurchasedTrainingSystem(PlinkoRuntimeService plinkoRuntimeService, RunEntityIndex runEntityIndex)
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
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _purchaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _registerOwnedUnitRequestPool = world.GetPool<RegisterOwnedUnitRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            if (_phasePool.Get(runEntity).Value != Enums.PhaseType.PurchasePhase)
            {
                return;
            }

            foreach (var eventEntity in _eventFilter)
            {
                ref var completedEvent = ref _trainingCompletedEventPool.Get(eventEntity);
                if (completedEvent.IsRetraining)
                {
                    continue;
                }

                var runtimeId = completedEvent.RuntimeId;
                var stagedEntity = -1;
                foreach (var entity in _stagedFilter)
                {
                    if (_stagedPool.Get(entity).RuntimeId == runtimeId && !_stagedPool.Get(entity).IsRetraining)
                    {
                        stagedEntity = entity;
                        break;
                    }
                }

                if (stagedEntity >= 0 && _plinkoRuntimeService.TryGetResult(runtimeId, out var result) &&
                    result != null && result.Result != null)
                {
                    ref var registerRequest = ref _registerOwnedUnitRequestPool.Add(world.NewEntity());
                    registerRequest.RuntimeId = result.Result.RuntimeId;
                    registerRequest.DisplayName = result.Result.DisplayName;
                    registerRequest.Level = result.Result.Level;
                    registerRequest.UnitTypeId = result.Result.UnitTypeId;
                    registerRequest.Attack = result.Result.FinalAttack;
                    registerRequest.Health = result.Result.FinalHealth;
                    registerRequest.ManaCost = result.Result.FinalManaCost;
                    registerRequest.MoveSpeed = result.Result.FinalMoveSpeed;
                    registerRequest.AttackRange = result.Result.FinalAttackRange;
                    registerRequest.AttackSpeed = result.Result.FinalAttackSpeed;
                    registerRequest.PassiveAbilityId = result.Result.PassiveAbilityId;
                    registerRequest.UpgradeCount = result.Result.UpgradeCount;

                    _plinkoRuntimeService.RemoveResult(runtimeId);
                }
                else if (stagedEntity < 0)
                {
                    Debug.LogWarning($"Purchased training completed without a staged trainee entity for unit {runtimeId}.");
                }
                else
                {
                    Debug.LogWarning($"Purchased training completed without a runtime result for unit {runtimeId}.");
                }

                if (stagedEntity >= 0)
                {
                    world.DelEntity(stagedEntity);
                }

                ref var purchaseState = ref _purchaseStatePool.Get(runEntity);
                purchaseState.ActiveTrainingCount = Mathf.Max(0, purchaseState.ActiveTrainingCount - 1);
                purchaseState.CanEnterBattle = purchaseState.ActiveTrainingCount <= 0;

                world.DelEntity(eventEntity);
            }
        }
    }
}
