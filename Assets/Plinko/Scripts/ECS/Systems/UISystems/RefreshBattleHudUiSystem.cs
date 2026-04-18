using System.Collections.Generic;
using System.Text;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshBattleHudUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly UnitConfigService _unitConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly EnemyWaveSelectionService _enemyWaveSelectionService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _currentLevelPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<DeployedForTurnComponent> _deployedPool;
        private EcsPool<DeploymentOrderComponent> _deploymentOrderPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;

        private EcsFilter _handFilter;
        private EcsFilter _deployedFilter;
        private EcsFilter _ownedFilter;

        public RefreshBattleHudUiSystem(
            GameSettingsService gameSettingsService,
            UnitConfigService unitConfigService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            EnemyWaveSelectionService enemyWaveSelectionService,
            BattleRuntimeService battleRuntimeService,
            OwnedUnitIndex ownedUnitIndex,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _gameSettingsService = gameSettingsService;
            _unitConfigService = unitConfigService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _enemyWaveSelectionService = enemyWaveSelectionService;
            _battleRuntimeService = battleRuntimeService;
            _ownedUnitIndex = ownedUnitIndex;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _currentLevelPool = world.GetPool<CurrentLevelComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _deployedPool = world.GetPool<DeployedForTurnComponent>();
            _deploymentOrderPool = world.GetPool<DeploymentOrderComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _handFilter = world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().End();
            _deployedFilter = world.Filter<DeployedForTurnComponent>().Inc<HandCardOwnerUnitComponent>().Inc<DeploymentOrderComponent>().End();
            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                !_phasePool.Has(runEntity))
            {
                _uiCompositionRoot.RefreshBattleHud(new BattleHudViewData());
                return;
            }

            var phase = _phasePool.Get(runEntity).Value;
            var isBattleVisible = phase == Enums.PhaseType.BattlePreparation ||
                                  phase == Enums.PhaseType.Battle ||
                                  phase == Enums.PhaseType.BattlePlayback;
            if (!isBattleVisible)
            {
                _uiCompositionRoot.RefreshBattleHud(new BattleHudViewData());
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
            var enemyBase = _enemyBasePool.Get(runEntity);
            var previewWave = _battleRuntimeService.CurrentEnemyWave ?? _enemyWaveSelectionService.SelectWave(levelData, enemyBase.Value);
            var viewData = new BattleHudViewData
            {
                LevelKey = $"{locationId}:{levelIndex}",
                LocationDisplayName = locationData != null && !string.IsNullOrWhiteSpace(locationData.DisplayName) ? locationData.DisplayName : locationId,
                Phase = phase,
                CurrentMana = _manaPool.Get(runEntity).Value,
                MaxMana = _gameSettingsService.GetManaPerTurn(),
                PlayerBaseHealth = playerBase.Value,
                EnemyBaseHealth = enemyBase.Value,
                CurrentTurn = battleState.CurrentTurn,
                ActiveEnemyWaveDebug = BuildWaveDebug(previewWave),
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
                EnemyBase = new BattleBaseViewData
                {
                    Sprite = levelData != null ? levelData.EnemyBaseSprite : null,
                    CurrentHealth = enemyBase.Value,
                    MaxHealth = enemyBase.MaxValue
                },
                Levels = BuildLevelProgress(locationData, levelIndex)
            };

            viewData.HandCards = BuildHandCards();
            viewData.DeckUnits = BuildDeckUnits();
            viewData.PlayerUnits = BuildPlayerUnits();
            viewData.EnemyUnits = BuildEnemyUnits(previewWave);
            _uiCompositionRoot.RefreshBattleHud(viewData);
        }

        private List<HandCardViewData> BuildHandCards()
        {
            var handCards = new List<HandCardViewData>();
            foreach (var handEntity in _handFilter)
            {
                var ownerRuntimeId = _handCardOwnerPool.Get(handEntity).OwnedUnitRuntimeId;
                if (!_ownedUnitIndex.TryGet(ownerRuntimeId, out var ownedEntity))
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
                    IsDeployed = _deployedPool.Has(handEntity),
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    BattleAnimations = unitType != null ? unitType.BattleAnimations : null
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
                    PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                    BattleAnimations = unitType != null ? unitType.BattleAnimations : null
                });
            }

            units.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            return units;
        }

        private List<BattleBoardUnitViewData> BuildPlayerUnits()
        {
            var deployedUnits = new List<DeploymentEntry>();
            foreach (var deployedEntity in _deployedFilter)
            {
                var ownerRuntimeId = _handCardOwnerPool.Get(deployedEntity).OwnedUnitRuntimeId;
                if (!_ownedUnitIndex.TryGet(ownerRuntimeId, out var ownedEntity))
                {
                    continue;
                }

                var unitType = _unitConfigService.GetUnit(_unitTypePool.Get(ownedEntity).Value);

                deployedUnits.Add(new DeploymentEntry
                {
                    Order = _deploymentOrderPool.Get(deployedEntity).Value,
                    ViewData = new BattleBoardUnitViewData
                    {
                        RuntimeId = ownerRuntimeId,
                        DisplayName = _displayNamePool.Get(ownedEntity).Value,
                        Attack = _statsPool.Get(ownedEntity).Attack,
                        Health = _statsPool.Get(ownedEntity).Health,
                        ManaCost = _manaCostPool.Get(ownedEntity).Value,
                        PortraitSprite = unitType != null ? unitType.PortraitSprite : null,
                        BattleAnimations = unitType != null ? unitType.BattleAnimations : null,
                        IsEnemy = false
                    }
                });
            }

            deployedUnits.Sort((left, right) => right.Order.CompareTo(left.Order));
            var result = new List<BattleBoardUnitViewData>(deployedUnits.Count);
            for (var index = 0; index < deployedUnits.Count; index++)
            {
                deployedUnits[index].ViewData.BoardIndex = index;
                result.Add(deployedUnits[index].ViewData);
            }

            return result;
        }

        private List<BattleBoardUnitViewData> BuildEnemyUnits(EnemyWaveModel wave)
        {
            var units = new List<BattleBoardUnitViewData>();
            if (wave == null)
            {
                return units;
            }

            for (var index = 0; index < wave.Enemies.Count; index++)
            {
                var enemy = wave.Enemies[index];
                if (enemy == null)
                {
                    continue;
                }

                units.Add(new BattleBoardUnitViewData
                {
                    RuntimeId = -(index + 1),
                    DisplayName = enemy.DisplayName,
                    Attack = enemy.Attack,
                    Health = enemy.Health,
                    ManaCost = 0,
                    PortraitSprite = enemy.PortraitSprite,
                    BattleAnimations = enemy.BattleAnimations,
                    BoardIndex = enemy.BoardX,
                    IsEnemy = true
                });
            }

            units.Sort((left, right) => right.BoardIndex.CompareTo(left.BoardIndex));
            for (var index = 0; index < units.Count; index++)
            {
                units[index].BoardIndex = index;
            }

            return units;
        }

        private static string BuildWaveDebug(EnemyWaveModel wave)
        {
            if (wave == null)
            {
                return "Wave: pending";
            }

            var builder = new StringBuilder();
            builder.Append($"Wave {wave.ThresholdPercent}%  enemies {wave.Enemies.Count}  atk {wave.TotalAttack}  hp {wave.TotalHealth}");
            return builder.ToString();
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
                        ? "Deploy units, then start battle."
                        : "Preparing next turn.";
                case Enums.PhaseType.Battle:
                    return "Battle resolving.";
                case Enums.PhaseType.BattlePlayback:
                    return "Battle playback.";
                default:
                    return string.Empty;
            }
        }

        private sealed class DeploymentEntry
        {
            public int Order;
            public BattleBoardUnitViewData ViewData;
        }
    }
}
