using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class SignalPurchaseBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world) => _world = world;
        public void RequestBuyUnit(int offerId) => _world.GetPool<BuySignalUnitRequest>().Add(_world.NewEntity()).OfferId = offerId;
        public void RequestRerollShop() => _world.GetPool<RerollSignalUnitShopRequest>().Add(_world.NewEntity());
        public void RequestLaunchSignal() => _world.GetPool<LaunchSignalRequest>().Add(_world.NewEntity());
    }
}
