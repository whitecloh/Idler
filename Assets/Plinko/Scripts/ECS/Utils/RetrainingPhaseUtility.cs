using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Units;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.ECS.Utils
{
    internal static class RetrainingPhaseUtility
    {
        public static void ClearOffers(EcsWorld world)
        {
            foreach (var entity in world.Filter<RetrainingShopOfferComponent>().End())
            {
                world.DelEntity(entity);
            }
        }

        public static List<int> CollectEligibleOwnedEntities(
            EcsWorld world,
            EcsPool<OwnedUnitComponent> ownedUnitPool,
            EcsPool<RetrainingPurchasedOnLevelComponent> purchasedOnLevelPool)
        {
            var result = new List<int>();
            foreach (var entity in world.Filter<OwnedUnitComponent>().End())
            {
                if (purchasedOnLevelPool.Has(entity))
                {
                    continue;
                }

                result.Add(entity);
            }

            return result;
        }

        public static void GenerateBatch(
            EcsWorld world,
            int offerCount,
            List<int> eligibleOwnedEntities,
            UnitConfigService unitConfigService,
            EcsPool<OwnedUnitComponent> ownedUnitPool,
            EcsPool<RetrainingShopOfferComponent> retrainingOfferPool,
            EcsPool<RetrainingOfferOwnerUnitComponent> ownerUnitPool,
            EcsPool<OfferPriceComponent> pricePool,
            EcsPool<UnitTypeIdComponent> unitTypeIdPool,
            EcsPool<UnitDisplayNameComponent> displayNamePool,
            EcsPool<UnitStatsComponent> statsPool,
            EcsPool<UnitManaCostComponent> manaCostPool,
            EcsPool<PassiveAbilityIdComponent> passiveAbilityPool,
            EcsPool<UnitLevelComponent> levelPool,
            EcsPool<UpgradeCountComponent> upgradeCountPool)
        {
            ClearOffers(world);
            if (eligibleOwnedEntities == null || eligibleOwnedEntities.Count == 0 || offerCount <= 0)
            {
                return;
            }

            var shuffledEntities = new List<int>(eligibleOwnedEntities);
            Shuffle(shuffledEntities);

            var slotIndex = 0;
            for (var index = 0; index < shuffledEntities.Count && slotIndex < offerCount; index++)
            {
                var ownedEntity = shuffledEntities[index];
                var unitTypeId = unitTypeIdPool.Get(ownedEntity).Value;
                var unitType = unitConfigService.GetUnit(unitTypeId);
                if (unitType == null)
                {
                    continue;
                }

                var offerEntity = world.NewEntity();
                retrainingOfferPool.Add(offerEntity).OfferSlotIndex = slotIndex;
                ownerUnitPool.Add(offerEntity).RuntimeId = ownedUnitPool.Get(ownedEntity).RuntimeId;
                pricePool.Add(offerEntity).Value = Mathf.Max(0, unitType.ShopPrice);
                unitTypeIdPool.Add(offerEntity).Value = unitTypeId;
                displayNamePool.Add(offerEntity).Value = displayNamePool.Get(ownedEntity).Value;
                statsPool.Add(offerEntity) = statsPool.Get(ownedEntity);
                manaCostPool.Add(offerEntity).Value = manaCostPool.Get(ownedEntity).Value;
                passiveAbilityPool.Add(offerEntity).Value = passiveAbilityPool.Get(ownedEntity).Value;
                levelPool.Add(offerEntity).Value = levelPool.Get(ownedEntity).Value;
                upgradeCountPool.Add(offerEntity).Value = upgradeCountPool.Get(ownedEntity).Value;
                slotIndex++;
            }
        }

        private static void Shuffle(List<int> values)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                var swapIndex = Random.Range(0, index + 1);
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }
        }
    }
}
