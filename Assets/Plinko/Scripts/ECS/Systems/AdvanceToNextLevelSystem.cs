using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class AdvanceToNextLevelSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly LevelConfigService _levelConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _pendingPinFilter;
        private EcsPool<AdvanceToNextLevelRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<RunStatusComponent> _statusPool;
        private EcsPool<PurchasePhaseStateComponent> _purchaseStatePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<LevelCompletedEvent> _levelCompletedEventPool;
        private EcsPool<RunCompletedEvent> _runCompletedEventPool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;
        private EcsPool<StartLevelRequest> _startLevelRequestPool;

        public AdvanceToNextLevelSystem(
            BattleRuntimeService battleRuntimeService,
            LevelConfigService levelConfigService,
            RunEntityIndex runEntityIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _levelConfigService = levelConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<AdvanceToNextLevelRequest>().End();
            _pendingPinFilter = world.Filter<PendingPurchasedPinComponent>().End();
            _requestPool = world.GetPool<AdvanceToNextLevelRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _statusPool = world.GetPool<RunStatusComponent>();
            _purchaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _levelCompletedEventPool = world.GetPool<LevelCompletedEvent>();
            _runCompletedEventPool = world.GetPool<RunCompletedEvent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
            _startLevelRequestPool = world.GetPool<StartLevelRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                _requestPool.Get(requestEntity);
                var currentPhase = _phasePool.Get(runEntity).Value;
                if (!CanAdvance(runEntity, currentPhase))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                _levelCompletedEventPool.Add(world.NewEntity());
                var nextLevelIndex = _levelPool.Get(runEntity).LevelIndex + 1;
                var locationId = _locationPool.Get(runEntity).LocationId;
                if (_levelConfigService.GetLevel(locationId, nextLevelIndex) == null)
                {
                    _statusPool.Get(runEntity).Value = Enums.RunStatus.Victory;
                    _phasePool.Get(runEntity).Value = Enums.PhaseType.Result;
                    _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.Result;
                    _runCompletedEventPool.Add(world.NewEntity());
                    world.DelEntity(requestEntity);
                    continue;
                }

                _startLevelRequestPool.Add(world.NewEntity()).LevelIndex = nextLevelIndex;
                world.DelEntity(requestEntity);
            }
        }

        private bool CanAdvance(int runEntity, Enums.PhaseType currentPhase)
        {
            switch (currentPhase)
            {
                case Enums.PhaseType.PurchasePhase:
                    return !_purchaseStatePool.Has(runEntity) ||
                           _purchaseStatePool.Get(runEntity).ActiveTrainingCount <= 0;
                case Enums.PhaseType.RetrainingPhase:
                    return !_retrainingStatePool.Has(runEntity) ||
                           _retrainingStatePool.Get(runEntity).ActiveTrainingCount <= 0;
                case Enums.PhaseType.FieldUpgradePhase:
                    return !_fieldUpgradeStatePool.Has(runEntity) ||
                           (_fieldUpgradeStatePool.Get(runEntity).SelectedSlotIndex < 0 && !HasPendingPins());
                case Enums.PhaseType.Result:
                    return _battleRuntimeService.CurrentResult != null &&
                           _battleRuntimeService.CurrentResult.IsVictory &&
                           !_battleRuntimeService.CurrentResult.IsDefeat &&
                           HasNextLevel(runEntity);
                default:
                    return false;
            }
        }

        private bool HasNextLevel(int runEntity)
        {
            var locationId = _locationPool.Get(runEntity).LocationId;
            var nextLevelIndex = _levelPool.Get(runEntity).LevelIndex + 1;
            return _levelConfigService.GetLevel(locationId, nextLevelIndex) != null;
        }

        private bool HasPendingPins()
        {
            foreach (var _ in _pendingPinFilter)
            {
                return true;
            }

            return false;
        }
    }
}
