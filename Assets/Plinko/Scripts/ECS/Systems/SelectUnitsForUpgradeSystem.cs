using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class SelectUnitsForUpgradeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _requestFilter;
        private EcsPool<SelectUnitsForUpgradeRequest> _requestPool;
        private EcsPool<SelectedForUpgradeComponent> _selectedPool;
        private EcsPool<UpgradePhaseStateComponent> _upgradePhaseStatePool;
        private EcsPool<UpgradeSelectionChangedEvent> _selectionChangedEventPool;

        public SelectUnitsForUpgradeSystem(RunEntityIndex runEntityIndex, OwnedUnitIndex ownedUnitIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<SelectUnitsForUpgradeRequest>().End();
            _requestPool = world.GetPool<SelectUnitsForUpgradeRequest>();
            _selectedPool = world.GetPool<SelectedForUpgradeComponent>();
            _upgradePhaseStatePool = world.GetPool<UpgradePhaseStateComponent>();
            _selectionChangedEventPool = world.GetPool<UpgradeSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            ref var state = ref _upgradePhaseStatePool.GetOrAdd(runEntity);
            foreach (var requestEntity in _requestFilter)
            {
                if (state.IsSelectionLocked)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var runtimeId = _requestPool.Get(requestEntity).RuntimeId;
                if (!_ownedUnitIndex.TryGet(runtimeId, out var ownedUnitEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (_selectedPool.Has(ownedUnitEntity))
                {
                    _selectedPool.Del(ownedUnitEntity);
                    state.SelectedCount = Mathf.Max(0, state.SelectedCount - 1);
                }
                else
                {
                    var selectionLimit = Mathf.Max(1, _gameSettingsService.GetUpgradeSelectionLimit());
                    if (state.SelectedCount >= selectionLimit)
                    {
                        world.DelEntity(requestEntity);
                        continue;
                    }

                    _selectedPool.Add(ownedUnitEntity);
                    state.SelectedCount++;
                }

                _selectionChangedEventPool.Add(world.NewEntity()).SelectedCount = state.SelectedCount;
                world.DelEntity(requestEntity);
            }
        }
    }
}