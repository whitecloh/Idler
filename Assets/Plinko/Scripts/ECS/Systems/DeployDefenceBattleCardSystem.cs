using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class DeployDefenceBattleCardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly UnitConfigService _unitConfigService;

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
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitDeployedEvent> _unitDeployedEventPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;

        public DeployDefenceBattleCardSystem(
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex,
            BattleRuntimeService battleRuntimeService,
            UnitConfigService unitConfigService)
        {
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
            _battleRuntimeService = battleRuntimeService;
            _unitConfigService = unitConfigService;
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
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _unitDeployedEventPool = world.GetPool<UnitDeployedEvent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (!request.HasBoardTarget)
                {
                    continue;
                }

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

                if (!TryDeployToBoard(request, ownedUnitRuntimeId, ownedUnitEntity))
                {
                    currentMana.Value += manaCost;
                    _manaChangedEventPool.Add(world.NewEntity()).Value = currentMana.Value;
                    world.DelEntity(requestEntity);
                    continue;
                }

                world.DelEntity(handCardEntity);

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

        private bool TryDeployToBoard(DeployCardRequest request, int ownedUnitRuntimeId, int ownedUnitEntity)
        {
            var state = _battleRuntimeService.CurrentBaseDefenseState;
            if (state == null)
            {
                return false;
            }

            if (request.TargetLaneIndex < 0 || request.TargetLaneIndex >= state.LaneCount)
            {
                return false;
            }

            if (request.TargetCellIndex < 0 || request.TargetCellIndex >= state.PlayerSideCellCount)
            {
                return false;
            }

            foreach (var unit in state.PlayerUnits)
            {
                if (unit.LaneIndex == request.TargetLaneIndex && unit.CellIndex == request.TargetCellIndex)
                {
                    return false;
                }
            }

            var unitTypeId = _unitTypeIdPool.Get(ownedUnitEntity).Value;
            var unitTypeData = _unitConfigService.GetUnit(unitTypeId);
            state.PlayerUnits.Add(new BaseDefenseUnitStateModel
            {
                RuntimeId = state.NextRuntimeId++,
                SourceOwnedUnitRuntimeId = ownedUnitRuntimeId,
                DisplayName = _displayNamePool.Get(ownedUnitEntity).Value,
                Attack = _unitStatsPool.Get(ownedUnitEntity).Attack,
                Health = _unitStatsPool.Get(ownedUnitEntity).Health,
                ManaCost = _unitManaCostPool.Get(ownedUnitEntity).Value,
                MoveRange = 0,
                AttackRange = _unitCombatStatsPool.Get(ownedUnitEntity).AttackRange,
                MoveSpeed = _unitCombatStatsPool.Get(ownedUnitEntity).MoveSpeed,
                AttackSpeed = _unitCombatStatsPool.Get(ownedUnitEntity).AttackSpeed,
                CanAttackOtherLines = unitTypeData != null && unitTypeData.CanAttackOtherLines,
                CanMoveBetweenLines = false,
                LaneIndex = request.TargetLaneIndex,
                CellIndex = request.TargetCellIndex,
                IsEnemy = false,
                PortraitSprite = unitTypeData != null ? unitTypeData.PortraitSprite : null,
                BattleAnimations = unitTypeData != null ? unitTypeData.BattleAnimations : null
            });
            return true;
        }
    }
}
