using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class ClampGoldSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<GoldChangedEvent> _goldChangedEventPool;

        public ClampGoldSystem(
            GameSettingsService gameSettingsService,
            RunEntityIndex runEntityIndex)
        {
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _goldChangedEventPool = world.GetPool<GoldChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!_runEntityIndex.TryGetRunEntity(out var runEntity) || !_goldPool.Has(runEntity))
            {
                return;
            }

            var maxGold = _gameSettingsService.GetMaxGold();
            ref var gold = ref _goldPool.Get(runEntity);
            var clampedValue = maxGold > 0
                ? Mathf.Clamp(gold.Value, 0, maxGold)
                : Mathf.Max(0, gold.Value);

            if (clampedValue == gold.Value)
            {
                return;
            }

            gold.Value = clampedValue;
            _goldChangedEventPool.Add(world.NewEntity()).Value = gold.Value;
        }
    }
}
