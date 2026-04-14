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
    public sealed class ValidateUpgradeSelectionCountSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _requestFilter;
        private EcsPool<ConfirmUpgradeSelectionRequest> _requestPool;
        private EcsPool<UpgradePhaseStateComponent> _upgradePhaseStatePool;
        private EcsPool<UpgradeSelectionConfirmedEvent> _selectionConfirmedEventPool;

        public ValidateUpgradeSelectionCountSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ConfirmUpgradeSelectionRequest>().End();
            _requestPool = world.GetPool<ConfirmUpgradeSelectionRequest>();
            _upgradePhaseStatePool = world.GetPool<UpgradePhaseStateComponent>();
            _selectionConfirmedEventPool = world.GetPool<UpgradeSelectionConfirmedEvent>();
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
                var selectionLimit = Mathf.Max(1, _gameSettingsService.GetUpgradeSelectionLimit());
                if (!state.IsSelectionLocked && state.SelectedCount >= 1 && state.SelectedCount <= selectionLimit)
                {
                    state.IsSelectionLocked = true;
                    _selectionConfirmedEventPool.Add(world.NewEntity());
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}