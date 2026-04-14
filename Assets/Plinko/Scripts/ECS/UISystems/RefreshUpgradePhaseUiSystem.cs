using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View.Controllers;
using UnityEngine;

namespace Plinko.Scripts.ECS.UISystems
{
    public sealed class RefreshUpgradePhaseUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly UpgradePhaseScreenController _controller;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _upgradeEnteredFilter;
        private EcsFilter _selectionChangedFilter;
        private EcsFilter _ownedUnitPoolChangedFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<UpgradePhaseStateComponent> _upgradePhaseStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<SelectedForUpgradeComponent> _selectedPool;

        public RefreshUpgradePhaseUiSystem(UpgradePhaseScreenController controller, RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _controller = controller;
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _upgradeEnteredFilter = world.Filter<UpgradePhaseEnteredEvent>().End();
            _selectionChangedFilter = world.Filter<UpgradeSelectionChangedEvent>().End();
            _ownedUnitPoolChangedFilter = world.Filter<OwnedUnitPoolChangedEvent>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _upgradePhaseStatePool = world.GetPool<UpgradePhaseStateComponent>();
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
            if (_controller == null || !_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldRefresh = _phaseChangedFilter.GetEntitiesCount() > 0 ||
                                _upgradeEnteredFilter.GetEntitiesCount() > 0 ||
                                _selectionChangedFilter.GetEntitiesCount() > 0 ||
                                _ownedUnitPoolChangedFilter.GetEntitiesCount() > 0;
            if (!shouldRefresh)
            {
                return;
            }

            var isVisible = _phasePool.Get(runEntity).Value == Enums.PhaseType.UpgradePhase;
            _controller.Show(isVisible);
            if (!isVisible)
            {
                return;
            }

            ref var state = ref _upgradePhaseStatePool.GetOrAdd(runEntity);
            var selectionLimit = Mathf.Max(1, _gameSettingsService.GetUpgradeSelectionLimit());
            var viewData = new UpgradePhaseViewData
            {
                SelectedCount = state.SelectedCount,
                SelectionLimit = selectionLimit,
                IsSelectionLocked = state.IsSelectionLocked,
                CanConfirm = !state.IsSelectionLocked && state.SelectedCount >= 1 && state.SelectedCount <= selectionLimit,
                OwnedUnits = new List<OwnedUnitViewData>()
            };

            foreach (var ownedUnitEntity in _ownedUnitFilter)
            {
                viewData.OwnedUnits.Add(new OwnedUnitViewData
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

            _controller.Refresh(viewData);
        }
    }
}