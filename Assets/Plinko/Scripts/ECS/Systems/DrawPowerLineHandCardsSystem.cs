using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class DrawPowerLineHandCardsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleRuntimeService _battleRuntimeService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<DrawPowerLineHandCardsRequest> _requestPool;
        private EcsPool<CurrentLevelTypeComponent> _levelTypePool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;

        public DrawPowerLineHandCardsSystem(
            BattleRuntimeService battleRuntimeService,
            RunEntityIndex runEntityIndex)
        {
            _battleRuntimeService = battleRuntimeService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<DrawPowerLineHandCardsRequest>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _requestPool = world.GetPool<DrawPowerLineHandCardsRequest>();
            _levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                ref var request = ref _requestPool.Get(requestEntity);
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                    !_levelTypePool.Has(runEntity) ||
                    _levelTypePool.Get(runEntity).Value != Enums.LevelType.PowerLineBattle ||
                    !_phasePool.Has(runEntity) ||
                    _phasePool.Get(runEntity).Value != Enums.PhaseType.Battle ||
                    !_handStatePool.Has(runEntity) ||
                    _battleRuntimeService.CurrentPowerLineState == null)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                var state = _battleRuntimeService.CurrentPowerLineState;
                if (request.ClearExisting)
                {
                    foreach (var handCardEntity in _handCardFilter)
                    {
                        var ownedUnitRuntimeId = _handCardOwnerPool.Get(handCardEntity).OwnedUnitRuntimeId;
                        if (!state.DeckOwnedUnitRuntimeIds.Contains(ownedUnitRuntimeId))
                        {
                            state.DeckOwnedUnitRuntimeIds.Add(ownedUnitRuntimeId);
                        }
                    }

                    state.DeckOwnedUnitRuntimeIds.Sort();
                    ClearHand(world);
                    _handStatePool.Get(runEntity).CardCount = 0;
                }

                var availableOwnedRuntimeIds = new List<int>();
                foreach (var ownedEntity in _ownedUnitFilter)
                {
                    var ownedRuntimeId = _ownedUnitPool.Get(ownedEntity).RuntimeId;
                    if (state.DeckOwnedUnitRuntimeIds.Contains(ownedRuntimeId))
                    {
                        availableOwnedRuntimeIds.Add(ownedRuntimeId);
                    }
                }

                if (availableOwnedRuntimeIds.Count == 0 || request.Count <= 0)
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                ref var handState = ref _handStatePool.Get(runEntity);
                var drawCount = Mathf.Min(Mathf.Max(0, request.Count), availableOwnedRuntimeIds.Count);
                for (var index = 0; index < drawCount; index++)
                {
                    var selectionIndex = Random.Range(0, availableOwnedRuntimeIds.Count);
                    var ownerRuntimeId = availableOwnedRuntimeIds[selectionIndex];
                    availableOwnedRuntimeIds.RemoveAt(selectionIndex);
                    state.DeckOwnedUnitRuntimeIds.Remove(ownerRuntimeId);
                    var cardEntity = world.NewEntity();
                    _handCardPool.Add(cardEntity).HandCardRuntimeId = handState.NextRuntimeId++;
                    _handCardOwnerPool.Add(cardEntity).OwnedUnitRuntimeId = ownerRuntimeId;
                    handState.CardCount++;
                }

                world.DelEntity(requestEntity);
            }
        }

        private void ClearHand(EcsWorld world)
        {
            var toDelete = new List<int>();
            foreach (var entity in _handCardFilter)
            {
                toDelete.Add(entity);
            }

            for (var index = 0; index < toDelete.Count; index++)
            {
                world.DelEntity(toDelete[index]);
            }
        }
    }
}
