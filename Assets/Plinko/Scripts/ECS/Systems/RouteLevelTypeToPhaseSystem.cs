using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RouteLevelTypeToPhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LevelConfigService _levelConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _levelLoadedFilter;
        private EcsPool<LevelLoadedEvent> _levelLoadedEventPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;

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
            _levelLoadedEventPool = world.GetPool<LevelLoadedEvent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
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
                }

                _phasePool.Get(runEntity).Value = nextPhase;
                _retrainingPool.Get(runEntity).SelectionLimit = levelData.PreBattlePhase != null && levelData.PreBattlePhase.OverrideRetrainingSelectionLimit > 0
                    ? levelData.PreBattlePhase.OverrideRetrainingSelectionLimit
                    : _gameSettingsService.GetDefaultRetrainingSelectionLimit();

                _phaseChangedEventPool.Add(world.NewEntity()).Value = nextPhase;
                world.DelEntity(eventEntity);
            }
        }
    }
}