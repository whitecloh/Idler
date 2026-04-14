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
    public sealed class SelectBoardSlotSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _requestFilter;
        private EcsPool<SelectBoardSlotRequest> _requestPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePhaseStatePool;
        private EcsPool<BoardSlotSelectionChangedEvent> _boardSlotSelectionChangedEventPool;

        public SelectBoardSlotSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<SelectBoardSlotRequest>().End();
            _requestPool = world.GetPool<SelectBoardSlotRequest>();
            _fieldUpgradePhaseStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _boardSlotSelectionChangedEventPool = world.GetPool<BoardSlotSelectionChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var maxSlotIndex = Mathf.Max(0, _gameSettingsService.GetBoardSlotCount() - 1);
            foreach (var requestEntity in _requestFilter)
            {
                var slotIndex = _requestPool.Get(requestEntity).SlotIndex;
                slotIndex = Mathf.Clamp(slotIndex, 0, maxSlotIndex);
                _fieldUpgradePhaseStatePool.GetOrAdd(runEntity).SelectedSlotIndex = slotIndex;
                _boardSlotSelectionChangedEventPool.Add(world.NewEntity()).SlotIndex = slotIndex;
                world.DelEntity(requestEntity);
            }
        }
    }
}