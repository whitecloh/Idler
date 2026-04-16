using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ReturnToMenuSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunSaveService _runSaveService;
        private readonly PlinkoRuntimeService _plinkoRuntimeService;
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;
        private readonly OwnedUnitIndex _ownedUnitIndex;
        private readonly ShopOfferIndex _shopOfferIndex;
        private readonly PinShopOfferIndex _pinShopOfferIndex;
        private readonly InstalledPinIndex _installedPinIndex;

        private EcsFilter _requestFilter;
        private EcsPool<ReturnToMenuRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<PhaseChangedEvent> _phaseChangedEventPool;

        public ReturnToMenuSystem(
            RunSaveService runSaveService,
            PlinkoRuntimeService plinkoRuntimeService,
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex,
            ShopOfferIndex shopOfferIndex,
            PinShopOfferIndex pinShopOfferIndex,
            InstalledPinIndex installedPinIndex)
        {
            _runSaveService = runSaveService;
            _plinkoRuntimeService = plinkoRuntimeService;
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
            _ownedUnitIndex = ownedUnitIndex;
            _shopOfferIndex = shopOfferIndex;
            _pinShopOfferIndex = pinShopOfferIndex;
            _installedPinIndex = installedPinIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<ReturnToMenuRequest>().End();
            _requestPool = world.GetPool<ReturnToMenuRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _phaseChangedEventPool = world.GetPool<PhaseChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                _requestPool.Get(requestEntity);

                if (_runEntityIndex.TryGetRunEntity(out var runEntity) &&
                    _phasePool.Get(runEntity).Value != Enums.PhaseType.Result)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                RuntimeEntityCleanup.ClearForNewRun(
                    world,
                    _runEntityIndex,
                    _ownedUnitIndex,
                    _shopOfferIndex,
                    _pinShopOfferIndex,
                    _installedPinIndex);

                _plinkoRuntimeService.Clear();
                _battleRuntimeService.Clear();
                _runSaveService.Clear();
                _phaseChangedEventPool.Add(world.NewEntity()).Value = Enums.PhaseType.MainMenu;
                world.DelEntity(requestEntity);
            }
        }
    }
}
