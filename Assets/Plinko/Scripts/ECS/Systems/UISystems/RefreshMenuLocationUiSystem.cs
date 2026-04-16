using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshMenuLocationUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private readonly LocationConfigService _locationConfigService;
        private readonly UnlocksService _unlocksService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<CurrentPhaseComponent> _phasePool;

        public RefreshMenuLocationUiSystem(
            RunSaveService runSaveService,
            LocationConfigService locationConfigService,
            UnlocksService unlocksService,
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _runSaveService = runSaveService;
            _locationConfigService = locationConfigService;
            _unlocksService = unlocksService;
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            _phasePool = systems.GetWorld().GetPool<CurrentPhaseComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            var hasRunEntity = _runEntityIndex.TryGetRunEntity(out var runEntity);
            var phase = hasRunEntity && _phasePool.Has(runEntity)
                ? _phasePool.Get(runEntity).Value
                : Enums.PhaseType.MainMenu;

            _uiCompositionRoot.RefreshMainMenu(BuildMainMenuViewData(hasRunEntity));
            _uiCompositionRoot.RefreshLocationSelection(BuildLocationSelectionViewData());
            _uiCompositionRoot.SyncScreenVisibility(hasRunEntity, phase);
        }

        private MainMenuViewData BuildMainMenuViewData(bool hasRunEntity)
        {
            var viewData = new MainMenuViewData();
            if (hasRunEntity)
            {
                return viewData;
            }

            var dto = _runSaveService.Load();
            if (dto == null || !dto.HasActiveRun || string.IsNullOrWhiteSpace(dto.LocationId) || dto.RunStatus != Enums.RunStatus.InProgress)
            {
                return viewData;
            }

            var location = _locationConfigService.GetLocation(dto.LocationId);
            if (location == null)
            {
                return viewData;
            }

            viewData.CanContinue = true;
            viewData.ContinueTitle = $"Continue: {location.DisplayName}";
            viewData.ContinueSubtitle = $"Level {dto.LevelIndex + 1} • {FormatPhase(dto.PhaseType)}";
            return viewData;
        }

        private LocationSelectionViewData BuildLocationSelectionViewData()
        {
            var viewData = new LocationSelectionViewData();
            var locations = _locationConfigService.GetAllLocations();
            foreach (var location in locations)
            {
                if (location == null)
                {
                    continue;
                }

                var maxCompletedLevelIndex = _unlocksService.GetMaxCompletedLevelIndex(location.Id);
                var totalLevelCount = location.Levels == null ? 0 : location.Levels.Count;
                var isUnlocked = _unlocksService.IsUnlocked(location.UnlockCondition);

                viewData.Locations.Add(new LocationEntryViewData
                {
                    LocationId = location.Id,
                    DisplayName = string.IsNullOrWhiteSpace(location.DisplayName) ? location.Id : location.DisplayName,
                    IsUnlocked = isUnlocked,
                    IsCompleted = _unlocksService.IsLocationCompleted(location.Id),
                    MaxCompletedLevelIndex = maxCompletedLevelIndex,
                    TotalLevelCount = totalLevelCount,
                    StatusText = BuildStatusText(location.Id, maxCompletedLevelIndex, totalLevelCount),
                    UnlockDescription = isUnlocked ? string.Empty : BuildUnlockDescription(location.UnlockCondition)
                });
            }

            return viewData;
        }

        private string BuildStatusText(string locationId, int maxCompletedLevelIndex, int totalLevelCount)
        {
            if (_unlocksService.IsLocationCompleted(locationId))
            {
                return $"Completed • {totalLevelCount}/{totalLevelCount} levels";
            }

            var completedLevelCount = maxCompletedLevelIndex + 1;
            return $"Progress • {completedLevelCount}/{totalLevelCount} levels";
        }

        private string BuildUnlockDescription(UnlockConditionData condition)
        {
            if (condition == null)
            {
                return string.Empty;
            }

            var requirements = new List<string>();
            if (condition.RequiresCompletedLocation && !string.IsNullOrWhiteSpace(condition.RequiredLocationId))
            {
                requirements.Add($"complete {GetLocationName(condition.RequiredLocationId)}");
            }

            if (!string.IsNullOrWhiteSpace(condition.RequiredLocationId) && condition.RequiredCompletedLevelIndex >= 0)
            {
                requirements.Add($"clear level {condition.RequiredCompletedLevelIndex + 1} in {GetLocationName(condition.RequiredLocationId)}");
            }

            return requirements.Count == 0
                ? "Locked"
                : $"Unlock: {string.Join(" and ", requirements)}";
        }

        private string GetLocationName(string locationId)
        {
            var location = _locationConfigService.GetLocation(locationId);
            return location == null || string.IsNullOrWhiteSpace(location.DisplayName)
                ? locationId
                : location.DisplayName;
        }

        private string FormatPhase(Enums.PhaseType phase)
        {
            switch (phase)
            {
                case Enums.PhaseType.PurchasePhase:
                    return "Purchase";
                case Enums.PhaseType.RetrainingPhase:
                    return "Retraining";
                case Enums.PhaseType.FieldUpgradePhase:
                    return "Field Upgrade";
                case Enums.PhaseType.BattlePreparation:
                    return "Battle Preparation";
                case Enums.PhaseType.Battle:
                    return "Battle";
                case Enums.PhaseType.BattlePlayback:
                    return "Battle Playback";
                case Enums.PhaseType.Result:
                    return "Result";
                default:
                    return phase.ToString();
            }
        }
    }
}
