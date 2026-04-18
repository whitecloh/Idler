using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class FieldUpgradeBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestBuyPin(int offerId) => _world.GetPool<BuyPinRequest>().Add(_world.NewEntity()).OfferId = offerId;
        public void RequestRerollShop() => _world.GetPool<RerollPinShopRequest>().Add(_world.NewEntity());
        public void RequestSelectBoardSlot(int slotIndex) => _world.GetPool<SelectBoardSlotRequest>().Add(_world.NewEntity()).SlotIndex = slotIndex;
        public void RequestCancelBoardSlotSelection(int slotIndex) => _world.GetPool<SelectBoardSlotRequest>().Add(_world.NewEntity()).SlotIndex = slotIndex;
        public void RequestReplaceBoardPin() => _world.GetPool<ReplaceBoardPinRequest>().Add(_world.NewEntity());
    }
}
