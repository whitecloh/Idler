using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshPowerLineBattleHudUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly StatTypeConfigService _statTypeConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _currentLevelPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitCombatStatsComponent> _unitCombatStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;

        private EcsFilter _handFilter;
        private EcsFilter _ownedFilter;
        private EcsFilter _unitSpawnedFilter;
        private EcsFilter _attackFilter;
        private EcsFilter _damageFilter;
        private EcsFilter _plugChangedFilter;
        private EcsFilter _laneConnectedFilter;
        private EcsPool<PowerLineUnitSpawnedEvent> _unitSpawnedEventPool;
        private EcsPool<PowerLineAttackEvent> _attackEventPool;
        private EcsPool<PowerLineDamageEvent> _damageEventPool;
        private EcsPool<PowerLinePlugStateChangedEvent> _plugChangedEventPool;
        private EcsPool<PowerLineLaneConnectedEvent> _laneConnectedEventPool;

        public RefreshPowerLineBattleHudUiSystem(
            UnitConfigService unitConfigService,
            StatTypeConfigService statTypeConfigService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _unitConfigService = unitConfigService;
            _statTypeConfigService = statTypeConfigService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _currentLevelPool = world.GetPool<CurrentLevelComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _unitCombatStatsPool = world.GetPool<UnitCombatStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _handFilter = world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().End();
            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
            _unitSpawnedFilter = world.Filter<PowerLineUnitSpawnedEvent>().End();
            _attackFilter = world.Filter<PowerLineAttackEvent>().End();
            _damageFilter = world.Filter<PowerLineDamageEvent>().End();
            _plugChangedFilter = world.Filter<PowerLinePlugStateChangedEvent>().End();
            _laneConnectedFilter = world.Filter<PowerLineLaneConnectedEvent>().End();
            _unitSpawnedEventPool = world.GetPool<PowerLineUnitSpawnedEvent>();
            _attackEventPool = world.GetPool<PowerLineAttackEvent>();
            _damageEventPool = world.GetPool<PowerLineDamageEvent>();
            _plugChangedEventPool = world.GetPool<PowerLinePlugStateChangedEvent>();
            _laneConnectedEventPool = world.GetPool<PowerLineLaneConnectedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity) ||
                !_levelTypePool.Has(runEntity) ||
                _levelTypePool.Get(runEntity).Value != Enums.LevelType.PowerLineBattle)
            {
                _uiCompositionRoot.RefreshPowerLineBattleHud(new PowerLineBattleHudViewData());
                return;
            }

            var phase = _phasePool.Get(runEntity).Value;
            if (phase != Enums.PhaseType.Battle)
            {
                _uiCompositionRoot.RefreshPowerLineBattleHud(new PowerLineBattleHudViewData());
                return;
            }

            var state = _battleRuntimeService.CurrentPowerLineState;
            if (state == null)
            {
                _uiCompositionRoot.RefreshPowerLineBattleHud(new PowerLineBattleHudViewData());
                return;
            }

            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _currentLevelPool.Get(runEntity).LevelIndex;
            var locationData = _locationConfigService.GetLocation(locationId);
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var playerBase = _playerBasePool.Get(runEntity);
            var viewData = new PowerLineBattleHudViewData
            {
                LevelKey = $"{locationId}:{levelIndex}",
                LevelTitle = !string.IsNullOrWhiteSpace(levelData?.DisplayName) ? levelData.DisplayName : levelData != null ? levelData.Id : string.Empty,
                LocationDisplayName = locationData != null && !string.IsNullOrWhiteSpace(locationData.DisplayName) ? locationData.DisplayName : locationId,
                Phase = phase,
                CurrentMana = _manaPool.Get(runEntity).Value,
                MaxMana = state.MaxMana,
                RemainingDeckCount = state.DeckOwnedUnitRuntimeIds != null ? state.DeckOwnedUnitRuntimeIds.Count : 0,
                RerollManaCost = state.RerollManaCost,
                CanReroll = _manaPool.Get(runEntity).Value >= state.RerollManaCost,
                IsInteractionLocked = state.IsPendingVictorySequence,
                IsVictorySequencePending = state.IsPendingVictorySequence,
                BackgroundSprite = levelData != null ? levelData.BackgroundSprite : null,
                PlayerBase = new BattleBaseViewData
                {
                    Sprite = levelData != null ? levelData.PlayerBaseSprite : null,
                    CurrentHealth = playerBase.Value,
                    MaxHealth = playerBase.MaxValue
                },
                EnemyBaseSprite = levelData != null ? levelData.EnemyBaseSprite : null,
                ConnectedLaneCount = PowerLineBattleUtility.GetConnectedLaneCount(state),
                RequiredLaneCount = state.Lanes != null ? state.Lanes.Count : 0,
                Levels = BuildLevelProgress(locationData, levelIndex),
                HandCards = BuildHandCards(),
                DeckUnits = BuildDeckUnits(),
                Lanes = BuildLanes(state),
                PlayerUnits = BuildUnits(state.PlayerUnits, state.LaneLength),
                EnemyUnits = BuildUnits(state.EnemyUnits, state.LaneLength),
                UnitSpawnEvents = BuildUnitSpawnEvents(state.LaneLength),
                AttackEvents = BuildAttackEvents(state.LaneLength),
                DamageEvents = BuildDamageEvents(state.LaneLength),
                PlugEvents = BuildPlugEvents(state.LaneLength),
                LaneConnectedEvents = BuildLaneConnectedEvents()
            };

            _uiCompositionRoot.RefreshPowerLineBattleHud(viewData);
        }

        private List<HandCardViewData> BuildHandCards()
        {
            var handCards = new List<HandCardViewData>();
            foreach (var handEntity in _handFilter)
            {
                var ownerRuntimeId = _handCardOwnerPool.Get(handEntity).OwnedUnitRuntimeId;
                if (!TryFindOwnedEntity(ownerRuntimeId, out var ownedEntity))
                {
                    continue;
                }

                var unitType = _unitConfigService.GetUnit(_unitTypePool.Get(ownedEntity).Value);
                handCards.Add(new HandCardViewData
                {
                    HandCardRuntimeId = _handCardPool.Get(handEntity).HandCardRuntimeId,
                    OwnedUnitRuntimeId = ownerRuntimeId,
                    DisplayName = _displayNamePool.Get(ownedEntity).Value,
                    Level = _unitLevelPool.Get(ownedEntity).Value,
                    UnitTypeId = _unitTypePool.Get(ownedEntity).Value,
                    Attack = _statsPool.Get(ownedEntity).Attack,
                    Health = _statsPool.Get(ownedEntity).Health,
                    MaxHealth = _statsPool.Get(ownedEntity).Health,
                    ManaCost = _manaCostPool.Get(ownedEntity).Value,
                    MoveSpeed = _unitCombatStatsPool.Get(ownedEntity).MoveSpeed,
                    AttackRange = _unitCombatStatsPool.Get(ownedEntity).AttackRange,
                    AttackSpeed = _unitCombatStatsPool.Get(ownedEntity).AttackSpeed,
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    BattleAnimations = unitType != null ? unitType.BattleAnimations : null,
                    Stats = StatViewDataFactory.BuildUnitStats(
                        _statTypeConfigService,
                        unitType,
                        _statsPool.Get(ownedEntity).Attack,
                        _statsPool.Get(ownedEntity).Health,
                        _manaCostPool.Get(ownedEntity).Value,
                        _unitCombatStatsPool.Get(ownedEntity).MoveSpeed,
                        _unitCombatStatsPool.Get(ownedEntity).AttackRange,
                        _unitCombatStatsPool.Get(ownedEntity).AttackSpeed)
                });
            }

            handCards.Sort((left, right) => left.HandCardRuntimeId.CompareTo(right.HandCardRuntimeId));
            return handCards;
        }

        private List<BattleDeckUnitViewData> BuildDeckUnits()
        {
            var units = new List<BattleDeckUnitViewData>();
            var state = _battleRuntimeService.CurrentPowerLineState;
            if (state == null || state.InitialDeckOwnedUnitRuntimeIds == null || state.InitialDeckOwnedUnitRuntimeIds.Count <= 0)
            {
                return units;
            }

            var handOwnedRuntimeIds = new HashSet<int>();
            foreach (var handEntity in _handFilter)
            {
                handOwnedRuntimeIds.Add(_handCardOwnerPool.Get(handEntity).OwnedUnitRuntimeId);
            }

            var deckOwnedRuntimeIds = new HashSet<int>(state.DeckOwnedUnitRuntimeIds);
            foreach (var runtimeId in state.InitialDeckOwnedUnitRuntimeIds)
            {
                if (!TryFindOwnedEntity(runtimeId, out var ownedEntity))
                {
                    continue;
                }

                var unitType = _unitConfigService.GetUnit(_unitTypePool.Get(ownedEntity).Value);
                units.Add(new BattleDeckUnitViewData
                {
                    RuntimeId = runtimeId,
                    DisplayName = _displayNamePool.Get(ownedEntity).Value,
                    Attack = _statsPool.Get(ownedEntity).Attack,
                    Health = _statsPool.Get(ownedEntity).Health,
                    MaxHealth = _statsPool.Get(ownedEntity).Health,
                    ManaCost = _manaCostPool.Get(ownedEntity).Value,
                    MoveSpeed = _unitCombatStatsPool.Get(ownedEntity).MoveSpeed,
                    AttackRange = _unitCombatStatsPool.Get(ownedEntity).AttackRange,
                    AttackSpeed = _unitCombatStatsPool.Get(ownedEntity).AttackSpeed,
                    IsUsed = !deckOwnedRuntimeIds.Contains(runtimeId) && !handOwnedRuntimeIds.Contains(runtimeId),
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    BattleAnimations = unitType != null ? unitType.BattleAnimations : null,
                    Stats = StatViewDataFactory.BuildUnitStats(
                        _statTypeConfigService,
                        unitType,
                        _statsPool.Get(ownedEntity).Attack,
                        _statsPool.Get(ownedEntity).Health,
                        _manaCostPool.Get(ownedEntity).Value,
                        _unitCombatStatsPool.Get(ownedEntity).MoveSpeed,
                        _unitCombatStatsPool.Get(ownedEntity).AttackRange,
                        _unitCombatStatsPool.Get(ownedEntity).AttackSpeed)
                });
            }

            return units;
        }

        private static List<PowerLineLaneViewData> BuildLanes(PowerLineBattleStateModel state)
        {
            var lanes = new List<PowerLineLaneViewData>();
            if (state?.Lanes == null)
            {
                return lanes;
            }

            for (var index = 0; index < state.Lanes.Count; index++)
            {
                var lane = state.Lanes[index];
                lanes.Add(new PowerLineLaneViewData
                {
                    LaneIndex = (int)lane.Lane,
                    Lane = lane.Lane,
                    IsConnected = lane.IsConnected,
                    IsSpawnAvailable = !lane.IsConnected,
                    Plug = new PowerLinePlugViewData
                    {
                        Status = lane.Plug.Status,
                        NormalizedPosition = state.LaneLength > 0f ? lane.Plug.Position / state.LaneLength : 0f,
                        CarrierRuntimeId = lane.Plug.CarrierRuntimeId
                    }
                });
            }

            lanes.Sort((left, right) => left.LaneIndex.CompareTo(right.LaneIndex));
            return lanes;
        }

        private static List<PowerLineUnitViewData> BuildUnits(List<PowerLineUnitStateModel> units, float laneLength)
        {
            var result = new List<PowerLineUnitViewData>();
            if (units == null)
            {
                return result;
            }

            for (var index = 0; index < units.Count; index++)
            {
                var unit = units[index];
                result.Add(new PowerLineUnitViewData
                {
                    RuntimeId = unit.RuntimeId,
                    DisplayName = unit.DisplayName,
                    Attack = unit.Attack,
                    Health = unit.Health,
                    MaxHealth = unit.MaxHealth,
                    ManaCost = unit.ManaCost,
                    MoveSpeed = unit.MoveSpeed,
                    AttackRange = unit.AttackRange,
                    AttackSpeed = unit.AttackSpeed,
                    AttackType = unit.AttackType,
                    LaneIndex = (int)unit.Lane,
                    NormalizedPosition = laneLength > 0f ? unit.Position / laneLength : 0f,
                    IsEnemy = unit.IsEnemy,
                    IsCarryingPlug = unit.IsCarryingPlug,
                    PortraitSprite = unit.PortraitSprite,
                    ProjectileSprite = unit.ProjectileSprite,
                    BattleAnimations = unit.BattleAnimations
                });
            }

            result.Sort((left, right) =>
            {
                var laneCompare = left.LaneIndex.CompareTo(right.LaneIndex);
                return laneCompare != 0 ? laneCompare : left.NormalizedPosition.CompareTo(right.NormalizedPosition);
            });
            return result;
        }

        private List<PowerLineUnitSpawnedEventViewData> BuildUnitSpawnEvents(float laneLength)
        {
            var result = new List<PowerLineUnitSpawnedEventViewData>();
            foreach (var entity in _unitSpawnedFilter)
            {
                var evt = _unitSpawnedEventPool.Get(entity);
                result.Add(new PowerLineUnitSpawnedEventViewData
                {
                    RuntimeId = evt.RuntimeId,
                    IsEnemy = evt.IsEnemy,
                    LaneIndex = (int)evt.Lane,
                    NormalizedPosition = laneLength > 0f ? evt.Position / laneLength : 0f
                });
            }

            return result;
        }

        private List<PowerLineAttackEventViewData> BuildAttackEvents(float laneLength)
        {
            var result = new List<PowerLineAttackEventViewData>();
            foreach (var entity in _attackFilter)
            {
                var evt = _attackEventPool.Get(entity);
                result.Add(new PowerLineAttackEventViewData
                {
                    AttackerRuntimeId = evt.AttackerRuntimeId,
                    AttackerIsEnemy = evt.AttackerIsEnemy,
                    TargetIsBase = evt.TargetIsBase,
                    LaneIndex = (int)evt.Lane,
                    StartNormalizedPosition = laneLength > 0f ? evt.StartPosition / laneLength : 0f,
                    TargetNormalizedPosition = laneLength > 0f ? evt.TargetPosition / laneLength : 0f,
                    AttackType = evt.AttackType,
                    ProjectileSprite = evt.ProjectileSprite
                });
            }

            return result;
        }

        private List<PowerLineDamageEventViewData> BuildDamageEvents(float laneLength)
        {
            var result = new List<PowerLineDamageEventViewData>();
            foreach (var entity in _damageFilter)
            {
                var evt = _damageEventPool.Get(entity);
                result.Add(new PowerLineDamageEventViewData
                {
                    TargetRuntimeId = evt.TargetRuntimeId,
                    TargetIsEnemy = evt.TargetIsEnemy,
                    TargetIsBase = evt.TargetIsBase,
                    LaneIndex = (int)evt.Lane,
                    NormalizedPosition = laneLength > 0f ? evt.Position / laneLength : 0f,
                    Amount = evt.Amount
                });
            }

            return result;
        }

        private List<PowerLinePlugEventViewData> BuildPlugEvents(float laneLength)
        {
            var result = new List<PowerLinePlugEventViewData>();
            foreach (var entity in _plugChangedFilter)
            {
                var evt = _plugChangedEventPool.Get(entity);
                result.Add(new PowerLinePlugEventViewData
                {
                    LaneIndex = (int)evt.Lane,
                    Status = evt.Status,
                    NormalizedPosition = laneLength > 0f ? evt.Position / laneLength : 0f,
                    CarrierRuntimeId = evt.CarrierRuntimeId
                });
            }

            return result;
        }

        private List<PowerLineLaneConnectedEventViewData> BuildLaneConnectedEvents()
        {
            var result = new List<PowerLineLaneConnectedEventViewData>();
            foreach (var entity in _laneConnectedFilter)
            {
                var evt = _laneConnectedEventPool.Get(entity);
                result.Add(new PowerLineLaneConnectedEventViewData
                {
                    LaneIndex = (int)evt.Lane
                });
            }

            return result;
        }

        private bool TryFindOwnedEntity(int runtimeId, out int ownedEntity)
        {
            foreach (var candidateEntity in _ownedFilter)
            {
                if (_ownedUnitPool.Get(candidateEntity).RuntimeId == runtimeId)
                {
                    ownedEntity = candidateEntity;
                    return true;
                }
            }

            ownedEntity = -1;
            return false;
        }

        private static List<PurchaseLevelProgressEntryViewData> BuildLevelProgress(LocationData locationData, int currentLevelIndex)
        {
            var result = new List<PurchaseLevelProgressEntryViewData>();
            if (locationData == null || locationData.Levels == null)
            {
                return result;
            }

            for (var index = 0; index < locationData.Levels.Count; index++)
            {
                var level = locationData.Levels[index];
                result.Add(new PurchaseLevelProgressEntryViewData
                {
                    LevelIndex = index,
                    DisplayNumber = index + 1,
                    LevelType = level != null ? level.LevelType : Enums.LevelType.None,
                    ProgressSprite = level != null ? level.ProgressSprite : null,
                    IsCompleted = index < currentLevelIndex,
                    IsCurrent = index == currentLevelIndex,
                    IsUnlocked = index <= currentLevelIndex
                });
            }

            return result;
        }
    }
}
