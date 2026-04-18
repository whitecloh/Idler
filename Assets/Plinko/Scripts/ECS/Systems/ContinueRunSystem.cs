using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ContinueRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly ShopOfferIndex _shopOfferIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _requestFilter;
        private EcsPool<ContinueRunRequest> _requestPool;
        private EcsPool<RunComponent> _runPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<BattleStateComponent> _battlePool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<DeployedForTurnComponent> _deployedPool;
        private EcsPool<DeploymentOrderComponent> _deploymentOrderPool;
        private EcsPool<RestoreOwnedUnitsRequest> _restoreOwnedUnitsRequestPool;
        private EcsPool<RestoreBoardStateRequest> _restoreBoardRequestPool;
        private EcsPool<GenerateHandRequest> _generateHandRequestPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<RunStartedEvent> _runStartedEventPool;
        private EcsPool<StartLevelRequest> _startLevelRequestPool;

        public ContinueRunSystem(
            RunSaveService runSaveService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            GameSettingsService gameSettingsService,
            PlinkoRuntimeService plinkoRuntimeService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex,
            ShopOfferIndex shopOfferIndex,
            PinShopOfferIndex pinShopOfferIndex,
            InstalledPinIndex installedPinIndex)
        {
            _runSaveService = runSaveService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _gameSettingsService = gameSettingsService;
            _plinkoRuntimeService = plinkoRuntimeService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
            _shopOfferIndex = shopOfferIndex;
            _pinShopOfferIndex = pinShopOfferIndex;
            _installedPinIndex = installedPinIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ContinueRunRequest>().End();
            _requestPool = world.GetPool<ContinueRunRequest>();
            _runPool = world.GetPool<RunComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _purchasePool = world.GetPool<PurchasePhaseStateComponent>();
            _retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _battlePool = world.GetPool<BattleStateComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _deployedPool = world.GetPool<DeployedForTurnComponent>();
            _deploymentOrderPool = world.GetPool<DeploymentOrderComponent>();
            _restoreOwnedUnitsRequestPool = world.GetPool<RestoreOwnedUnitsRequest>();
            _restoreBoardRequestPool = world.GetPool<RestoreBoardStateRequest>();
            _generateHandRequestPool = world.GetPool<GenerateHandRequest>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _runStartedEventPool = world.GetPool<RunStartedEvent>();
            _startLevelRequestPool = world.GetPool<StartLevelRequest>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                _requestPool.Get(requestEntity);
                var dto = _runSaveService.Load();
                if (dto == null || !dto.HasActiveRun || string.IsNullOrWhiteSpace(dto.LocationId))
                {
                    _runSaveService.Clear();
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (dto.RunStatus != Enums.RunStatus.InProgress)
                {
                    _runSaveService.Clear();
                    world.DelEntity(requestEntity);
                    continue;
                }

                var location = _locationConfigService.GetLocation(dto.LocationId);
                if (location == null)
                {
                    _runSaveService.Clear();
                    world.DelEntity(requestEntity);
                    continue;
                }

                var levelData = _levelConfigService.GetLevel(dto.LocationId, dto.LevelIndex);
                if (levelData == null || !IsSaveNumericallyValid(dto))
                {
                    RestartLocationFromCorruptedSave(world, dto.LocationId);
                    world.DelEntity(requestEntity);
                    continue;
                }

                var normalizedPhase = NormalizePhase(levelData.LevelType, dto.PhaseType);
                if (normalizedPhase == Enums.PhaseType.None)
                {
                    RestartLocationFromCorruptedSave(world, dto.LocationId);
                    world.DelEntity(requestEntity);
                    continue;
                }

                var ownedUnits = dto.OwnedUnits ?? new List<OwnedUnitSaveDto>();
                if (!AreOwnedUnitsValid(ownedUnits))
                {
                    RestartLocationFromCorruptedSave(world, dto.LocationId);
                    world.DelEntity(requestEntity);
                    continue;
                }

                RuntimeEntityCleanup.ClearForNewRun(world, _runEntityIndex, _ownedUnitIndex, _shopOfferIndex, _pinShopOfferIndex, _installedPinIndex);
                _plinkoRuntimeService.Clear();
                _battleRuntimeService.Clear();
                var battleRestore = BuildBattleRestoreState(dto, levelData, ownedUnits, normalizedPhase);
                var retrainingOfferCount = levelData != null && levelData.PreBattlePhase != null && levelData.PreBattlePhase.OverrideRetrainingOfferCount > 0
                    ? levelData.PreBattlePhase.OverrideRetrainingOfferCount
                    : _gameSettingsService.GetDefaultRetrainingOfferCount();
                var enemyBaseMaxHealth = levelData.EnemyBaseMaxHealth > 0 ? levelData.EnemyBaseMaxHealth : dto.EnemyBaseHealth;

                var runEntity = world.NewEntity();
                _runPool.Add(runEntity);
                _locationPool.Add(runEntity).LocationId = dto.LocationId;
                _levelPool.Add(runEntity).LevelIndex = dto.LevelIndex;
                _levelTypePool.Add(runEntity).Value = levelData.LevelType;
                _phasePool.Add(runEntity).Value = normalizedPhase;
                _goldPool.Add(runEntity).Value = Mathf.Max(0, dto.Gold);
                _playerBasePool.Add(runEntity) = new PlayerBaseHealthComponent
                {
                    Value = Mathf.Clamp(dto.PlayerBaseHealth, 0, _gameSettingsService.GetStartingBaseHealth()),
                    MaxValue = _gameSettingsService.GetStartingBaseHealth()
                };
                _enemyBasePool.Add(runEntity) = new EnemyBaseHealthComponent
                {
                    Value = Mathf.Clamp(dto.EnemyBaseHealth, 0, enemyBaseMaxHealth),
                    MaxValue = enemyBaseMaxHealth
                };
                _statusPool.Add(runEntity).Value = Enums.RunStatus.InProgress;
                _manaPool.Add(runEntity).Value = battleRestore.CurrentMana;
                _handStatePool.Add(runEntity) = new HandStateComponent
                {
                    CardCount = battleRestore.HandCards.Count,
                    NextRuntimeId = battleRestore.NextHandRuntimeId
                };
                _purchasePool.Add(runEntity) = new PurchasePhaseStateComponent { RerollCount = Mathf.Max(0, dto.PurchaseRerollCount), ActiveTrainingCount = 0, CanEnterBattle = true };
                _retrainingPool.Add(runEntity) = new RetrainingPhaseStateComponent
                {
                    OfferCount = retrainingOfferCount,
                    RerollCount = 0,
                    ActiveTrainingCount = 0
                };
                _fieldUpgradePool.Add(runEntity) = new FieldUpgradePhaseStateComponent { RerollCount = Mathf.Max(0, dto.PinRerollCount), SelectedSlotIndex = -1, IsPlacementHighlighted = false };
                _battlePool.Add(runEntity) = new BattleStateComponent
                {
                    CurrentTurn = battleRestore.CurrentTurn,
                    IsResolved = false,
                    NextDeploymentOrder = battleRestore.NextDeploymentOrder,
                    IsPlayerTurnActive = battleRestore.IsPlayerTurnActive,
                    HasGeneratedHandThisTurn = battleRestore.HasGeneratedHandThisTurn,
                    TotalEnemyKills = battleRestore.TotalEnemyKills,
                    TotalDamageToEnemyBase = battleRestore.TotalDamageToEnemyBase,
                    TotalDamageToPlayerBase = battleRestore.TotalDamageToPlayerBase
                };

                _runEntityIndex.SetRunEntity(runEntity);
                _battleRuntimeService.CurrentResult = CloneBattleResult(battleRestore.RestoredResult);

                _restoreOwnedUnitsRequestPool.Add(world.NewEntity()).OwnedUnits = ownedUnits;
                _restoreBoardRequestPool.Add(world.NewEntity()).Board = dto.Board ?? new PlinkoBoardSaveDto();
                RestoreHandCards(world, battleRestore.HandCards);
                RestoreDeployedUnits(world, battleRestore.DeployedUnits);
                _goldChangedEventPool.Add(world.NewEntity()).Value = _goldPool.Get(runEntity).Value;
                _manaChangedEventPool.Add(world.NewEntity()).Value = _manaPool.Get(runEntity).Value;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = normalizedPhase;
                if (battleRestore.ShouldGenerateHand)
                {
                    _generateHandRequestPool.Add(world.NewEntity());
                }
                world.DelEntity(requestEntity);
            }
        }

        private void RestartLocationFromCorruptedSave(EcsWorld world, string locationId)
        {
            _runSaveService.Clear();
            var location = _locationConfigService.GetLocation(locationId);
            if (location == null)
            {
                return;
            }

            RuntimeEntityCleanup.ClearForNewRun(world, _runEntityIndex, _ownedUnitIndex, _shopOfferIndex, _pinShopOfferIndex, _installedPinIndex);
            _plinkoRuntimeService.Clear();
            _battleRuntimeService.Clear();

            var runEntity = world.NewEntity();
            _runPool.Add(runEntity);
            _locationPool.Add(runEntity).LocationId = locationId;
            _levelPool.Add(runEntity).LevelIndex = 0;
            _levelTypePool.Add(runEntity).Value = Enums.LevelType.None;
            _phasePool.Add(runEntity).Value = Enums.PhaseType.Location;
            _goldPool.Add(runEntity).Value = _gameSettingsService.GetStartingGold();
            _playerBasePool.Add(runEntity) = new PlayerBaseHealthComponent
            {
                Value = _gameSettingsService.GetStartingBaseHealth(),
                MaxValue = _gameSettingsService.GetStartingBaseHealth()
            };
            _enemyBasePool.Add(runEntity) = new EnemyBaseHealthComponent { Value = 0, MaxValue = 0 };
            _statusPool.Add(runEntity).Value = Enums.RunStatus.InProgress;
            _manaPool.Add(runEntity).Value = _gameSettingsService.GetManaPerTurn();
            _handStatePool.Add(runEntity) = new HandStateComponent { CardCount = 0, NextRuntimeId = 1 };
            _purchasePool.Add(runEntity) = new PurchasePhaseStateComponent { RerollCount = 0, ActiveTrainingCount = 0, CanEnterBattle = false };
            _retrainingPool.Add(runEntity) = new RetrainingPhaseStateComponent
            {
                OfferCount = _gameSettingsService.GetDefaultRetrainingOfferCount(),
                RerollCount = 0,
                ActiveTrainingCount = 0
            };
            _fieldUpgradePool.Add(runEntity) = new FieldUpgradePhaseStateComponent { RerollCount = 0, SelectedSlotIndex = -1, IsPlacementHighlighted = false };
            _battlePool.Add(runEntity) = new BattleStateComponent
            {
                CurrentTurn = 0,
                IsResolved = false,
                NextDeploymentOrder = 0,
                IsPlayerTurnActive = false,
                HasGeneratedHandThisTurn = false,
                TotalEnemyKills = 0,
                TotalDamageToEnemyBase = 0,
                TotalDamageToPlayerBase = 0
            };

            _runEntityIndex.SetRunEntity(runEntity);

            _runStartedEventPool.Add(world.NewEntity());
            _goldChangedEventPool.Add(world.NewEntity()).Value = _goldPool.Get(runEntity).Value;
            _startLevelRequestPool.Add(world.NewEntity()).LevelIndex = 0;
        }

        private BattleRestoreState BuildBattleRestoreState(
            RunSaveDto dto,
            LevelData levelData,
            IReadOnlyList<OwnedUnitSaveDto> ownedUnits,
            Enums.PhaseType normalizedPhase)
        {
            var manaPerTurn = _gameSettingsService.GetManaPerTurn();
            var restoreState = new BattleRestoreState
            {
                CurrentTurn = levelData.LevelType == Enums.LevelType.Battle ? Mathf.Max(1, dto.BattleTurn) : 0,
                CurrentMana = Mathf.Clamp(dto.CurrentMana > 0 ? dto.CurrentMana : manaPerTurn, 0, manaPerTurn),
                NextHandRuntimeId = Mathf.Max(1, dto.HandNextRuntimeId),
                NextDeploymentOrder = Mathf.Max(0, dto.NextDeploymentOrder),
                IsPlayerTurnActive = levelData.LevelType == Enums.LevelType.Battle && normalizedPhase == Enums.PhaseType.BattlePreparation,
                HasGeneratedHandThisTurn = false,
                ShouldGenerateHand = false,
                TotalEnemyKills = Mathf.Max(0, dto.BattleEnemyKillsTotal),
                TotalDamageToEnemyBase = Mathf.Max(0, dto.BattleDamageToEnemyBaseTotal),
                TotalDamageToPlayerBase = Mathf.Max(0, dto.BattleDamageToPlayerBaseTotal)
            };

            if (levelData.LevelType != Enums.LevelType.Battle || normalizedPhase != Enums.PhaseType.BattlePreparation)
            {
                if (normalizedPhase == Enums.PhaseType.Result)
                {
                    restoreState.RestoredResult = BuildSavedOrFallbackBattleResult(dto, restoreState);
                }

                return restoreState;
            }

            if (dto.PhaseType == Enums.PhaseType.Battle || dto.PhaseType == Enums.PhaseType.BattlePlayback)
            {
                restoreState.CurrentMana = manaPerTurn;
                restoreState.NextDeploymentOrder = 0;
                restoreState.ShouldGenerateHand = true;
                return restoreState;
            }

            var validOwnedRuntimeIds = new HashSet<int>();
            foreach (var ownedUnit in ownedUnits)
            {
                validOwnedRuntimeIds.Add(ownedUnit.RuntimeId);
            }

            var hasInvalidTurnState = false;
            var maxHandCardRuntimeId = 0;
            if (dto.HandCards != null)
            {
                var uniqueCardIds = new HashSet<int>();
                foreach (var handCard in dto.HandCards)
                {
                    if (handCard == null ||
                        handCard.HandCardRuntimeId <= 0 ||
                        !uniqueCardIds.Add(handCard.HandCardRuntimeId) ||
                        !validOwnedRuntimeIds.Contains(handCard.OwnedUnitRuntimeId))
                    {
                        hasInvalidTurnState = true;
                        break;
                    }

                    restoreState.HandCards.Add(handCard);
                    maxHandCardRuntimeId = Mathf.Max(maxHandCardRuntimeId, handCard.HandCardRuntimeId);
                }
            }

            if (!hasInvalidTurnState && dto.DeployedUnits != null)
            {
                var uniqueDeploymentOrders = new HashSet<int>();
                foreach (var deployedUnit in dto.DeployedUnits)
                {
                    if (deployedUnit == null ||
                        deployedUnit.DeploymentOrder < 0 ||
                        !uniqueDeploymentOrders.Add(deployedUnit.DeploymentOrder) ||
                        !validOwnedRuntimeIds.Contains(deployedUnit.OwnedUnitRuntimeId))
                    {
                        hasInvalidTurnState = true;
                        break;
                    }

                    restoreState.DeployedUnits.Add(deployedUnit);
                    restoreState.NextDeploymentOrder = Mathf.Max(restoreState.NextDeploymentOrder, deployedUnit.DeploymentOrder + 1);
                }
            }

            if (hasInvalidTurnState)
            {
                restoreState.HandCards.Clear();
                restoreState.DeployedUnits.Clear();
                restoreState.CurrentMana = manaPerTurn;
                restoreState.NextDeploymentOrder = 0;
                restoreState.ShouldGenerateHand = true;
                return restoreState;
            }

            restoreState.NextHandRuntimeId = Mathf.Max(restoreState.NextHandRuntimeId, maxHandCardRuntimeId + 1);
            restoreState.HasGeneratedHandThisTurn = true;

            var expectsCards = ownedUnits.Count > 0 && _gameSettingsService.GetHandSize() > 0;
            if (expectsCards && restoreState.HandCards.Count == 0 && restoreState.DeployedUnits.Count == 0)
            {
                restoreState.HasGeneratedHandThisTurn = false;
                restoreState.CurrentMana = manaPerTurn;
                restoreState.ShouldGenerateHand = true;
            }

            return restoreState;
        }

        private void RestoreHandCards(EcsWorld world, IReadOnlyList<HandCardSaveDto> handCards)
        {
            if (handCards == null)
            {
                return;
            }

            foreach (var handCard in handCards)
            {
                if (handCard == null)
                {
                    continue;
                }

                var entity = world.NewEntity();
                _handCardPool.Add(entity).HandCardRuntimeId = handCard.HandCardRuntimeId;
                _handCardOwnerPool.Add(entity).OwnedUnitRuntimeId = handCard.OwnedUnitRuntimeId;
            }
        }

        private void RestoreDeployedUnits(EcsWorld world, IReadOnlyList<DeployedUnitSaveDto> deployedUnits)
        {
            if (deployedUnits == null)
            {
                return;
            }

            foreach (var deployedUnit in deployedUnits)
            {
                if (deployedUnit == null)
                {
                    continue;
                }

                var entity = world.NewEntity();
                _handCardOwnerPool.Add(entity).OwnedUnitRuntimeId = deployedUnit.OwnedUnitRuntimeId;
                _deployedPool.Add(entity);
                _deploymentOrderPool.Add(entity).Value = deployedUnit.DeploymentOrder;
            }
        }

        private static BattleResultModel BuildSavedOrFallbackBattleResult(RunSaveDto dto, BattleRestoreState restoreState)
        {
            var savedResult = CloneBattleResult(dto.BattleResult);
            if (savedResult != null)
            {
                return savedResult;
            }

            return new BattleResultModel
            {
                PlayerBaseHealthBefore = dto.PlayerBaseHealth + restoreState.TotalDamageToPlayerBase,
                PlayerBaseHealthAfter = dto.PlayerBaseHealth,
                EnemyBaseHealthBefore = dto.EnemyBaseHealth + restoreState.TotalDamageToEnemyBase,
                EnemyBaseHealthAfter = dto.EnemyBaseHealth,
                EnemyKillsThisTurn = 0,
                EnemyKillsTotal = restoreState.TotalEnemyKills,
                DamageToEnemyBaseThisTurn = 0,
                DamageToEnemyBaseTotal = restoreState.TotalDamageToEnemyBase,
                DamageToPlayerBaseThisTurn = 0,
                DamageToPlayerBaseTotal = restoreState.TotalDamageToPlayerBase,
                TurnsSpent = Mathf.Max(1, restoreState.CurrentTurn),
                BaseReward = 0,
                RewardGranted = 0,
                IsVictory = dto.RunStatus == Enums.RunStatus.Victory || dto.RunStatus == Enums.RunStatus.InProgress,
                IsDefeat = dto.RunStatus == Enums.RunStatus.Defeat
            };
        }

        private static BattleResultModel CloneBattleResult(BattleResultModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new BattleResultModel
            {
                PlayerBaseHealthBefore = source.PlayerBaseHealthBefore,
                PlayerBaseHealthAfter = source.PlayerBaseHealthAfter,
                EnemyBaseHealthBefore = source.EnemyBaseHealthBefore,
                EnemyBaseHealthAfter = source.EnemyBaseHealthAfter,
                EnemyKillsThisTurn = source.EnemyKillsThisTurn,
                EnemyKillsTotal = source.EnemyKillsTotal,
                DamageToEnemyBaseThisTurn = source.DamageToEnemyBaseThisTurn,
                DamageToEnemyBaseTotal = source.DamageToEnemyBaseTotal,
                DamageToPlayerBaseThisTurn = source.DamageToPlayerBaseThisTurn,
                DamageToPlayerBaseTotal = source.DamageToPlayerBaseTotal,
                TurnsSpent = source.TurnsSpent,
                BaseReward = source.BaseReward,
                RewardGranted = source.RewardGranted,
                IsVictory = source.IsVictory,
                IsDefeat = source.IsDefeat
            };
        }

        private bool AreOwnedUnitsValid(IReadOnlyList<OwnedUnitSaveDto> ownedUnits)
        {
            var uniqueRuntimeIds = new HashSet<int>();
            foreach (var ownedUnit in ownedUnits)
            {
                if (ownedUnit == null || ownedUnit.RuntimeId <= 0 || !uniqueRuntimeIds.Add(ownedUnit.RuntimeId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSaveNumericallyValid(RunSaveDto dto)
        {
            return dto.LevelIndex >= 0 &&
                   dto.Gold >= 0 &&
                   dto.CurrentMana >= 0 &&
                   dto.PlayerBaseHealth >= 0 &&
                   dto.EnemyBaseHealth >= 0 &&
                   dto.BattleEnemyKillsTotal >= 0 &&
                   dto.BattleDamageToEnemyBaseTotal >= 0 &&
                   dto.BattleDamageToPlayerBaseTotal >= 0 &&
                   dto.PurchaseRerollCount >= 0 &&
                   dto.PinRerollCount >= 0 &&
                   dto.HandNextRuntimeId >= 0 &&
                   dto.NextDeploymentOrder >= 0;
        }

        private static Enums.PhaseType NormalizePhase(Enums.LevelType levelType, Enums.PhaseType savedPhase)
        {
            switch (levelType)
            {
                case Enums.LevelType.Purchase:
                    return savedPhase == Enums.PhaseType.PurchasePhase || savedPhase == Enums.PhaseType.Result
                        ? savedPhase
                        : Enums.PhaseType.None;
                case Enums.LevelType.Retraining:
                    return savedPhase == Enums.PhaseType.RetrainingPhase || savedPhase == Enums.PhaseType.Result
                        ? savedPhase
                        : Enums.PhaseType.None;
                case Enums.LevelType.FieldUpgrade:
                    return savedPhase == Enums.PhaseType.FieldUpgradePhase || savedPhase == Enums.PhaseType.Result
                        ? savedPhase
                        : Enums.PhaseType.None;
                case Enums.LevelType.Battle:
                    if (savedPhase == Enums.PhaseType.Result)
                    {
                        return Enums.PhaseType.Result;
                    }

                    if (savedPhase == Enums.PhaseType.BattlePreparation ||
                        savedPhase == Enums.PhaseType.Battle ||
                        savedPhase == Enums.PhaseType.BattlePlayback)
                    {
                        return Enums.PhaseType.BattlePreparation;
                    }

                    return Enums.PhaseType.None;
                default:
                    return Enums.PhaseType.None;
            }
        }

        private sealed class BattleRestoreState
        {
            public int CurrentTurn;
            public int CurrentMana;
            public int NextHandRuntimeId;
            public int NextDeploymentOrder;
            public bool IsPlayerTurnActive;
            public bool HasGeneratedHandThisTurn;
            public bool ShouldGenerateHand;
            public int TotalEnemyKills;
            public int TotalDamageToEnemyBase;
            public int TotalDamageToPlayerBase;
            public BattleResultModel RestoredResult;
            public List<HandCardSaveDto> HandCards = new();
            public List<DeployedUnitSaveDto> DeployedUnits = new();
        }
    }
}
