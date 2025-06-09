using Game.Data.Business;
using Game.Events;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game
{
    public class EcsUiEventBridge : MonoBehaviour
    {
        public static EcsUiEventBridge Instance { get; private set; }
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            Instance = this;
            _world = world;
        }

        public void SendBuyLevelEvent(BusinessId businessId)
        {
            var entity = _world.NewEntity();
            ref var e = ref _world.GetPool<BuyLevelEvent>().Add(entity);
            e.BusinessId = businessId;
        }

        public void SendUpgradeEvent(BusinessId businessId, int upgradeIndex)
        {
            var entity = _world.NewEntity();
            ref var e = ref _world.GetPool<UpgradeEvent>().Add(entity);
            e.BusinessId = businessId;
            e.UpgradeIndex = upgradeIndex;
        }
    }
}