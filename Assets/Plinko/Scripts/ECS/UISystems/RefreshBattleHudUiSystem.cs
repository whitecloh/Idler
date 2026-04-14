using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Controllers;

namespace Plinko.Scripts.ECS.UISystems
{
    public sealed class RefreshBattleHudUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly BattleScreenController _controller;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _manaChangedFilter;
        private EcsFilter _handGeneratedFilter;
        private EcsFilter _handClearedFilter;
        private EcsFilter _unitDeployedFilter;
        private EcsFilter _battleResolvedFilter;
        private EcsFilter _turnCompletedFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<PlayerBaseHealthComponent> _playerBaseHealthPool;
        private EcsPool<EnemyBaseHealthComponent> _enemyBaseHealthPool;
        private EcsPool<BattleStateComponent> _battleStatePool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<UnitTypeIdComponent> _unitTypePool;
        private EcsPool<UnitStatsComponent> _unitStatsPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<UnitDisplayNameComponent> _displayNamePool;
        private EcsPool<UnitLevelComponent> _levelPool;
        private EcsPool<DeployedForTurnComponent> _deployedPool;

        public RefreshBattleHudUiSystem(BattleScreenController controller, RunEntityIndex runEntityIndex)
        {
            _controller = controller;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _manaChangedFilter = world.Filter<ManaChangedEvent>().End();
            _handGeneratedFilter = world.Filter<HandGeneratedEvent>().End();
            _handClearedFilter = world.Filter<HandClearedEvent>().End();
            _unitDeployedFilter = world.Filter<UnitDeployedEvent>().End();
            _battleResolvedFilter = world.Filter<BattleResolvedEvent>().End();
            _turnCompletedFilter = world.Filter<TurnCompletedEvent>().End();
            _handCardFilter = world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().Inc<UnitTypeIdComponent>().Inc<UnitStatsComponent>().Inc<UnitManaCostComponent>().End();
            _handCardPool = world.GetPool<HandCardComponent>();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _playerBaseHealthPool = world.GetPool<PlayerBaseHealthComponent>();
            _enemyBaseHealthPool = world.GetPool<EnemyBaseHealthComponent>();
            _battleStatePool = world.GetPool<BattleStateComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _unitTypePool = world.GetPool<UnitTypeIdComponent>();
            _unitStatsPool = world.GetPool<UnitStatsComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            _levelPool = world.GetPool<UnitLevelComponent>();
            _deployedPool = world.GetPool<DeployedForTurnComponent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            if (_controller == null || !_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldRefresh = _phaseChangedFilter.GetEntitiesCount() > 0 ||
                                _manaChangedFilter.GetEntitiesCount() > 0 ||
                                _handGeneratedFilter.GetEntitiesCount() > 0 ||
                                _handClearedFilter.GetEntitiesCount() > 0 ||
                                _unitDeployedFilter.GetEntitiesCount() > 0 ||
                                _battleResolvedFilter.GetEntitiesCount() > 0 ||
                                _turnCompletedFilter.GetEntitiesCount() > 0;
            if (!shouldRefresh)
            {
                return;
            }

            var isVisible = _phasePool.Get(runEntity).Value == Enums.PhaseType.Battle;
            _controller.Show(isVisible);
            if (!isVisible)
            {
                return;
            }

            ref var battleState = ref _battleStatePool.GetOrAdd(runEntity);
            var viewData = new BattleHudViewData
            {
                CurrentMana = _manaPool.GetOrAdd(runEntity).Value,
                PlayerBaseHealth = _playerBaseHealthPool.GetOrAdd(runEntity).Value,
                EnemyBaseHealth = _enemyBaseHealthPool.GetOrAdd(runEntity).Value,
                CurrentTurn = battleState.CurrentTurn,
                IsBattleResolved = battleState.IsResolved,
                HandCards = new List<HandCardViewData>()
            };

            foreach (var handCardEntity in _handCardFilter)
            {
                viewData.HandCards.Add(new HandCardViewData
                {
                    CardId = _handCardPool.Get(handCardEntity).CardId,
                    RuntimeId = _handCardOwnerPool.Get(handCardEntity).RuntimeId,
                    DisplayName = _displayNamePool.Get(handCardEntity).Value,
                    Level = _levelPool.Get(handCardEntity).Value,
                    UnitTypeId = _unitTypePool.Get(handCardEntity).Value,
                    Attack = _unitStatsPool.Get(handCardEntity).Attack,
                    Health = _unitStatsPool.Get(handCardEntity).Health,
                    ManaCost = _manaCostPool.Get(handCardEntity).Value,
                    IsDeployed = _deployedPool.Has(handCardEntity)
                });
            }

            _controller.Refresh(viewData);
        }
    }
}