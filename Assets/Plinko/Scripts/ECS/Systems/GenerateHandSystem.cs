using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class GenerateHandSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<GenerateHandRequest> _requestPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<HandGeneratedEvent> _handGeneratedEventPool;
        private EcsPool<SaveRunRequest> _saveRunRequestPool;

        public GenerateHandSystem(GameSettingsService gameSettingsService, RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<GenerateHandRequest>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().End();
            _handCardFilter = world.Filter<HandCardComponent>().End();
            _requestPool = world.GetPool<GenerateHandRequest>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _handGeneratedEventPool = world.GetPool<HandGeneratedEvent>();
            _saveRunRequestPool = world.GetPool<SaveRunRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                if (!_runEntityIndex.TryGetRunEntity(out var runEntity))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (!IsAllowedPhase(_phasePool.Get(runEntity).Value))
                {
                    world.DelEntity(requestEntity);
                    continue;
                }

                if (_battleStatePool.Has(runEntity))
                {
                    ref var battleState = ref _battleStatePool.Get(runEntity);
                    if (!battleState.IsPlayerTurnActive || battleState.HasGeneratedHandThisTurn)
                    {
                        world.DelEntity(requestEntity);
                        continue;
                    }
                }

                if (!_handStatePool.Has(runEntity))
                {
                    _handStatePool.Add(runEntity) = new HandStateComponent
                    {
                        CardCount = 0,
                        NextRuntimeId = 1
                    };
                }

                ref var handState = ref _handStatePool.Get(runEntity);
                var ownedRuntimeIds = new List<int>();
                foreach (var ownedEntity in _ownedUnitFilter)
                {
                    ownedRuntimeIds.Add(_ownedUnitPool.Get(ownedEntity).RuntimeId);
                }

                ClearActiveHand(world);

                var generatedCount = 0;
                var handSize = Mathf.Max(0, _gameSettingsService.GetHandSize());
                if (ownedRuntimeIds.Count > 0)
                {
                    for (var index = 0; index < handSize; index++)
                    {
                        var ownerRuntimeId = ownedRuntimeIds[Random.Range(0, ownedRuntimeIds.Count)];
                        var cardEntity = world.NewEntity();
                        _handCardPool.Add(cardEntity).HandCardRuntimeId = handState.NextRuntimeId++;
                        _handCardOwnerPool.Add(cardEntity).OwnedUnitRuntimeId = ownerRuntimeId;
                        generatedCount++;
                    }
                }

                handState.CardCount = generatedCount;
                if (_battleStatePool.Has(runEntity))
                {
                    _battleStatePool.Get(runEntity).HasGeneratedHandThisTurn = true;
                }
                _handGeneratedEventPool.Add(world.NewEntity());
                _saveRunRequestPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }

        private void ClearActiveHand(EcsWorld world)
        {
            var entitiesToDelete = new List<int>();
            foreach (var handCardEntity in _handCardFilter)
            {
                entitiesToDelete.Add(handCardEntity);
            }

            foreach (var handCardEntity in entitiesToDelete)
            {
                world.DelEntity(handCardEntity);
            }
        }

        private static bool IsAllowedPhase(Enums.PhaseType phase)
        {
            return phase == Enums.PhaseType.BattlePreparation;
        }
    }
}
