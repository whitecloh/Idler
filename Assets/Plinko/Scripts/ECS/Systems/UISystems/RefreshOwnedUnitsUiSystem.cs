using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Systems.UISystems
{
    public sealed class RefreshOwnedUnitsUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly UiCompositionRoot _uiCompositionRoot;

        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _statsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<RetrainingPurchasedOnLevelComponent> _purchasedOnLevelPool;

        private EcsFilter _ownedFilter;

        public RefreshOwnedUnitsUiSystem(
            RunEntityIndex runEntityIndex,
            UiCompositionRoot uiCompositionRoot)
        {
            _runEntityIndex = runEntityIndex;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _statsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _purchasedOnLevelPool = world.GetPool<RetrainingPurchasedOnLevelComponent>();
            _ownedFilter = world.Filter<OwnedUnitComponent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_uiCompositionRoot == null)
            {
                return;
            }

            if (!_runEntityIndex.TryGetRunEntity(out _))
            {
                _uiCompositionRoot.RefreshOwnedUnits(new List<OwnedUnitViewData>());
                return;
            }

            var ownedUnits = new List<OwnedUnitViewData>();
            foreach (var ownedEntity in _ownedFilter)
            {
                ownedUnits.Add(new OwnedUnitViewData
                {
                    RuntimeId = _ownedUnitPool.Get(ownedEntity).RuntimeId,
                    DisplayName = _displayNamePool.Get(ownedEntity).Value,
                    Level = _levelPool.Get(ownedEntity).Value,
                    UnitTypeId = _unitTypePool.Get(ownedEntity).Value,
                    Attack = _statsPool.Get(ownedEntity).Attack,
                    Health = _statsPool.Get(ownedEntity).Health,
                    ManaCost = _manaCostPool.Get(ownedEntity).Value,
                    UpgradeCount = _upgradeCountPool.Get(ownedEntity).Value,
                    IsSelectedForRetraining = _purchasedOnLevelPool.Has(ownedEntity)
                });
            }

            ownedUnits.Sort((left, right) => left.RuntimeId.CompareTo(right.RuntimeId));
            _uiCompositionRoot.RefreshOwnedUnits(ownedUnits);
        }
    }
}
