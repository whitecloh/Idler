using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RestoreRunBoardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LocationConfigService _locationConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _requestFilter;
        private EcsPool<RestoreBoardStateRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<PlinkoBoardChangedEvent> _boardChangedEventPool;

        public RestoreRunBoardSystem(LocationConfigService locationConfigService, RunEntityIndex runEntityIndex, InstalledPinIndex installedPinIndex)
        {
            _locationConfigService = locationConfigService;
            _runEntityIndex = runEntityIndex;
            _installedPinIndex = installedPinIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<RestoreBoardStateRequest>().End();
            _requestPool = world.GetPool<RestoreBoardStateRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _boardChangedEventPool = world.GetPool<PlinkoBoardChangedEvent>();
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
                var boardDto = _requestPool.Get(requestEntity).Board;
                var locationId = _locationPool.Get(runEntity).LocationId;
                var location = _locationConfigService.GetLocation(locationId);
                var field = location != null ? location.DefaultPlinkoField : null;
                if (field == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                foreach (var existingEntity in world.Filter<InstalledPinComponent>().End())
                {
                    world.DelEntity(existingEntity);
                }
                _installedPinIndex.Clear();

                var savedPinsBySlot = new Dictionary<int, string>();
                if (boardDto != null && boardDto.InstalledPins != null)
                {
                    foreach (var pin in boardDto.InstalledPins)
                    {
                        if (pin != null)
                        {
                            savedPinsBySlot[pin.SlotIndex] = pin.PinTypeId;
                        }
                    }
                }

                var slotIndex = 0;
                for (var rowIndex = 0; rowIndex < field.Rows.Count; rowIndex++)
                {
                    var row = field.Rows[rowIndex];
                    if (row == null || row.Cells == null)
                    {
                        continue;
                    }

                    for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
                    {
                        var entity = world.NewEntity();
                        ref var installedPin = ref _installedPinPool.Add(entity);
                        installedPin.SlotIndex = slotIndex;
                        installedPin.RowIndex = rowIndex;
                        installedPin.ColumnIndex = columnIndex;
                        installedPin.PinTypeId = savedPinsBySlot.TryGetValue(slotIndex, out var overriddenPinTypeId)
                            ? overriddenPinTypeId
                            : row.Cells[columnIndex] != null && row.Cells[columnIndex].PinType != null
                                ? row.Cells[columnIndex].PinType.Id
                                : string.Empty;
                        _installedPinIndex.Register(slotIndex, entity);
                        slotIndex++;
                    }
                }

                _boardChangedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}