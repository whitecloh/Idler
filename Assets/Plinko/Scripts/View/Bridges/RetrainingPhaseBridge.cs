using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class RetrainingPhaseBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestRerollShop() => _world.GetPool<RerollRetrainingShopRequest>().Add(_world.NewEntity());
        public void RequestBuyBatch() => _world.GetPool<BuyRetrainingBatchRequest>().Add(_world.NewEntity());
    }
}
