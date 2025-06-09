using System;
using Game.Components;
using Game.Services;
using Leopotam.EcsLite;
using UnityEngine;
using Utils;

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

            var offlineSeconds = (DateTime.UtcNow - appState.LastSaveTimestamp).TotalSeconds;
            if (offlineSeconds <= 0) return;

            var businessPool = world.GetPool<BusinessComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();
            var upgradePool = world.GetPool<UpgradeComponent>();
            var balancePool = world.GetPool<BalanceComponent>();

            foreach (var entity in world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().End())
            {
                ref var biz = ref businessPool.Get(entity);
                if (biz.Level <= 0) continue;

                ref var progress = ref progressPool.Get(entity);
                var delay = progress.Delay;
                if (delay <= 0.01f) continue;

                var cycles = (int)(offlineSeconds / delay);
                if (cycles <= 0) continue;

                progress.Progress = (float)(offlineSeconds % delay) / delay;

                var baseIncome = ConfigService.Instance.GetBaseIncome(biz.BusinessId);
                var multiplier = EcsBusinessUtils.CalculateTotalUpgradeMultiplier(world, upgradePool, biz.BusinessId);
                var rawIncome = biz.Level * baseIncome * (1f + multiplier);
                var income = (long)Mathf.Round(rawIncome);

                foreach (var balEntity in world.Filter<BalanceComponent>().End())
                {
                    ref var balance = ref balancePool.Get(balEntity);
                    balance.Value += income * cycles;
                }
                
                Debug.Log($"[OfflineIncome] BizId: {biz.BusinessId}, Level: {biz.Level}, BaseIncome: {baseIncome}, Multiplier: {multiplier}, Cycles: {cycles}, RawIncome: {rawIncome}, FinalIncomePerCycle: {income}, TotalIncome: {income * cycles}");
            }
        }
    }
}