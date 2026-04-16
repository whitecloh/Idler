using System.Collections.Generic;
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
    public sealed class ContinueRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private readonly GameSettingsService _gameSettingsService;
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
        private EcsPool<PurchasePhaseStateComponent> _purchasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<BattleStateComponent> _battlePool;
        private EcsPool<RestoreOwnedUnitsRequest> _restoreOwnedUnitsRequestPool;
        private EcsPool<RestoreBoardStateRequest> _restoreBoardRequestPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;

        public ContinueRunSystem(
            RunSaveService runSaveService,
            GameSettingsService gameSettingsService,
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex,
            ShopOfferIndex shopOfferIndex,
            PinShopOfferIndex pinShopOfferIndex,
            InstalledPinIndex installedPinIndex)
        {
            _runSaveService = runSaveService;
            _gameSettingsService = gameSettingsService;
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
            _purchasePool = world.GetPool<PurchasePhaseStateComponent>();
            _retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _battlePool = world.GetPool<BattleStateComponent>();
            _restoreOwnedUnitsRequestPool = world.GetPool<RestoreOwnedUnitsRequest>();
            _restoreBoardRequestPool = world.GetPool<RestoreBoardStateRequest>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
        }
        
                public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                var dto = _runSaveService.Load();
                if (dto == null || !dto.HasActiveRun || string.IsNullOrWhiteSpace(dto.LocationId))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                RuntimeEntityCleanup.ClearForNewRun(world, _runEntityIndex, _ownedUnitIndex, _shopOfferIndex, _pinShopOfferIndex, _installedPinIndex);

                var runEntity = world.NewEntity();
                _runPool.Add(runEntity);
                _locationPool.Add(runEntity).LocationId = dto.LocationId;
                _levelPool.Add(runEntity).LevelIndex = dto.LevelIndex;
                _levelTypePool.Add(runEntity).Value = dto.LevelType;
                _phasePool.Add(runEntity).Value = dto.PhaseType;
                _goldPool.Add(runEntity).Value = dto.Gold;
                _playerBasePool.Add(runEntity) = new PlayerBaseHealthComponent
                {
                    Value = dto.PlayerBaseHealth,
                    MaxValue = _gameSettingsService.GetStartingBaseHealth()
                };
                _enemyBasePool.Add(runEntity) = new EnemyBaseHealthComponent { Value = dto.EnemyBaseHealth, MaxValue = dto.EnemyBaseHealth };
                _statusPool.Add(runEntity).Value = Enums.RunStatus.InProgress;
                _manaPool.Add(runEntity).Value = _gameSettingsService.GetManaPerTurn();
                _purchasePool.Add(runEntity) = new PurchasePhaseStateComponent { RerollCount = dto.PurchaseRerollCount, ActiveTrainingCount = 0, CanEnterBattle = true };
                _retrainingPool.Add(runEntity) = new RetrainingPhaseStateComponent
                {
                    SelectedCount = 0,
                    SelectionLimit = _gameSettingsService.GetDefaultRetrainingSelectionLimit(),
                    IsSelectionLocked = false,
                    ActiveTrainingCount = 0
                };
                _fieldUpgradePool.Add(runEntity) = new FieldUpgradePhaseStateComponent { RerollCount = dto.PinRerollCount, SelectedSlotIndex = -1, IsPlacementHighlighted = false };
                _battlePool.Add(runEntity) = new BattleStateComponent { CurrentTurn = 0, IsResolved = false };

                _runEntityIndex.SetRunEntity(runEntity);

                _restoreOwnedUnitsRequestPool.Add(world.NewEntity()).OwnedUnits = dto.OwnedUnits ?? new List<OwnedUnitSaveDto>();
                _restoreBoardRequestPool.Add(world.NewEntity()).Board = dto.Board ?? new PlinkoBoardSaveDto();
                _goldChangedEventPool.Add(world.NewEntity()).Value = dto.Gold;
                _phaseChangedEventPool.Add(world.NewEntity()).Value = dto.PhaseType;
                world.DelEntity(requestEntity);
            }
        }
    }
}