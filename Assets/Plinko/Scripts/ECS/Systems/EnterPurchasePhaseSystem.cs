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
    public sealed class EnterPurchasePhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _phaseChangedFilter;
        private EcsPool<PhaseChangedEvent> _phaseChangedPool;
        private EcsPool<PurchasePhaseStateComponent> _purchasePhaseStatePool;
        private EcsPool<GenerateUnitShopOffersRequest> _generateOffersRequestPool;
        private EcsPool<PurchasePhaseEnteredEvent> _purchasePhaseEnteredEventPool;

        public EnterPurchasePhaseSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _phaseChangedPool = world.GetPool<PhaseChangedEvent>();
            _purchasePhaseStatePool = world.GetPool<PurchasePhaseStateComponent>();
            _generateOffersRequestPool = world.GetPool<GenerateUnitShopOffersRequest>();
            _purchasePhaseEnteredEventPool = world.GetPool<PurchasePhaseEnteredEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }
            
            foreach (var eventEntity in _phaseChangedFilter)
            {
                if (_phaseChangedPool.Get(eventEntity).Value == Enums.PhaseType.PurchasePhase)
                {
                    ref var purchaseState = ref _purchasePhaseStatePool.GetOrAdd(runEntity);
                    purchaseState.RerollCount = 0;
                    purchaseState.IsReady = true;

                    _purchasePhaseEnteredEventPool.Add(world.NewEntity());
                    ref var generateRequest = ref _generateOffersRequestPool.Add(world.NewEntity());
                    generateRequest.OfferCount = _gameSettingsService.GetUnitShopOfferCount();
                    generateRequest.Offset = 0;
                }
            }
        }
    }
}