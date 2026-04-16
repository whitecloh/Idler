using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RouteLevelTypeToPhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _levelLoadedFilter;
        private EcsFilter _selectedForRetrainingFilter;
        private EcsPool<LevelLoadedEvent> _levelLoadedEventPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<SelectedForRetrainingComponent> _selectedForRetrainingPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<PurchasePhaseEnteredEvent> _purchasePhaseEnteredEventPool;
        private EcsPool<RetrainingPhaseEnteredEvent> _retrainingPhaseEnteredEventPool;
        private EcsPool<FieldUpgradePhaseEnteredEvent> _fieldUpgradePhaseEnteredEventPool;
        private EcsPool<BeginBattleTurnRequest> _beginBattleTurnRequestPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;

        public RouteLevelTypeToPhaseSystem(LevelConfigService levelConfigService, GameSettingsService gameSettingsService, RunEntityIndex runEntityIndex)
        {
            _levelConfigService = levelConfigService;
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _levelLoadedFilter = world.Filter<LevelLoadedEvent>().End();
            _selectedForRetrainingFilter = world.Filter<SelectedForRetrainingComponent>().End();
            _levelLoadedEventPool = world.GetPool<LevelLoadedEvent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _selectedForRetrainingPool = world.GetPool<SelectedForRetrainingComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _purchasePhaseEnteredEventPool = world.GetPool<PurchasePhaseEnteredEvent>();
            _retrainingPhaseEnteredEventPool = world.GetPool<RetrainingPhaseEnteredEvent>();
            _fieldUpgradePhaseEnteredEventPool = world.GetPool<FieldUpgradePhaseEnteredEvent>();
            _beginBattleTurnRequestPool = world.GetPool<BeginBattleTurnRequest>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var eventEntity in _levelLoadedFilter)
            {
                var locationId = _locationPool.Get(runEntity).LocationId;
                var levelIndex = _levelPool.Get(runEntity).LevelIndex;
                var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
                if (levelData == null)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                var nextPhase = Enums.PhaseType.PurchasePhase;
                switch (levelData.LevelType)
                {
                    case Enums.LevelType.Purchase:
                        nextPhase = Enums.PhaseType.PurchasePhase;
                        break;
                    case Enums.LevelType.Retraining:
                        nextPhase = Enums.PhaseType.RetrainingPhase;
                        break;
                    case Enums.LevelType.FieldUpgrade:
                        nextPhase = Enums.PhaseType.FieldUpgradePhase;
                        break;
                    case Enums.LevelType.Battle:
                        nextPhase = Enums.PhaseType.BattlePreparation;
                        break;
                }

                _phasePool.Get(runEntity).Value = nextPhase;

                ref var retrainingState = ref _retrainingPool.Get(runEntity);
                retrainingState.SelectedCount = 0;
                retrainingState.SelectionLimit = levelData.PreBattlePhase != null && levelData.PreBattlePhase.OverrideRetrainingSelectionLimit > 0
                    ? levelData.PreBattlePhase.OverrideRetrainingSelectionLimit
                    : _gameSettingsService.GetDefaultRetrainingSelectionLimit();
                retrainingState.IsSelectionLocked = false;
                retrainingState.ActiveTrainingCount = 0;

                ref var fieldUpgradeState = ref _fieldUpgradePool.Get(runEntity);
                fieldUpgradeState.SelectedSlotIndex = -1;
                fieldUpgradeState.IsPlacementHighlighted = false;

                var selectedEntities = new List<int>();
                foreach (var selectedEntity in _selectedForRetrainingFilter)
                {
                    selectedEntities.Add(selectedEntity);
                }

                foreach (var selectedEntity in selectedEntities)
                {
                    _selectedForRetrainingPool.Del(selectedEntity);
                }

                _phaseChangedEventPool.Add(world.NewEntity()).Value = nextPhase;
                switch (nextPhase)
                {
                    case Enums.PhaseType.PurchasePhase:
                        _purchasePhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.RetrainingPhase:
                        _retrainingPhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.FieldUpgradePhase:
                        _fieldUpgradePhaseEnteredEventPool.Add(world.NewEntity());
                        _saveRunRequestPool.Add(world.NewEntity());
                        break;
                    case Enums.PhaseType.BattlePreparation:
                        _beginBattleTurnRequestPool.Add(world.NewEntity());
                        break;
                }

                world.DelEntity(eventEntity);
            }
        }
    }
}
