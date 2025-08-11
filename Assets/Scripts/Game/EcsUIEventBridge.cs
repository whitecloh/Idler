namespace Game
{
    using Data.Business;
    using Events;
    using Leopotam.EcsLite;
    using UnityEngine;
    
    public class EcsUIEventBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void SendBuyLevelEvent(BusinessId businessId)
        {
            var entity = _world.NewEntity();
            ref var buyLevelEvent = ref _world.GetPool<BuyLevelEvent>().Add(entity);
            buyLevelEvent.BusinessId = businessId;
        }

        public void SendUpgradeEvent(BusinessId businessId, int upgradeIndex)
        {
            var entity = _world.NewEntity();
            ref var upgradeEvent = ref _world.GetPool<UpgradeEvent>().Add(entity);
            upgradeEvent.BusinessId = businessId;
            upgradeEvent.UpgradeIndex = upgradeIndex;
        }
    }
}