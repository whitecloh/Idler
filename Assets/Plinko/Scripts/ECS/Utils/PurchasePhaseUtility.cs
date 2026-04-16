using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Units;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Utils
{
    internal static class PurchasePhaseUtility
    {
        public static LevelData GetCurrentLevelData(
            LevelConfigService levelConfigService,
            EcsPool<CurrentLocationComponent> locationPool,
            EcsPool<CurrentLevelComponent> levelPool,
            int runEntity)
        {
            return levelConfigService.GetLevel(locationPool.Get(runEntity).LocationId, levelPool.Get(runEntity).LevelIndex);
        }

        public static List<UnitTypeData> BuildUnlockedPool(UnitConfigService unitConfigService, LevelData levelData)
        {
            var result = new List<UnitTypeData>();
            var pool = unitConfigService.GetUnlockedShopPool(levelData);
            if (pool == null)
            {
                return result;
            }

            foreach (var unit in pool)
            {
                if (unit != null)
                {
                    result.Add(unit);
                }
            }

            return result;
        }

        public static void ClearOffers(EcsWorld world, ShopOfferIndex shopOfferIndex)
        {
            foreach (var entity in world.Filter<UnitShopOfferComponent>().End())
            {
                world.DelEntity(entity);
            }

            shopOfferIndex.Clear();
        }
        
        public static void GenerateFullShop(
            EcsWorld world,
            int offerCount,
            List<UnitTypeData> pool,
            WeightedRandomService weightedRandomService,
            ShopOfferIndex shopOfferIndex,
            EcsPool<UnitShopOfferComponent> offerPool,
            EcsPool<OfferPriceComponent> pricePool,
            EcsPool<ShopOfferUnitTypeIdComponent> unitTypePool)
        {
            ClearOffers(world, shopOfferIndex);
            if (pool == null || pool.Count == 0)
            {
                return;
            }

            for (var offerId = 0; offerId < offerCount; offerId++)
            {
                var unit = weightedRandomService.Roll(pool, value => value.GenerationWeight);
                if (unit == null)
                {
                    continue;
                }

                var entity = world.NewEntity();
                offerPool.Add(entity).OfferId = offerId;
                pricePool.Add(entity).Value = unit.ShopPrice;
                unitTypePool.Add(entity).Value = unit.Id;
                shopOfferIndex.Register(offerId, entity);
            }
        }
        
        public static void RefillOffer(
            List<UnitTypeData> pool,
            WeightedRandomService weightedRandomService,
            int offerEntity,
            EcsPool<OfferPriceComponent> pricePool,
            EcsPool<ShopOfferUnitTypeIdComponent> unitTypePool)
        {
            if (pool == null || pool.Count == 0)
            {
                return;
            }

            var unit = weightedRandomService.Roll(pool, value => value.GenerationWeight);
            if (unit == null)
            {
                return;
            }

            pricePool.Get(offerEntity).Value = unit.ShopPrice;
            unitTypePool.Get(offerEntity).Value = unit.Id;
        }
        
        public static int GetNextRuntimeId(EcsWorld world, EcsPool<OwnedUnitComponent> ownedUnitPool, EcsPool<StagedTraineeComponent> stagedPool)
        {
            var maxRuntimeId = 0;
            foreach (var entity in world.Filter<OwnedUnitComponent>().End())
            {
                maxRuntimeId = Mathf.Max(maxRuntimeId, ownedUnitPool.Get(entity).RuntimeId);
            }

            foreach (var entity in world.Filter<StagedTraineeComponent>().End())
            {
                maxRuntimeId = Mathf.Max(maxRuntimeId, stagedPool.Get(entity).RuntimeId);
            }

            return maxRuntimeId + 1;
        }
    }
}