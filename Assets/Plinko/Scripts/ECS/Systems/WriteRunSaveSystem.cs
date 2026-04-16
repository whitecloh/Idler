using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Models;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class WriteRunSaveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<SaveRunRequest> _requestPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBasePool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBasePool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _unitManaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _unitLevelPool;
        private EcsPool<PassiveAbilityIdComponent> _passiveAbilityPool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<RunSavedEvent> _runSavedEventPool;

        private EcsFilter _ownedUnitFilter;
        private EcsFilter _installedPinFilter;

        public WriteRunSaveSystem(RunSaveService runSaveService, RunEntityIndex runEntityIndex)
        {
            _runSaveService = runSaveService;
            _runEntityIndex = runEntityIndex;
        }
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<SaveRunRequest>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _requestPool = world.GetPool<SaveRunRequest>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            _purchasePool = world.GetPool<PurchasePhaseStateComponent>();
            _fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _unitManaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _unitLevelPool = world.GetPool<UnitLevelComponent>();
            _passiveAbilityPool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _runSavedEventPool = world.GetPool<RunSavedEvent>();
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
                var dto = new RunSaveDto
                {
                    HasActiveRun = true,
                    LocationId = _locationPool.Get(runEntity).LocationId,
                    LevelIndex = _levelPool.Get(runEntity).LevelIndex,
                    LevelType = _levelTypePool.Get(runEntity).Value,
                    PhaseType = _phasePool.Get(runEntity).Value,
                    Gold = _goldPool.Get(runEntity).Value,
                    PlayerBaseHealth = _playerBasePool.Get(runEntity).Value,
                    EnemyBaseHealth = _enemyBasePool.Get(runEntity).Value,
                    PurchaseRerollCount = _purchasePool.Has(runEntity) ? _purchasePool.Get(runEntity).RerollCount : 0,
                    PinRerollCount = _fieldUpgradePool.Has(runEntity) ? _fieldUpgradePool.Get(runEntity).RerollCount : 0,
                    OwnedUnits = new List<OwnedUnitSaveDto>(),
                    Board = new PlinkoBoardSaveDto { InstalledPins = new List<InstalledPinSaveDto>() }
                };

                foreach (var ownedUnitEntity in _ownedUnitFilter)
                {
                    dto.OwnedUnits.Add(new OwnedUnitSaveDto
                    {
                        RuntimeId = _ownedUnitPool.Get(ownedUnitEntity).RuntimeId,
                        UnitTypeId = _unitTypeIdPool.Get(ownedUnitEntity).Value,
                        Attack = _unitStatsPool.Get(ownedUnitEntity).Attack,
                        Health = _unitStatsPool.Get(ownedUnitEntity).Health,
                        ManaCost = _unitManaCostPool.Get(ownedUnitEntity).Value,
                        DisplayName = _displayNamePool.Get(ownedUnitEntity).Value,
                        Level = _unitLevelPool.Get(ownedUnitEntity).Value,
                        PassiveAbilityId = _passiveAbilityPool.Get(ownedUnitEntity).Value,
                        UpgradeCount = _upgradeCountPool.Get(ownedUnitEntity).Value
                    });
                }

                foreach (var installedPinEntity in _installedPinFilter)
                {
                    var installedPin = _installedPinPool.Get(installedPinEntity);
                    dto.Board.InstalledPins.Add(new InstalledPinSaveDto
                    {
                        SlotIndex = installedPin.SlotIndex,
                        PinTypeId = installedPin.PinTypeId
                    });
                }

                _runSaveService.Save(dto);
                _runSavedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}