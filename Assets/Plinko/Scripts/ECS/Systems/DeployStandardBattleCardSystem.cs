using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class DeployStandardBattleCardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<DeployCardRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<DeployedForTurnComponent> _deployedPool;
        private EcsPool<DeploymentOrderComponent> _deploymentOrderPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDeployedEvent> _unitDeployedEventPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;

        public DeployStandardBattleCardSystem(RunEntityIndex runEntityIndex, OwnedUnitIndex ownedUnitIndex)
        {
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<DeployCardRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().End();
            _requestPool = world.GetPool<DeployCardRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _deployedPool = world.GetPool<DeployedForTurnComponent>();
            _deploymentOrderPool = world.GetPool<DeploymentOrderComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _unitDeployedEventPool = world.GetPool<UnitDeployedEvent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (!_levelTypePool.Has(runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (request.HasBoardTarget || _levelTypePool.Get(runEntity).Value != Enums.LevelType.StandardBattle)
                {
                    continue;
                }

                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.BattlePreparation ||
                    !_battleStatePool.Has(runEntity))
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

                var handCardEntity = FindHandCardEntity(request.HandCardRuntimeId);
                if (handCardEntity < 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var ownedUnitRuntimeId = _handCardOwnerPool.Get(handCardEntity).OwnedUnitRuntimeId;
                if (!_ownedUnitIndex.TryGet(ownedUnitRuntimeId, out var ownedUnitEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var manaCost = _unitManaCostPool.Get(ownedUnitEntity).Value;
                ref var currentMana = ref _manaPool.Get(runEntity);
                if (currentMana.Value < manaCost)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                currentMana.Value -= manaCost;
                _manaChangedEventPool.Add(world.NewEntity()).Value = currentMana.Value;

                _handCardPool.Del(handCardEntity);
                if (!_deployedPool.Has(handCardEntity))
                {
                    _deployedPool.Add(handCardEntity);
                }

                if (!_deploymentOrderPool.Has(handCardEntity))
                {
                    _deploymentOrderPool.Add(handCardEntity);
                }

                _deploymentOrderPool.Get(handCardEntity).Value = battleState.NextDeploymentOrder++;

                if (_handStatePool.Has(runEntity))
                {
                    ref var handState = ref _handStatePool.Get(runEntity);
                    handState.CardCount = handState.CardCount > 0 ? handState.CardCount - 1 : 0;
                }

                _unitDeployedEventPool.Add(world.NewEntity()).OwnedUnitRuntimeId = ownedUnitRuntimeId;
                world.DelEntity(requestEntity);
            }
        }

        private int FindHandCardEntity(int handCardRuntimeId)
        {
            foreach (var candidateEntity in _handCardFilter)
            {
                if (_handCardPool.Get(candidateEntity).HandCardRuntimeId == handCardRuntimeId)
                {
                    return candidateEntity;
                }
            }

            return -1;
        }
    }
}
