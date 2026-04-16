using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class TrainingPipelineService
    {
        private readonly UnitConfigService _unitConfigService;
        private readonly PinConfigService _pinConfigService;
        private readonly LocationConfigService _locationConfigService;
        private readonly LevelConfigService _levelConfigService;
        private readonly PlinkoConfigService _plinkoConfigService;
        private readonly PlinkoPathFactory _plinkoPathFactory;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;

        public TrainingPipelineService(
            UnitConfigService unitConfigService,
            PinConfigService pinConfigService,
            LocationConfigService locationConfigService,
            LevelConfigService levelConfigService,
            PlinkoConfigService plinkoConfigService,
            PlinkoPathFactory plinkoPathFactory,
            PlinkoRuntimeService plinkoRuntimeService)
        {
            _unitConfigService = unitConfigService;
            _pinConfigService = pinConfigService;
            _locationConfigService = locationConfigService;
            _levelConfigService = levelConfigService;
            _plinkoConfigService = plinkoConfigService;
            _plinkoPathFactory = plinkoPathFactory;
            _plinkoRuntimeService = plinkoRuntimeService;
        }

        public bool TryPreparePurchaseTraining(
            int runtimeId,
            string unitTypeId,
            string displayName,
            string locationId,
            int levelIndex,
            IReadOnlyList<InstalledPinSnapshotModel> installedPins,
            out TrainingPipelineRunModel trainingRun)
        {
            trainingRun = null;
            var unitType = _unitConfigService.GetUnit(unitTypeId);
            if (unitType == null)
            {
                Debug.LogWarning($"Purchase training could not start: missing unit type '{unitTypeId}' for runtimeId={runtimeId}.");
                return false;
            }

            var result = _plinkoPathFactory.GeneratePurchaseResult(
                runtimeId,
                unitType,
                displayName,
                ResolveField(locationId, levelIndex),
                BuildInstalledPins(installedPins));
            trainingRun = RegisterResult(runtimeId, result);
            return trainingRun != null;
        }

        public TrainingPipelineRunModel PrepareRetraining(
            int runtimeId,
            string unitTypeId,
            string displayName,
            int attack,
            int health,
            int manaCost,
            string passiveAbilityId,
            int level,
            int upgradeCount,
            string locationId,
            int levelIndex,
            IReadOnlyList<InstalledPinSnapshotModel> installedPins)
        {
            var result = _plinkoPathFactory.GenerateRetrainingResult(
                runtimeId,
                unitTypeId,
                displayName,
                attack,
                health,
                manaCost,
                passiveAbilityId,
                level,
                upgradeCount,
                ResolveField(locationId, levelIndex),
                BuildInstalledPins(installedPins));
            return RegisterResult(runtimeId, result);
        }

        private TrainingPipelineRunModel RegisterResult(int runtimeId, PlinkoPathResultModel result)
        {
            if (result == null || result.Result == null)
            {
                Debug.LogWarning($"Training pipeline returned no result for runtimeId={runtimeId}.");
                return null;
            }

            _plinkoRuntimeService.SetResult(runtimeId, result);
            var totalNodeCount = result.Nodes != null ? result.Nodes.Count : 0;
            return new TrainingPipelineRunModel
            {
                Result = result,
                PlaybackDuration = Mathf.Max(0.75f, totalNodeCount * 0.2f),
                TotalNodeCount = totalNodeCount
            };
        }

        private PlinkoFieldSettingsData ResolveField(string locationId, int levelIndex)
        {
            var location = _locationConfigService.GetLocation(locationId);
            var levelData = _levelConfigService.GetLevel(locationId, levelIndex);
            var field = _plinkoConfigService.GetField(location, levelData);
            if (field == null)
            {
                Debug.LogWarning($"Training pipeline is using a null field for location='{locationId}', level={levelIndex}.");
            }

            return field;
        }

        private Dictionary<int, PinTypeData> BuildInstalledPins(IReadOnlyList<InstalledPinSnapshotModel> installedPins)
        {
            var result = new Dictionary<int, PinTypeData>();
            if (installedPins == null)
            {
                return result;
            }

            foreach (var installedPin in installedPins)
            {
                if (installedPin == null || string.IsNullOrWhiteSpace(installedPin.PinTypeId))
                {
                    continue;
                }

                var pinType = _pinConfigService.GetPin(installedPin.PinTypeId);
                if (pinType != null)
                {
                    result[installedPin.SlotIndex] = pinType;
                }
            }

            return result;
        }
    }
}
