using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.ECS.Utils;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class DeployUnitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _requestFilter;
        private EcsFilter _handCardFilter;
        private EcsPool<DeployUnitRequest> _requestPool;
        private EcsPool<HandCardComponent> _handCardPool;
        private EcsPool<HandCardOwnerUnitComponent> _handCardOwnerPool;
        private EcsPool<UnitManaCostComponent> _manaCostPool;
        private EcsPool<DeployedForTurnComponent> _deployedPool;
        private EcsPool<CurrentManaComponent> _manaPool;
        private EcsPool<ManaChangedEvent> _manaChangedEventPool;
        private EcsPool<UnitDeployedEvent> _unitDeployedEventPool;

        public DeployUnitSystem(RunEntityIndex runEntityIndex)
        {
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<DeployUnitRequest>().End();
            _handCardFilter = world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().End();
            _requestPool = world.GetPool<DeployUnitRequest>();
            _handCardPool = world.GetPool<HandCardComponent>();
            _handCardOwnerPool = world.GetPool<HandCardOwnerUnitComponent>();
            _manaCostPool = world.GetPool<UnitManaCostComponent>();
            _deployedPool = world.GetPool<DeployedForTurnComponent>();
            _manaPool = world.GetPool<CurrentManaComponent>();
            _manaChangedEventPool = world.GetPool<ManaChangedEvent>();
            _unitDeployedEventPool = world.GetPool<UnitDeployedEvent>();
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
                var cardId = _requestPool.Get(requestEntity).CardId;
                foreach (var handCardEntity in _handCardFilter)
                {
                    if (_handCardPool.Get(handCardEntity).CardId != cardId)
                    {
                        continue;
                    }

                    if (_deployedPool.Has(handCardEntity))
                    {
                        break;
                    }

                    var cost = _manaCostPool.Get(handCardEntity).Value;
                    ref var mana = ref _manaPool.GetOrAdd(runEntity);
                    if (mana.Value < cost)
                    {
                        break;
                    }

                    mana.Value -= cost;
                    _deployedPool.Add(handCardEntity);
                    _manaChangedEventPool.Add(world.NewEntity()).Value = mana.Value;
                    _unitDeployedEventPool.Add(world.NewEntity()).RuntimeId = _handCardOwnerPool.Get(handCardEntity).RuntimeId;
                    break;
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}