using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class InitializeLocationBoardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly LocationConfigService _locationConfigService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _runStartedFilter;
        private EcsPool<RunStartedEvent> _runStartedEventPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<PlinkoBoardChangedEvent> _boardChangedEventPool;

        public InitializeLocationBoardSystem(LocationConfigService locationConfigService, RunEntityIndex runEntityIndex, InstalledPinIndex installedPinIndex)
        {
            _locationConfigService = locationConfigService;
            _runEntityIndex = runEntityIndex;
            _installedPinIndex = installedPinIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _runStartedFilter = world.Filter<RunStartedEvent>().End();
            _runStartedEventPool = world.GetPool<RunStartedEvent>();
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

            foreach (var eventEntity in _runStartedFilter)
            {
                var locationId = _locationPool.Get(runEntity).LocationId;
                var location = _locationConfigService.GetLocation(locationId);
                var field = location != null ? location.DefaultPlinkoField : null;
                if (field == null)
                {
                    world.DelEntity(eventEntity);
                    continue;
                }

                foreach (var existingEntity in world.Filter<InstalledPinComponent>().End())
                {
                    world.DelEntity(existingEntity);
                }
                _installedPinIndex.Clear();

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
                        installedPin.PinTypeId = row.Cells[columnIndex] != null && row.Cells[columnIndex].PinType != null
                            ? row.Cells[columnIndex].PinType.Id
                            : string.Empty;
                        _installedPinIndex.Register(slotIndex, entity);
                        slotIndex++;
                    }
                }

                _boardChangedEventPool.Add(world.NewEntity());
                world.DelEntity(eventEntity);
            }
        }
    }
}