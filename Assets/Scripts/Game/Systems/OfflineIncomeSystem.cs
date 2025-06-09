using System;
using Game.Components;
using Game.Services;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Systems
{
    public class OfflineIncomeSystem : IEcsInitSystem
    {
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var appStateFilter = world.Filter<OfflineIncomeComponent>().End();
            var appStatePool = world.GetPool<OfflineIncomeComponent>();

            if (appStateFilter.GetEntitiesCount() == 0)
                return;
            
            var appEntity = appStateFilter.GetRawEntities()[0];
            ref var appState = ref appStatePool.Get(appEntity);

            var lastSave = appState.LastSaveTimestamp;
            var now = DateTime.UtcNow;

            var offlineSeconds = (now - lastSave).TotalSeconds;
            if (offlineSeconds <= 0)
                return;

            var businessFilter = world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().End();
            var businessPool = world.GetPool<BusinessComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();
            var balanceFilter = world.Filter<BalanceComponent>().End();
            var balancePool = world.GetPool<BalanceComponent>();

            foreach (var entity in businessFilter)
            {
                ref var biz = ref businessPool.Get(entity);
                if (biz.Level <= 0)
                    continue;

                ref var progress = ref progressPool.Get(entity);
                var delay = progress.Delay;
                if (delay <= 0.01f) 
                    continue;

                var cycles = (int)(offlineSeconds / delay);
                if (cycles <= 0) 
                    continue;

                var partial = (float)(offlineSeconds % delay) / delay;
                progress.Progress = partial;
                
                var totalMultiplier = 0f;
                foreach (var upgEntity in world.Filter<UpgradeComponent>().End())
                {
                    ref var upg = ref world.GetPool<UpgradeComponent>().Get(upgEntity);
                    if (upg.BusinessId == biz.BusinessId && upg.IsActive)
                        totalMultiplier += upg.Multiplier;
                }

                var baseIncome = ConfigService.Instance.GetBaseIncome(biz.BusinessId);
                var income = Mathf.RoundToInt(biz.Level * baseIncome * (1f + totalMultiplier));

                foreach (var balEntity in balanceFilter)
                {
                    ref var balance = ref balancePool.Get(balEntity);
                    balance.Value += income * cycles;
                }
            }
        }
    }
}