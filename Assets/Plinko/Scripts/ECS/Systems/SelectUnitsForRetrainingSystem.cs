using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class SelectUnitsForRetrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;

        private EcsFilter _requestFilter;
        private EcsPool<SelectUnitsForRetrainingRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<RetrainingPhaseStateComponent> _retrainingStatePool;
        private EcsPool<SelectedForRetrainingComponent> _selectedForRetrainingPool;
        private EcsPool<RetrainingSelectionChangedEvent> _selectionChangedEventPool;

        public SelectUnitsForRetrainingSystem(RunEntityIndex runEntityIndex, OwnedUnitIndex ownedUnitIndex)
        {
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<SelectUnitsForRetrainingRequest>().End();
            _requestPool = world.GetPool<SelectUnitsForRetrainingRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _retrainingStatePool = world.GetPool<RetrainingPhaseStateComponent>();
            _selectedForRetrainingPool = world.GetPool<SelectedForRetrainingComponent>();
            _selectionChangedEventPool = world.GetPool<RetrainingSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (_phasePool.Get(runEntity).Value != Enums.PhaseType.RetrainingPhase)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var retrainingState = ref _retrainingStatePool.Get(runEntity);
                if (retrainingState.IsSelectionLocked || retrainingState.ActiveTrainingCount > 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (!_ownedUnitIndex.TryGet(request.RuntimeId, out var ownedUnitEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var nextSelectedCount = retrainingState.SelectedCount;
                var hasChanged = false;
                if (_selectedForRetrainingPool.Has(ownedUnitEntity))
                {
                    _selectedForRetrainingPool.Del(ownedUnitEntity);
                    nextSelectedCount = Mathf.Max(0, nextSelectedCount - 1);
                    hasChanged = true;
                }
                else if (retrainingState.SelectionLimit > 0 && nextSelectedCount < retrainingState.SelectionLimit)
                {
                    _selectedForRetrainingPool.Add(ownedUnitEntity);
                    nextSelectedCount++;
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    retrainingState.SelectedCount = nextSelectedCount;
                    _selectionChangedEventPool.Add(world.NewEntity()).SelectedCount = nextSelectedCount;
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}
