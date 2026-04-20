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
    public sealed class CompleteSignalPurchaseTrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _eventFilter;
        private EcsFilter _pendingFilter;
        private EcsPool<TrainingCompletedEvent> _trainingCompletedEventPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<StagedTraineeComponent> _stagedPool;
        private EcsPool<SignalPendingUnitComponent> _signalPendingPool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<RegisterOwnedUnitRequest> _registerOwnedUnitRequestPool;
        private EcsPool<SignalGeneratorBrokenEvent> _signalGeneratorBrokenEventPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;

        public CompleteSignalPurchaseTrainingSystem(
            PlinkoRuntimeService plinkoRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _plinkoRuntimeService = plinkoRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _eventFilter = world.Filter<TrainingCompletedEvent>().End();
            _pendingFilter = world.Filter<SignalPendingUnitComponent>().Inc<StagedTraineeComponent>().End();
            _trainingCompletedEventPool = world.GetPool<TrainingCompletedEvent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _stagedPool = world.GetPool<StagedTraineeComponent>();
            _signalPendingPool = world.GetPool<SignalPendingUnitComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _registerOwnedUnitRequestPool = world.GetPool<RegisterOwnedUnitRequest>();
            _signalGeneratorBrokenEventPool = world.GetPool<SignalGeneratorBrokenEvent>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.SignalPurchasePhase)
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

                var stagedEntity = -1;
                foreach (var entity in _pendingFilter)
                {
                    if (_stagedPool.Get(entity).RuntimeId == completedEvent.RuntimeId)
                    {
                        stagedEntity = entity;
                        break;
                    }
                }

                if (stagedEntity >= 0 && _plinkoRuntimeService.TryGetResult(completedEvent.RuntimeId, out var result) &&
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

                    _plinkoRuntimeService.RemoveResult(completedEvent.RuntimeId);
                }
                else if (stagedEntity < 0)
                {
                    Debug.LogWarning($"Signal purchase training completed without a pending staged entity for unit {completedEvent.RuntimeId}.");
                }
                else
                {
                    Debug.LogWarning($"Signal purchase training completed without a runtime result for unit {completedEvent.RuntimeId}.");
                }

                if (stagedEntity >= 0)
                {
                    world.DelEntity(stagedEntity);
                }

                ref var state = ref _signalPurchasePool.Get(runEntity);
                state.ActiveTrainingCount = Mathf.Max(0, state.ActiveTrainingCount - 1);
                if (state.ActiveTrainingCount == 0)
                {
                    if (state.WillBreakAfterCurrentSignal)
                    {
                        state.IsGeneratorBroken = true;
                        state.WillBreakAfterCurrentSignal = false;
                        _signalGeneratorBrokenEventPool.Add(world.NewEntity());
                    }

                    _saveRunRequestPool.Add(world.NewEntity());
                }

                world.DelEntity(eventEntity);
            }
        }
    }
}
