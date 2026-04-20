using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class AccrueSignalPurchaseIncomeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly UnitConfigService _unitConfigService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _ownedFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<SignalPurchasePhaseStateComponent> _signalPurchasePool;
        private EcsPool<UnitTypeIdComponent> _unitTypeIdPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;

        public AccrueSignalPurchaseIncomeSystem(
            GameSettingsService gameSettingsService,
            UnitConfigService unitConfigService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _unitConfigService = unitConfigService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _ownedFilter = world.Filter<OwnedUnitComponent>().Inc<UnitTypeIdComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _signalPurchasePool = world.GetPool<SignalPurchasePhaseStateComponent>();
            _unitTypeIdPool = world.GetPool<UnitTypeIdComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) ||
                _phasePool.Get(runEntity).Value != Enums.PhaseType.SignalPurchasePhase)
            {
                return;
            }

            ref var state = ref _signalPurchasePool.Get(runEntity);
            if (state.IsGeneratorBroken)
            {
                return;
            }

            state.PassiveIncomeTickElapsed += Time.deltaTime;
            var tickDuration = Mathf.Max(0.01f, _gameSettingsService.GetBattleTickDuration());
            if (state.PassiveIncomeTickElapsed < tickDuration)
            {
                return;
            }

            var tickCount = Mathf.FloorToInt(state.PassiveIncomeTickElapsed / tickDuration);
            state.PassiveIncomeTickElapsed -= tickCount * tickDuration;

            var incomePerTick = 0;
            foreach (var entity in _ownedFilter)
            {
                var unitType = _unitConfigService.GetUnit(_unitTypeIdPool.Get(entity).Value);
                if (unitType != null)
                {
                    incomePerTick += Mathf.Max(0, unitType.PassiveIncomePerTick);
                }
            }

            if (incomePerTick <= 0 || tickCount <= 0)
            {
                return;
            }

            ref var gold = ref _goldPool.Get(runEntity);
            gold.Value += incomePerTick * tickCount;
            _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;
        }
    }
}
