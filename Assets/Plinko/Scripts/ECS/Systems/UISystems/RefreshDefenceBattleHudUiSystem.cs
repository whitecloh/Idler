using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshDefenceBattleHudUiSystem : IEcsInitSystem, IEcsRunSystem
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
        private EcsPool<BattleStateComponent> _battleStatePool;
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

        public RefreshDefenceBattleHudUiSystem(
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
            _battleStatePool = world.GetPool<BattleStateComponent>();
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
                _levelTypePool.Get(runEntity).Value != Enums.LevelType.DefenceBattle)
            {
                _uiCompositionRoot.RefreshDefenceBattleHud(new DefenceBattleHudViewData());
                return;
            }

            var phase = _phasePool.Get(runEntity).Value;
            var isBattleVisible = phase == Enums.PhaseType.BattlePreparation ||
                                  phase == Enums.PhaseType.Battle ||
                                  phase == Enums.PhaseType.BattlePlayback;
            if (!isBattleVisible)
            {
                _uiCompositionRoot.RefreshDefenceBattleHud(new DefenceBattleHudViewData());
                return;
            }

            var battleState = _battleStatePool.Has(runEntity)
                ? _battleStatePool.Get(runEntity)
                : new BattleStateComponent();
            var locationId = _locationPool.Get(runEntity).LocationId;
            var levelIndex = _currentLevelPool.Get(runEntity).LevelIndex;
            var locationData = _locationConfigService.GetLocation(locationId);
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var playerBase = _playerBasePool.Get(runEntity);
            var baseDefenseState = _battleRuntimeService.CurrentBaseDefenseState;
            var viewData = new DefenceBattleHudViewData
            {
                LevelKey = $"{locationId}:{levelIndex}",
                LocationDisplayName = locationData != null && !string.IsNullOrWhiteSpace(locationData.DisplayName) ? locationData.DisplayName : locationId,
                Phase = phase,
                CurrentMana = _manaPool.Get(runEntity).Value,
                MaxMana = baseDefenseState != null ? baseDefenseState.CurrentManaCap : 0,
                CurrentTurn = battleState.CurrentTurn,
                BaseDefenseCompletedTurns = baseDefenseState != null ? baseDefenseState.CompletedTurnCount : 0,
                BaseDefenseRequiredTurns = baseDefenseState != null ? baseDefenseState.RequiredTurnCount : 0,
                BaseDefenseLaneCount = baseDefenseState != null ? baseDefenseState.LaneCount : 0,
                BaseDefenseCellsPerLane = baseDefenseState != null ? baseDefenseState.CellsPerLane : 0,
                BaseDefensePlayerSideCellCount = baseDefenseState != null ? baseDefenseState.PlayerSideCellCount : 0,
                ActiveEnemyWaveDebug = BuildWaveDebug(baseDefenseState, battleState.CurrentTurn),
                StatusText = BuildStatusText(phase, battleState),
                CanDeploy = phase == Enums.PhaseType.BattlePreparation &&
                            battleState.IsPlayerTurnActive &&
                            !battleState.IsResolved,
                CanStartBattle = phase == Enums.PhaseType.BattlePreparation &&
                                 battleState.IsPlayerTurnActive &&
                                 battleState.HasGeneratedHandThisTurn &&
                                 !battleState.IsResolved,
                IsBattleResolved = battleState.IsResolved,
                IsInteractionLocked = phase != Enums.PhaseType.BattlePreparation ||
                                      !battleState.IsPlayerTurnActive ||
                                      battleState.IsResolved,
                BackgroundSprite = levelData != null ? levelData.BackgroundSprite : null,
                PlayerBase = new BattleBaseViewData
                {
                    Sprite = levelData != null ? levelData.PlayerBaseSprite : null,
                    CurrentHealth = playerBase.Value,
                    MaxHealth = playerBase.MaxValue
                },
                Levels = BuildLevelProgress(locationData, levelIndex)
            };

            viewData.HandCards = BuildHandCards();
            viewData.DeckUnits = BuildDeckUnits();
            if (baseDefenseState != null)
            {
                viewData.PlayerUnits = BuildBoardUnits(baseDefenseState.PlayerUnits, false, baseDefenseState.CellsPerLane);
                viewData.EnemyUnits = BuildBoardUnits(baseDefenseState.EnemyUnits, true, baseDefenseState.CellsPerLane);
                viewData.NextWaveUnits = BuildPreviewUnits(baseDefenseState.PreviewWaveUnits, baseDefenseState.CellsPerLane);
            }

            _uiCompositionRoot.RefreshDefenceBattleHud(viewData);
        }

        private List<HandCardViewData> BuildHandCards()
        {
            var handCards = new List<HandCardViewData>();
            foreach (var handEntity in _handFilter)
            {
                var ownerRuntimeId = _handCardOwnerPool.Get(handEntity).OwnedUnitRuntimeId;
                if (!_runEntityIndex.TryGetRunEntity(out _))
                {
                    continue;
                }

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
            foreach (var ownedEntity in _ownedFilter)
            {
                var runtimeId = _ownedUnitPool.Get(ownedEntity).RuntimeId;
                var unitTypeId = _unitTypePool.Get(ownedEntity).Value;
                var unitType = _unitConfigService.GetUnit(unitTypeId);
                units.Add(new BattleDeckUnitViewData
                {
                    RuntimeId = runtimeId,
                    DisplayName = _displayNamePool.Get(ownedEntity).Value,
                    Attack = _statsPool.Get(ownedEntity).Attack,
                    Health = _statsPool.Get(ownedEntity).Health,
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

            units.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            return units;
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

        private static List<BattleBoardUnitViewData> BuildBoardUnits(
            List<BaseDefenseUnitStateModel> units,
            bool isEnemy,
            int cellsPerLane)
        {
            var result = new List<BattleBoardUnitViewData>();
            if (units == null)
            {
                return result;
            }

            foreach (var unit in units)
            {
                if (unit == null)
                {
                    continue;
                }

                result.Add(new BattleBoardUnitViewData
                {
                    RuntimeId = unit.RuntimeId,
                    DisplayName = unit.DisplayName,
                    Attack = unit.Attack,
                    Health = unit.Health,
                    ManaCost = unit.ManaCost,
                    BoardIndex = unit.LaneIndex * cellsPerLane + unit.CellIndex,
                    LaneIndex = unit.LaneIndex,
                    CellIndex = unit.CellIndex,
                    IsEnemy = isEnemy,
                    PortraitSprite = unit.PortraitSprite,
                    BattleAnimations = unit.BattleAnimations
                });
            }

            return result;
        }

        private static List<BattleBoardUnitViewData> BuildPreviewUnits(
            List<BaseDefenseWavePreviewUnitModel> previewUnits,
            int cellsPerLane)
        {
            var result = new List<BattleBoardUnitViewData>();
            if (previewUnits == null)
            {
                return result;
            }

            for (var index = 0; index < previewUnits.Count; index++)
            {
                var unit = previewUnits[index];
                if (unit == null)
                {
                    continue;
                }

                var cellIndex = cellsPerLane + unit.EnemySideCellIndex;
                result.Add(new BattleBoardUnitViewData
                {
                    RuntimeId = -(index + 1),
                    DisplayName = unit.DisplayName,
                    Attack = unit.Attack,
                    Health = unit.Health,
                    ManaCost = 0,
                    BoardIndex = unit.LaneIndex * cellsPerLane + cellIndex,
                    LaneIndex = unit.LaneIndex,
                    CellIndex = cellIndex,
                    IsEnemy = true,
                    IsPreview = true,
                    PortraitSprite = unit.PortraitSprite,
                    BattleAnimations = unit.BattleAnimations
                });
            }

            return result;
        }

        private static string BuildWaveDebug(BaseDefenseBattleStateModel state, int currentTurn)
        {
            if (state == null)
            {
                return "Wave: pending";
            }

            return $"Wave {currentTurn}/{System.Math.Max(1, state.RequiredTurnCount)}  next {state.PreviewWaveUnits.Count}  progress {state.CompletedTurnCount}/{state.RequiredTurnCount}";
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

        private static string BuildStatusText(Enums.PhaseType phase, BattleStateComponent battleState)
        {
            switch (phase)
            {
                case Enums.PhaseType.BattlePreparation:
                    return battleState.IsPlayerTurnActive
                        ? "Select cells, then start battle."
                        : "Preparing next turn.";
                case Enums.PhaseType.Battle:
                    return "Battle resolving.";
                case Enums.PhaseType.BattlePlayback:
                    return "Battle playback.";
                default:
                    return string.Empty;
            }
        }
    }
}
