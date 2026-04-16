using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Indexes;

namespace Plinko.Scripts.ECS.Systems
{
    internal static class RuntimeEntityCleanup
    {
        public static void ClearForNewRun(
            EcsWorld world,
            RunEntityIndex runEntityIndex,
            OwnedUnitIndex ownedUnitIndex,
            ShopOfferIndex shopOfferIndex,
            PinShopOfferIndex pinShopOfferIndex,
            InstalledPinIndex installedPinIndex)
        {
            foreach (var entity in world.Filter<RunComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<OwnedUnitComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<InstalledPinComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<UnitShopOfferComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<PinShopOfferComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<HandCardComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<DeployedForTurnComponent>().End())
            {
                world.DelEntity(entity);
            }

            foreach (var entity in world.Filter<StagedTraineeComponent>().End())
            {
                world.DelEntity(entity);
            }

            runEntityIndex.Clear();
            ownedUnitIndex.Clear();
            shopOfferIndex.Clear();
            pinShopOfferIndex.Clear();
            installedPinIndex.Clear();
        }
    }
}
