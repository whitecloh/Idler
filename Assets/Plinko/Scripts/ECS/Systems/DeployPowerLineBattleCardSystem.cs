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
    public sealed class DeployPowerLineBattleCardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly UnitConfigService _unitConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<DeployCardRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<DrawPowerLineHandCardsRequest> _drawHandRequestPool;
        private EcsPool<UnitDeployedEvent> _unitDeployedEventPool;
        private EcsPool<PowerLineUnitSpawnedEvent> _powerLineUnitSpawnedEventPool;
        private EcsPool<PowerLinePlugStateChangedEvent> _powerLinePlugStateChangedEventPool;

        public DeployPowerLineBattleCardSystem(
            BattleRuntimeService battleRuntimeService,
            UnitConfigService unitConfigService,
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _unitConfigService = unitConfigService;
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
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _drawHandRequestPool = world.GetPool<DrawPowerLineHandCardsRequest>();
            _unitDeployedEventPool = world.GetPool<UnitDeployedEvent>();
            _powerLineUnitSpawnedEventPool = world.GetPool<PowerLineUnitSpawnedEvent>();
            _powerLinePlugStateChangedEventPool = world.GetPool<PowerLinePlugStateChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                    !_levelTypePool.Has(runEntity) ||
                    _levelTypePool.Get(runEntity).Value != Enums.LevelType.PowerLineBattle ||
                    !_phasePool.Has(runEntity) ||
                    _phasePool.Get(runEntity).Value != Enums.PhaseType.Battle ||
                    !_handStatePool.Has(runEntity) ||
                    _battleRuntimeService.CurrentPowerLineState == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var lane = (Enums.PowerLineLane)request.TargetLaneIndex;
                var laneState = PowerLineBattleUtility.GetLane(_battleRuntimeService.CurrentPowerLineState, lane);
                if (laneState == null || laneState.IsConnected)
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
                _battleRuntimeService.CurrentPowerLineState.CurrentMana = currentMana.Value;
                _manaChangedEventPool.Add(world.NewEntity()).Value = currentMana.Value;

                var unitType = _unitConfigService.GetUnit(_unitTypeIdPool.Get(ownedUnitEntity).Value);
                var unit = PowerLineBattleUtility.CreatePlayerUnit(
                    _battleRuntimeService.CurrentPowerLineState.NextRuntimeId++,
                    ownedUnitRuntimeId,
                    _displayNamePool.Get(ownedUnitEntity).Value,
                    _unitStatsPool.Get(ownedUnitEntity).Attack,
                    _unitStatsPool.Get(ownedUnitEntity).Health,
                    manaCost,
                    _unitCombatStatsPool.Get(ownedUnitEntity).MoveSpeed,
                    _unitCombatStatsPool.Get(ownedUnitEntity).AttackRange,
                    _unitCombatStatsPool.Get(ownedUnitEntity).AttackSpeed,
                    lane,
                    unitType != null ? unitType.PortraitSprite : null,
                    unitType != null ? unitType.BattleAnimations : null);

                _battleRuntimeService.CurrentPowerLineState.PlayerUnits.Add(unit);
                _powerLineUnitSpawnedEventPool.Add(world.NewEntity()) = new PowerLineUnitSpawnedEvent
                {
                    RuntimeId = unit.RuntimeId,
                    IsEnemy = false,
                    Lane = lane,
                    Position = unit.Position
                };

                if (TryPickupPlug(_battleRuntimeService.CurrentPowerLineState, laneState, unit))
                {
                    _powerLinePlugStateChangedEventPool.Add(world.NewEntity()) = new PowerLinePlugStateChangedEvent
                    {
                        Lane = lane,
                        Status = laneState.Plug.Status,
                        Position = laneState.Plug.Position,
                        CarrierRuntimeId = laneState.Plug.CarrierRuntimeId
                    };
                }

                world.DelEntity(handCardEntity);
                ref var handState = ref _handStatePool.Get(runEntity);
                handState.CardCount = handState.CardCount > 0 ? handState.CardCount - 1 : 0;

                ref var drawRequest = ref _drawHandRequestPool.Add(world.NewEntity());
                drawRequest.Count = 1;
                drawRequest.ClearExisting = false;
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

        private static bool TryPickupPlug(PowerLineBattleStateModel state, PowerLineLaneStateModel laneState, PowerLineUnitStateModel unit)
        {
            if (laneState == null || unit == null || unit.IsEnemy || laneState.IsConnected)
            {
                return false;
            }

            if (laneState.Plug.Status == PowerLinePlugStatus.AtSpawn ||
                laneState.Plug.Status == PowerLinePlugStatus.Dropped)
            {
                if (unit.Position < laneState.Plug.Position)
                {
                    return false;
                }

                unit.IsCarryingPlug = true;
                laneState.Plug.Status = PowerLinePlugStatus.Carried;
                laneState.Plug.CarrierRuntimeId = unit.RuntimeId;
                laneState.Plug.Position = unit.Position;
                return true;
            }

            return false;
        }
    }
}
