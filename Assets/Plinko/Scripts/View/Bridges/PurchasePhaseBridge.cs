using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class PurchasePhaseBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestBuyUnit(int offerId) => _world.GetPool<BuyUnitRequest>().Add(_world.NewEntity()).OfferId = offerId;
        public void RequestRerollShop() => _world.GetPool<RerollUnitShopRequest>().Add(_world.NewEntity());
        public void RequestStartBattle() => _world.GetPool<StartBattleRequest>().Add(_world.NewEntity());
    }
}