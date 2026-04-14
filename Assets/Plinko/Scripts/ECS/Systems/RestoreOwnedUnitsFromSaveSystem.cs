using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class RestoreOwnedUnitsFromSaveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private bool _isRestored;

        private EcsPool<RegisterOwnedUnitRequest> _registerOwnedUnitRequestPool;

        public RestoreOwnedUnitsFromSaveSystem(RunSaveService runSaveService)
        {
            _runSaveService = runSaveService;
        }

        public void Init(IEcsSystems systems)
        {
            _registerOwnedUnitRequestPool = systems.GetWorld().GetPool<RegisterOwnedUnitRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_isRestored)
            {
                return;
            }

            var world = systems.GetWorld();
            var dto = _runSaveService.Load();
            if (dto.OwnedUnits != null)
            {
                foreach (var ownedUnit in dto.OwnedUnits)
                {
                    var entity = world.NewEntity();
                    ref var request = ref _registerOwnedUnitRequestPool.Add(entity);
                    request.RuntimeId = ownedUnit.RuntimeId;
                    request.UnitTypeId = ownedUnit.UnitTypeId;
                    request.Attack = ownedUnit.Attack;
                    request.Health = ownedUnit.Health;
                    request.ManaCost = ownedUnit.ManaCost;
                    request.PassiveAbilityId = ownedUnit.PassiveAbilityId;
                    request.UpgradeCount = ownedUnit.UpgradeCount;
                }
            }

            _isRestored = true;
        }
    }
}