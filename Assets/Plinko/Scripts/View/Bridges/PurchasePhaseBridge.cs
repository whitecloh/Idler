using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class PurchasePhaseBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void RequestBuyUnit(int offerId)
        {
            var entity = _world.NewEntity();
            _world.GetPool<BuyUnitRequest>().Add(entity).OfferId = offerId;
        }

        public void RequestStartTraining()
        {
            var entity = _world.NewEntity();
            _world.GetPool<StartTrainingRequest>().Add(entity);
        }
    }
}