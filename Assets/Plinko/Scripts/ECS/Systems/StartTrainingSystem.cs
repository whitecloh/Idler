using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Requests;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class StartTrainingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _requestFilter;
        private EcsFilter _stagedPurchasedUnitFilter;
        private EcsFilter _stagedUpgradeUnitFilter;
        private EcsPool<TrainingCompletedEvent> _trainingCompletedEventPool;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _requestFilter = world.Filter<StartTrainingRequest>().End();
            _stagedPurchasedUnitFilter = world.Filter<StagedPurchasedUnitComponent>().End();
            _stagedUpgradeUnitFilter = world.Filter<StagedUpgradeUnitComponent>().End();
            _trainingCompletedEventPool = world.GetPool<TrainingCompletedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var requestEntity in _requestFilter)
            {
                if (_stagedPurchasedUnitFilter.GetEntitiesCount() > 0 || _stagedUpgradeUnitFilter.GetEntitiesCount() > 0)
                {
                    _trainingCompletedEventPool.Add(world.NewEntity());
                }

                world.DelEntity(requestEntity);
            }
        }
    }
}