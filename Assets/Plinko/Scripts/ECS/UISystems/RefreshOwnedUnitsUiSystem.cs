using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Controllers;

namespace Plinko.Scripts.ECS.UISystems
{
    public sealed class RefreshOwnedUnitsUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly OwnedUnitsScreenController _controller;

        private EcsFilter _ownedUnitPoolChangedFilter;
        private EcsFilter _selectionChangedFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<SelectedForUpgradeComponent> _selectedPool;

        public RefreshOwnedUnitsUiSystem(OwnedUnitsScreenController controller)
        {
            _controller = controller;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _ownedUnitPoolChangedFilter = world.Filter<OwnedUnitPoolChangedEvent>().End();
            _selectionChangedFilter = world.Filter<UpgradeSelectionChangedEvent>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _selectedPool = world.GetPool<SelectedForUpgradeComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_controller == null)
            {
                return;
            }

            var shouldRefresh = _ownedUnitPoolChangedFilter.GetEntitiesCount() > 0 ||
                                _selectionChangedFilter.GetEntitiesCount() > 0;
            if (!shouldRefresh)
            {
                return;
            }

            var ownedUnits = new List<OwnedUnitViewData>();
            foreach (var ownedUnitEntity in _ownedUnitFilter)
            {
                ownedUnits.Add(new OwnedUnitViewData
                {
                    RuntimeId = _ownedUnitPool.Get(ownedUnitEntity).RuntimeId,
                    DisplayName = _displayNamePool.Get(ownedUnitEntity).Value,
                    Level = _levelPool.Get(ownedUnitEntity).Value,
                    UnitTypeId = _unitTypePool.Get(ownedUnitEntity).Value,
                    Attack = _unitStatsPool.Get(ownedUnitEntity).Attack,
                    Health = _unitStatsPool.Get(ownedUnitEntity).Health,
                    ManaCost = _manaCostPool.Get(ownedUnitEntity).Value,
                    UpgradeCount = _upgradeCountPool.Get(ownedUnitEntity).Value,
                    IsSelectedForUpgrade = _selectedPool.Has(ownedUnitEntity)
                });
            }

            _controller.Refresh(ownedUnits);
        }
    }
}