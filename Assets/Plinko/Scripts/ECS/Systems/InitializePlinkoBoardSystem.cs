using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class InitializePlinkoBoardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _runStartedFilter;
        private EcsFilter _installedPinFilter;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<PlinkoBoardChangedEvent> _plinkoBoardChangedEventPool;

        public InitializePlinkoBoardSystem(GameSettingsService gameSettingsService)
        {
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _runStartedFilter = world.Filter<RunStartedEvent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _plinkoBoardChangedEventPool = world.GetPool<PlinkoBoardChangedEvent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (_runStartedFilter.GetEntitiesCount() <= 0)
            {
                return;
            }

            foreach (var installedPinEntity in _installedPinFilter)
            {
                world.DelEntity(installedPinEntity);
            }

            var rows = _gameSettingsService.GetPlinkoBoardRows();
            var globalIndex = 0;
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                if (row == null || row.Cells == null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
                {
                    var cell = row.Cells[columnIndex];
                    var pinEntity = world.NewEntity();
                    ref var installedPin = ref _installedPinPool.Add(pinEntity);
                    installedPin.GlobalIndex = globalIndex;
                    installedPin.RowIndex = rowIndex;
                    installedPin.ColumnIndex = columnIndex;
                    installedPin.PinTypeId = cell != null && cell.PinType != null ? cell.PinType.Id : string.Empty;
                    globalIndex++;
                }
            }

            if (globalIndex == 0)
            {
                var fallbackCount = _gameSettingsService.GetBoardSlotCount();
                for (var fallbackIndex = 0; fallbackIndex < fallbackCount; fallbackIndex++)
                {
                    var pinEntity = world.NewEntity();
                    ref var installedPin = ref _installedPinPool.Add(pinEntity);
                    installedPin.GlobalIndex = fallbackIndex;
                    installedPin.RowIndex = 0;
                    installedPin.ColumnIndex = fallbackIndex;
                    installedPin.PinTypeId = string.Empty;
                }
            }

            _plinkoBoardChangedEventPool.Add(world.NewEntity());
        }
    }
}