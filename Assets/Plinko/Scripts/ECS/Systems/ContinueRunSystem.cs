using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ContinueRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsPool<ContinueRunRequest> _requestPool;
        private EcsPool<RunComponent> _runPool;
        private EcsPool<CurrentLocationComponent> _locationPool;
        private EcsPool<CurrentLevelComponent> _levelPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBaseHealthPool;
        private EcsPool<RunStatusComponent> _runStatusPool;
        private EcsPool<RunStartedEvent> _runStartedPool;
        private EcsPool<GoldChangedEvent> _goldChangedPool;
        private EcsPool<RegisterOwnedUnitRequest> _registerOwnedUnitRequestPool;

        public ContinueRunSystem(RunSaveService runSaveService, RunEntityIndex runEntityIndex)
        {
            _runSaveService = runSaveService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ContinueRunRequest>().End();
            _requestPool = world.GetPool<ContinueRunRequest>();
            _runPool = world.GetPool<RunComponent>();
            _locationPool = world.GetPool<CurrentLocationComponent>();
            _levelPool = world.GetPool<CurrentLevelComponent>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _playerBaseHealthPool = world.GetPool<PlayerBaseHealthComponent>();
            _runStatusPool = world.GetPool<RunStatusComponent>();
            _runStartedPool = world.GetPool<RunStartedEvent>();
            _goldChangedPool = world.GetPool<GoldChangedEvent>();
            _registerOwnedUnitRequestPool = world.GetPool<RegisterOwnedUnitRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                var dto = _runSaveService.Load();
                if (!dto.HasActiveRun)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var runEntity = GetOrCreateRunEntity(world);
                _locationPool.GetOrAdd(runEntity).LocationId = dto.LocationId;
                _levelPool.GetOrAdd(runEntity).LevelIndex = dto.LevelIndex;
                _levelTypePool.GetOrAdd(runEntity).Value = dto.LevelType;
                _phasePool.GetOrAdd(runEntity).Value = dto.PhaseType;
                _goldPool.GetOrAdd(runEntity).Value = dto.Gold;
                _playerBaseHealthPool.GetOrAdd(runEntity).Value = dto.PlayerBaseHealth;
                _runStatusPool.GetOrAdd(runEntity).Value = Enums.RunStatus.InProgress;

                if (dto.OwnedUnits != null)
                {
                    foreach (var ownedUnit in dto.OwnedUnits)
                    {
                        var registerEntity = world.NewEntity();
                        ref var registerRequest = ref _registerOwnedUnitRequestPool.Add(registerEntity);
                        registerRequest.RuntimeId = ownedUnit.RuntimeId;
                        registerRequest.DisplayName = ownedUnit.DisplayName;
                        registerRequest.Level = ownedUnit.Level;
                        registerRequest.UnitTypeId = ownedUnit.UnitTypeId;
                        registerRequest.Attack = ownedUnit.Attack;
                        registerRequest.Health = ownedUnit.Health;
                        registerRequest.ManaCost = ownedUnit.ManaCost;
                        registerRequest.PassiveAbilityId = ownedUnit.PassiveAbilityId;
                        registerRequest.UpgradeCount = ownedUnit.UpgradeCount;
                    }
                }

                _runStartedPool.Add(world.NewEntity());
                _goldChangedPool.Add(world.NewEntity()).Value = dto.Gold;
                world.DelEntity(requestEntity);
            }
        }
        
        private int GetOrCreateRunEntity(EcsWorld world)
        {
            if (_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return runEntity;
            }

            runEntity = world.NewEntity();
            _runPool.Add(runEntity);
            _runEntityIndex.SetRunEntity(runEntity);
            return runEntity;
        }
    }
}