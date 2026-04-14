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
    public sealed class EnterFieldUpgradePhaseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _phaseChangedFilter;
        private EcsPool<PhaseChangedEvent> _phaseChangedPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<FieldUpgradePhaseEnteredEvent> _fieldUpgradeEnteredEventPool;
        private EcsPool<GeneratePinShopOffersRequest> _generateOffersRequestPool;
        private EcsPool<BoardSlotSelectionChangedEvent> _boardSlotSelectionChangedEventPool;

        public EnterFieldUpgradePhaseSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _phaseChangedPool = world.GetPool<PhaseChangedEvent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _fieldUpgradeEnteredEventPool = world.GetPool<FieldUpgradePhaseEnteredEvent>();
            _generateOffersRequestPool = world.GetPool<GeneratePinShopOffersRequest>();
            _boardSlotSelectionChangedEventPool = world.GetPool<BoardSlotSelectionChangedEvent>();
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
                if (_phaseChangedPool.Get(eventEntity).Value != Enums.PhaseType.FieldUpgradePhase)
                {
                    continue;
                }

                ref var state = ref _fieldUpgradeStatePool.GetOrAdd(runEntity);
                state.RerollCount = 0;
                state.SelectedSlotIndex = -1;

                _fieldUpgradeEnteredEventPool.Add(world.NewEntity());
                ref var generateRequest = ref _generateOffersRequestPool.Add(world.NewEntity());
                generateRequest.OfferCount = _gameSettingsService.GetPinShopOfferCount();
                generateRequest.Offset = 0;
                _boardSlotSelectionChangedEventPool.Add(world.NewEntity()).SlotIndex = -1;
            }
        }
    }
}