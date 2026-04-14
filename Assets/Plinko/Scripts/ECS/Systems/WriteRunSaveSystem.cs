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

        private EcsFilter _saveRequestFilter;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBaseHealthPool;
        private EcsFilter _ownedUnitFilter;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<PassiveAbilityIdComponent> _passivePool;
        private EcsPool<UpgradeCountComponent> _upgradeCountPool;
        private EcsPool<RunSavedEvent> _runSavedEventPool;

        public WriteRunSaveSystem(RunSaveService runSaveService, RunEntityIndex runEntityIndex)
        {
            _runSaveService = runSaveService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _saveRequestFilter = world.Filter<SaveRunRequest>().End();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _playerBaseHealthPool = world.GetPool<PlayerBaseHealthComponent>();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _passivePool = world.GetPool<PassiveAbilityIdComponent>();
            _upgradeCountPool = world.GetPool<UpgradeCountComponent>();
            _runSavedEventPool = world.GetPool<RunSavedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _saveRequestFilter)
            {
                if (_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    var dto = BuildDto(runEntity);
                    _runSaveService.Save(dto);
                    _runSavedEventPool.Add(world.NewEntity());
                }

                world.DelEntity(requestEntity);
            }
        }

        private RunSaveDto BuildDto(int runEntity)
        {
            var dto = new RunSaveDto
            {
                LocationId = _locationPool.Get(runEntity).LocationId,
                LevelIndex = _levelPool.Get(runEntity).LevelIndex,
                LevelType = _levelTypePool.Get(runEntity).Value,
                PhaseType = _phasePool.Get(runEntity).Value,
                Gold = _goldPool.Get(runEntity).Value,
                PlayerBaseHealth = _playerBaseHealthPool.Get(runEntity).Value,
                HasActiveRun = true,
                OwnedUnits = new List<OwnedUnitSaveDto>()
            };

            foreach (var ownedUnitEntity in _ownedUnitFilter)
            {
                dto.OwnedUnits.Add(new OwnedUnitSaveDto
                {
                    RuntimeId = _ownedUnitPool.Get(ownedUnitEntity).RuntimeId,
                    UnitTypeId = _unitTypePool.Get(ownedUnitEntity).Value,
                    Attack = _unitStatsPool.Get(ownedUnitEntity).Attack,
                    Health = _unitStatsPool.Get(ownedUnitEntity).Health,
                    ManaCost = _manaCostPool.Get(ownedUnitEntity).Value,
                    PassiveAbilityId = _passivePool.Get(ownedUnitEntity).Value,
                    UpgradeCount = _upgradeCountPool.Get(ownedUnitEntity).Value
                });
            }

            return dto;
        }
    }
}