using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class GenerateHandSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;
        private readonly GameSettingsService _gameSettingsService;

        private EcsFilter _requestFilter;
        private EcsFilter _existingHandFilter;
        private EcsFilter _ownedUnitFilter;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<HandStateComponent> _handStatePool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<OwnedUnitComponent> _ownedUnitPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<HandGeneratedEvent> _handGeneratedEventPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;

        public GenerateHandSystem(RunEntityIndex runEntityIndex, GameSettingsService gameSettingsService)
        {
            _runEntityIndex = runEntityIndex;
            _gameSettingsService = gameSettingsService;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<GenerateHandRequest>().End();
            _existingHandFilter = world.Filter<HandCardComponent>().End();
            _ownedUnitFilter = world.Filter<OwnedUnitComponent>().Inc<UnitTypeIdComponent>().Inc<UnitStatsComponent>().Inc<UnitManaCostComponent>().Inc<UnitDisplayNameComponent>().Inc<UnitLevelComponent>().End();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _handStatePool = world.GetPool<HandStateComponent>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _ownedUnitPool = world.GetPool<OwnedUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _handGeneratedEventPool = world.GetPool<HandGeneratedEvent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
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
                foreach (var handCardEntity in _existingHandFilter)
                {
                    world.DelEntity(handCardEntity);
                }

                var ownedUnitEntities = new List<int>();
                foreach (var ownedUnitEntity in _ownedUnitFilter)
                {
                    ownedUnitEntities.Add(ownedUnitEntity);
                }

                var handSize = Mathf.Max(1, _gameSettingsService.GetHandSize());
                var createdCount = 0;
                if (ownedUnitEntities.Count > 0)
                {
                    for (var index = 0; index < handSize; index++)
                    {
                        var sourceEntity = ownedUnitEntities[Random.Range(0, ownedUnitEntities.Count)];
                        var handCardEntity = world.NewEntity();
                        _handCardPool.Add(handCardEntity).CardId = index + 1;
                        _handCardOwnerPool.Add(handCardEntity).RuntimeId = _ownedUnitPool.Get(sourceEntity).RuntimeId;
                        _unitTypePool.Add(handCardEntity).Value = _unitTypePool.Get(sourceEntity).Value;
                        _displayNamePool.Add(handCardEntity).Value = _displayNamePool.Get(sourceEntity).Value;
                        _levelPool.Add(handCardEntity).Value = _levelPool.Get(sourceEntity).Value;

                        ref var handStats = ref _unitStatsPool.Add(handCardEntity);
                        handStats.Attack = _unitStatsPool.Get(sourceEntity).Attack;
                        handStats.Health = _unitStatsPool.Get(sourceEntity).Health;

                        _manaCostPool.Add(handCardEntity).Value = _manaCostPool.Get(sourceEntity).Value;
                        createdCount++;
                    }
                }

                _handStatePool.GetOrAdd(runEntity).CardCount = createdCount;
                _manaPool.GetOrAdd(runEntity).Value = _gameSettingsService.GetManaPerTurn();
                _manaChangedEventPool.Add(world.NewEntity()).Value = _manaPool.Get(runEntity).Value;
                _handGeneratedEventPool.Add(world.NewEntity());
                world.DelEntity(requestEntity);
            }
        }
    }
}