using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class FieldUpgradePhaseBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void RequestBuyPin(int offerId)
        {
            var entity = _world.NewEntity();
            _world.GetPool<BuyPinRequest>().Add(entity).OfferId = offerId;
        }

        public void RequestSelectBoardSlot(int slotIndex)
        {
            var entity = _world.NewEntity();
            _world.GetPool<SelectBoardSlotRequest>().Add(entity).SlotIndex = slotIndex;
        }

        public void RequestReplaceBoardPin()
        {
            var entity = _world.NewEntity();
            _world.GetPool<ReplaceBoardPinRequest>().Add(entity);
        }
    }
}